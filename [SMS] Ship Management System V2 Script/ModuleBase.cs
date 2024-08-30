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
            private bool _defaultState;
            private bool _supportStandby;
            private double _cooldownDelay;

            public string Id { get; }
            public bool IsReady { get; private set; } = false;
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

            public int Init()
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
                    _errsMngr.AddIniMissingKey(_program.Me.CustomName, Id, "Cooldown Delay");
                    _errsMngr.AddErrorDescription($"For module '{Id}'");
                    return 1;
                }
                _logger.LogDebug($"Cooldown Delay = {_cooldownDelay}");


                // Subsystems
                string[] subsystems = _configs.Get(Id, "Subsystems").ToString().Split('\n');
                if (subsystems[0] == "")
                {
                    _errsMngr.AddIniMissingKey(_program.Me.CustomName, Id, "Subsystems");
                    _errsMngr.AddErrorDescription($"For module '{Id}'");
                    return 1;
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
                    _errsMngr.AddIniMissingKey(_program.Me.CustomName, Id, "Startup Sequence");
                    _errsMngr.AddErrorDescription($"For module '{Id}'");
                    return 1;
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

                    if (_subsystems.Find(s => s.Name == step.Name) == default(Subsystem))
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
                    _errsMngr.AddIniMissingKey(_program.Me.CustomName, Id, "Shutdown Sequence");
                    _errsMngr.AddErrorDescription($"For module '{Id}'");
                    return 1;
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

                    if (_subsystems.Find(s => s.Name == step.Name) == default(Subsystem))
                    {
                        _logger.LogError($"Invalid step name '{step.Name}', no matching subsystem");
                        _errsMngr.AddSequenceStepInvalidError(Id, "Shutdown", step.Name);
                        continue;
                    }

                    _logger.LogDebug($"Found step '{step.Name}'");
                    _shutdownSteps.Add(step);
                }
                _logger.LogDebug($"Actual shutdown steps: {_shutdownSteps.Count}");



                IsReady = true;
                return 0;
            }



            public bool CheckState()
            {
                if (!_type.NeedsStateChange(State.ToBool()))
                    return false;

                _type.ForceState(State.ToBool());
                return true;
            }

            public int TryFixError()
            {
                if (State != ModuleStates.Error)
                    return 1;

                foreach (Subsystem subsystem in _subsystems)
                {
                    if (subsystem.State != subsystem.DefaultState)
                        return 2;
                }
                return 0;
            }

            public int ToggleState()
            {
                if ((State & (ModuleStates.Offline | ModuleStates.Active)) == ModuleStates.None)
                    return 1;

                if ((State & ModuleStates.Active) != 0)
                    State = ModuleStates.ShuttingDown;
                else if (State == ModuleStates.Offline)
                    State = ModuleStates.BootingUp;
                return 0;
            }

            public int SetState(ModuleStates state)
            {
                if ((State & (ModuleStates.Offline | ModuleStates.Active)) == ModuleStates.None)
                    return 1;

                if (state == ModuleStates.Online)
                {
                    if (State != ModuleStates.Offline)
                        return 2;
                    State = ModuleStates.BootingUp;
                }
                else if (state == ModuleStates.Offline)
                {
                    if ((State & ModuleStates.Active) == ModuleStates.None)
                        return 2;
                    State = ModuleStates.ShuttingDown;
                }

                return 0;
            }

            public int Standby()
            {
                if ((State & ModuleStates.Active) == ModuleStates.None)
                    return 1;

                State = State == ModuleStates.Standby ? ModuleStates.Online : ModuleStates.Standby;
                return 0;
            }


            public bool? GetProperty(string propertyName)
            {
                if (string.IsNullOrEmpty(propertyName))
                    return null;

                var subsys = _subsystems.Find(s => s.Name == propertyName);
                if (subsys == default(Subsystem))
                    return null;

                return subsys.State;
            }

            public int ToggleProperty(string propertyName)
            {
                if (string.IsNullOrEmpty(propertyName))
                    return 1;

                var subsys = _subsystems.Find(s => s.Name == propertyName);
                if (subsys == default(Subsystem))
                    return 2;

                subsys.State = !subsys.State;
                return 0;
            }

            public int SetProperty(string propertyName, bool state)
            {
                if (string.IsNullOrEmpty(propertyName))
                    return 1;

                var subsys = _subsystems.Find(s => s.Name == propertyName);
                if (subsys == default(Subsystem))
                    return 2;

                subsys.State = state;
                return 0;
            }
        }
    }
}
