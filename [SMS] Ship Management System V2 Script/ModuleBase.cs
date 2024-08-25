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
            private bool _defaultState;
            private bool _supportStandby;
            private double _cooldownDelay;
            private List<Subsystem> _subsystems = new List<Subsystem>();

            public string Id { get; }
            public bool IsReady { get; private set; } = false;
            public string Name { get; private set; }
            public ModuleSubtype Subtype { get; }
            public ModuleState State { get; private set; }



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
                    State = ModuleState.Online;
                else
                    State = ModuleState.Offline;
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
                if (subsystems.Length == 0)
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
                return 100;
            }

            public int ToggleState()
            {
                return 100;
            }

            public int SetState(bool state)
            {
                return 100;
            }

            public int Standby()
            {
                return 100;
            }


            public bool? GetProperty(string propertyName)
            {
                if (string.IsNullOrEmpty(propertyName))
                    return null;

                var subsys = _subsystems.Find(s => s.Name == propertyName);
                if (subsys == default(Subsystem))
                    return null;

                return subsys.Enabled;
            }

            public int ToggleProperty(string propertyName)
            {
                if (string.IsNullOrEmpty(propertyName))
                    return 1;

                var subsys = _subsystems.Find(s => s.Name == propertyName);
                if (subsys == default(Subsystem))
                    return 2;

                subsys.Enabled = !subsys.Enabled;
                return 0;
            }

            public int SetProperty(string propertyName, bool state)
            {
                if (string.IsNullOrEmpty(propertyName))
                    return 1;

                var subsys = _subsystems.Find(s => s.Name == propertyName);
                if (subsys == default(Subsystem))
                    return 2;

                subsys.Enabled = state;
                return 0;
            }
        }
    }
}
