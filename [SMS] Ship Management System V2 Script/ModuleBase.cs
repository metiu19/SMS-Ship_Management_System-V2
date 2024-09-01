using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class ModuleBase : IModule
        {
            private readonly Program _program;
            private readonly SMSErrorsManager _errsMngr;
            private readonly ScreenLogger _logger;
            private readonly MyIni _configs;
            private readonly IModuleType _type;
            private readonly List<Subsystem> _subsystems = new List<Subsystem>();
            private readonly List<SequenceStep> _startupSteps = new List<SequenceStep>();
            private readonly List<SequenceStep> _shutdownSteps = new List<SequenceStep>();
            private int _currentStep = 0;
            private bool _defaultState;
            private bool _supportStandby;
            private double _cooldownDelay;
            private bool _checkTime = false;
            private double _delayTargetTime = 0;
            private double DelayTargetTime
            {
                get { return _delayTargetTime; }
                set
                {
                    _delayTargetTime = value;
                    _checkTime = true;
                }
            }

            public string Id { get; }
            public string Name { get; private set; }
            public ModuleSubtype Subtype { get; }
            public ModuleStates State { get; private set; }



            public ModuleBase(Program program, string id, IModuleType type, ModuleSubtype subtype, MyIni iniConfigs)
            {
                _program = program;
                _errsMngr = _program.ErrsMngr;
                _logger = _program.Logger;
                _configs = iniConfigs;
                _type = type;
                Id = id;
                Subtype = subtype;
            }

            public bool Init()
            {
                _logger.LogInfo($"Module '{Id}' Init Called");

                // Default State
                _defaultState = _configs.Get(Id, "Default State").ToBoolean();
                if (_defaultState)
                    State = ModuleStates.Online;
                else
                    State = ModuleStates.Offline;
                _logger.LogDebug($"Default State = {State}");


                // Custom Name
                Name = _configs.Get(Id, "Name").ToString(Id);
                _logger.LogDebug($"Name = '{Name}'");


                // Standby
                _supportStandby = _configs.Get(Id, "Standby").ToBoolean();
                _logger.LogDebug($"Standby support: {_supportStandby}");


                // Cooldown 
                _cooldownDelay = _configs.Get(Id, "Cooldown Delay").ToDouble(double.NaN);
                if (double.IsNaN(_cooldownDelay))
                {
                    _logger.LogError("Couldn't parse Cooldown Delay");
                    _errsMngr.AddIniMissingKey(_program.Me.CustomName, Id, "Cooldown Delay");
                    _errsMngr.AddErrorDescription($"For module '{Id}'");
                    return false;
                }
                _logger.LogDebug($"Cooldown Delay = {_cooldownDelay}");


                // Subsystems
                string[] subsystems = _configs.Get(Id, "Subsystems").ToString().Split('\n');
                if (subsystems[0] == "")
                {
                    _logger.LogError("Couldn't parse Subsystems");
                    _errsMngr.AddIniMissingKey(_program.Me.CustomName, Id, "Subsystems");
                    _errsMngr.AddErrorDescription($"For module '{Id}'");
                    return false;
                }
                _logger.LogDebug($"Possible subsystems: {subsystems.Length}");

                Subsystem subsys;
                foreach (var item in subsystems.Select((value, i) => new { i, value }))
                {
                    if (Subsystem.TryParse(item.value, out subsys))
                    {
                        _logger.LogDebug($"Found subsystem '{subsys.Name}'");
                        _subsystems.Add(subsys);
                    }
                    else
                    {
                        _logger.LogError($"Coudln't parse subsystem {item.i + 1}");
                        _errsMngr.AddSubsystemParseError(Id, item.i);
                    }
                }
                _logger.LogDebug($"Actual subsystems: {_subsystems.Count}");


                // Sequences
                string[] steps;
                SequenceStep step;

                // Startup Sequence
                steps = _configs.Get(Id, "Startup Sequence").ToString().Split('\n');
                if (string.IsNullOrEmpty(steps[0]))
                {
                    _logger.LogError("Couldn't parse Startup Sequence");
                    _errsMngr.AddIniMissingKey(_program.Me.CustomName, Id, "Startup Sequence");
                    _errsMngr.AddErrorDescription($"For module '{Id}'");
                    return false;
                }
                _logger.LogDebug($"Possible startup steps: {steps.Length}");

                foreach (var item in steps.Select((value, i) => new { i, value }))
                {
                    if (!SequenceStep.TryParse(item.value, out step))
                    {
                        _logger.LogError($"Couldn't parse startup step {item.i + 1}");
                        _errsMngr.AddSequenceStepParseError(Id, "Startup", item.i);
                        continue;
                    }

                    if (_subsystems.Find(s => s.Name == step.Name) == null)
                    {
                        _logger.LogError($"Invalid step name '{step.Name}', no matching subsystem");
                        _errsMngr.AddSequenceStepInvalidError(Id, "Startup", step.Name);
                        continue;
                    }

                    _logger.LogDebug($"Found step '{step.Name}'");
                    _startupSteps.Add(step);
                }
                _logger.LogDebug($"Actual startup steps: {_startupSteps.Count}");

                // Shutdown Sequence
                steps = _configs.Get(Id, "Shutdown Sequence").ToString().Split('\n');
                if (string.IsNullOrEmpty(steps[0]))
                {
                    _logger.LogError("Couldn't parse Shutdown Sequence");
                    _errsMngr.AddIniMissingKey(_program.Me.CustomName, Id, "Shutdown Sequence");
                    _errsMngr.AddErrorDescription($"For module '{Id}'");
                    return false;
                }
                _logger.LogDebug($"Possible shutdown steps: {steps.Length}");

                foreach (var item in steps.Select((value, i) => new { i, value }))
                {
                    if (!SequenceStep.TryParse(item.value, out step))
                    {
                        _logger.LogError($"Couldn't parse startup step {item.i + 1}");
                        _errsMngr.AddSequenceStepParseError(Id, "Shutdown", item.i);
                        continue;
                    }

                    if (_subsystems.Find(s => s.Name == step.Name) == null)
                    {
                        _logger.LogError($"Invalid step name '{step.Name}', no matching subsystem");
                        _errsMngr.AddSequenceStepInvalidError(Id, "Shutdown", step.Name);
                        continue;
                    }

                    _logger.LogDebug($"Found step '{step.Name}'");
                    _shutdownSteps.Add(step);
                }
                _logger.LogDebug($"Actual shutdown steps: {_shutdownSteps.Count}");
                return true;
            }



            public bool CheckState()
            {
                if (_checkTime && _program.Time > DelayTargetTime && (State & ModuleStates.Transition) != ModuleStates.None)
                {
                    _logger.LogDebug($"Checking sequences  |  Module '{Id}'");
                    _checkTime = false;

                    SequenceStep step;
                    Subsystem subsys;
                    if (State == ModuleStates.BootingUp)
                    {
                        step = _startupSteps[_currentStep];
                        subsys = _subsystems.Find(s => s.Name == step.Name);

                        if (step.State == subsys.State)
                        {
                            _currentStep++;
                            _logger.LogInfo($"Correct step executed, advancing in Startup Sequence  |  Module '{Id}'");

                            if (_currentStep >= _startupSteps.Count)
                            {
                                _currentStep = 0;
                                State = ModuleStates.Online;
                                _logger.LogInfo($"Sequence completed, changing state to Online  |  Module '{Id}'");
                            }
                        }
                        else
                        {
                            _logger.LogError($"Sequence not respected, changing state to Error  |  Module '{Id}'");
                            State = ModuleStates.Error;
                        }
                    }
                    else
                    {
                        step = _shutdownSteps[_currentStep];
                        subsys = _subsystems.Find(s => s.Name == step.Name);

                        if (step.State == subsys.State)
                        {
                            _currentStep++;
                            _logger.LogInfo($"Correct step executed, advancing in Shutdown Sequence  |  Module '{Id}'");

                            if (_currentStep >= _shutdownSteps.Count)
                            {
                                _currentStep = 0;
                                State = ModuleStates.Offline;
                                _delayTargetTime = _program.Time + _cooldownDelay;
                                _logger.LogInfo($"Sequence completed, changing state to Offline  |  Module '{Id}'");
                            }
                        }
                        else
                        {
                            _logger.LogError($"Sequence not respected, changing state to Error  |  Module '{Id}'");
                            State = ModuleStates.Error;
                        }
                    }
                }

                if (!_type.NeedsStateChange(State.ToBool()))
                    return false;

                _logger.LogDebug($"Module '{Id}' forcing state!");
                _type.ForceState(State.ToBool());
                return true;
            }

            public int ResetErrorState()
            {
                if (State != ModuleStates.Error)
                {
                    _logger.LogWarning($"Action not available in current state  |  Module '{Id}'");
                    return 1;
                }

                if (_program.Time < DelayTargetTime)
                {
                    _logger.LogWarning($"Action currently not available, try later!   Module'{Id}'");
                    return 2;
                }

                _subsystems.ForEach(s => s.State = s.DefaultState);
                State = _defaultState ? ModuleStates.Online : ModuleStates.Offline;
                _delayTargetTime = _program.Time + _cooldownDelay;
                _logger.LogInfo($"Error state fixed, module restored to default state '{State}'  |  Module '{Id}'");
                return 0;
            }

            public int ToggleState()
            {
                if ((State & (ModuleStates.Offline | ModuleStates.Active)) == ModuleStates.None)
                {
                    _logger.LogWarning($"Action not available in current state  |  Module '{Id}'");
                    return 1;
                }

                if (_program.Time < DelayTargetTime)
                {
                    _logger.LogWarning($"Action currently not available, try later  |  Module'{Id}'");
                    return 2;
                }

                if ((State & ModuleStates.Active) != ModuleStates.None)
                    State = ModuleStates.ShuttingDown;
                else if (State == ModuleStates.Offline)
                    State = ModuleStates.BootingUp;

                _logger.LogInfo($"State set to '{State}'  |  Module '{Id}'");
                return 0;
            }

            public int SetState(ModuleStates state)
            {
                if ((State & (ModuleStates.Offline | ModuleStates.Active)) == ModuleStates.None)
                {
                    _logger.LogWarning($"Action not available in current state  |  Module '{Id}'");
                    return 1;
                }

                if (_program.Time < DelayTargetTime)
                {
                    _logger.LogWarning($"Action currently not available, try later  |  Module'{Id}'");
                    return 2;
                }

                if (state == ModuleStates.Online)
                {
                    if (State != ModuleStates.Offline)
                    {
                        _logger.LogWarning($"Requested state '{state}' not available in current state '{State}'  |  Module '{Id}'");
                        return 3;
                    }
                    State = ModuleStates.BootingUp;
                }
                else if (state == ModuleStates.Offline)
                {
                    if ((State & ModuleStates.Active) == ModuleStates.None)
                    {
                        _logger.LogWarning($"Requested state '{state}' not available in current state '{State}'  |  Module '{Id}'");
                        return 3;
                    }
                    State = ModuleStates.ShuttingDown;
                }
                else
                {
                    _logger.LogWarning($"The requested state '{state} can't be manually forced  |  Module '{Id}'");
                    return 4;
                }

                _logger.LogInfo($"State set to '{State}'  |  Module '{Id}'");
                return 0;
            }

            public int Standby()
            {
                if (!_supportStandby)
                {
                    _logger.LogWarning($"Standby not supported  |  Module '{Id}'");
                    return 1;
                }

                if ((State & ModuleStates.Active) == ModuleStates.None)
                {
                    _logger.LogWarning($"Action not available in current state  |  Module '{Id}'");
                    return 2;
                }

                if (_program.Time < DelayTargetTime)
                {
                    _logger.LogWarning($"Action currently not available, try later  |  Module'{Id}'");
                    return 3;
                }

                State = State == ModuleStates.Standby ? ModuleStates.Online : ModuleStates.Standby;
                _logger.LogInfo($"State set to '{State}'  |  Module '{Id}'");
                return 0;
            }


            public bool? GetSubsystemState(string subsystemName)
            {
                if (string.IsNullOrEmpty(subsystemName))
                {
                    _logger.LogWarning($"Invalid subsystem name  |  Module '{Id}'");
                    return null;
                }

                var subsys = _subsystems.Find(s => s.Name == subsystemName);
                if (subsys == null)
                {
                    _logger.LogWarning($"Couldn't find any subsystem with name '{subsystemName}'  |  Module '{Id}'");
                    return null;
                }

                _logger.LogInfo($"Subsystem '{subsys.Name}' state '{(subsys.State ? "On" : "Off")}  |  Module '{Id}'");
                return subsys.State;
            }

            public int ToggleSubsystemState(string subsystemName)
            {
                if ((State & ModuleStates.Transition) == ModuleStates.None)
                {
                    _logger.LogWarning($"Action not available in current state  |  Module '{Id}'");
                    return 1;
                }

                if (_program.Time < DelayTargetTime)
                {
                    _logger.LogWarning($"Action currently not available, try later  |  Module'{Id}'");
                    return 2;
                }

                if (string.IsNullOrEmpty(subsystemName))
                {
                    _logger.LogWarning($"Invalid subsystem name  |  Module '{Id}'");
                    return 3;
                }

                var subsys = _subsystems.Find(s => s.Name == subsystemName);
                if (subsys == null)
                {
                    _logger.LogWarning($"Couldn't find any subsystem with name '{subsystemName}'  |  Module '{Id}'");
                    return 4;
                }

                subsys.State = !subsys.State;
                DelayTargetTime = subsys.State ? _program.Time + subsys.StartDelay : _program.Time + subsys.StopDelay;
                _logger.LogInfo($"Subsystem '{subsys.Name} state set to '{(subsys.State ? "On" : "Off")}'  |  Module '{Id}'");
                return 0;
            }

            public int SetSubsystemState(string subsystemName, bool state)
            {
                if ((State & ModuleStates.Transition) == ModuleStates.None)
                {
                    _logger.LogWarning($"Action not available in current state  |  Module '{Id}'");
                    return 1;
                }

                if (_program.Time < DelayTargetTime)
                {
                    _logger.LogWarning($"Action currently not available, try later  |  Module'{Id}'");
                    return 2;
                }

                if (string.IsNullOrEmpty(subsystemName))
                {
                    _logger.LogWarning($"Invalid subsystem name  |  Module '{Id}'");
                    return 3;
                }

                var subsys = _subsystems.Find(s => s.Name == subsystemName);
                if (subsys == null)
                {
                    _logger.LogWarning($"Couldn't find any subsystem with name '{subsystemName}'  |  Module '{Id}'");
                    return 4;
                }

                subsys.State = state;
                DelayTargetTime = state ? _program.Time + subsys.StartDelay : _program.Time + subsys.StopDelay;
                _logger.LogInfo($"Subsystem '{subsys.Name} state set to '{(subsys.State ? "On" : "Off")}'  |  Module '{Id}'");
                return 0;
            }
        }
    }
}
