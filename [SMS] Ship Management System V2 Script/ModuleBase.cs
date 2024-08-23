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
            private List<Subsystem> _subsystems;

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

                // Custom Name
                Name = _configs.Get(Id, "Name").ToString(Id);
                _logger.LogInfo($"Module '{Id}' aka '{Name}'");

                // Standby
                _supportStandby = _configs.Get(Id, "Standby").ToBoolean();
                _logger.LogInfo($"Module '{Id}' standby support: {_supportStandby}");

                // Cooldown 
                _cooldownDelay = _configs.Get(Id, "Cooldown Delay").ToDouble(double.NaN);
                if (double.IsNaN(_cooldownDelay))
                {
                    _errsMngr.AddIniMissingKey(_program.Me.CustomName, Id, "Cooldown Delay");
                    _errsMngr.AddErrorDescription($"For module '{Id}'");
                    return 1;
                }

                // Subsystems
                string props = _configs.Get(Id, "Subsystems").ToString();
                if (props == "")
                {
                    _errsMngr.AddIniMissingKey(_program.Me.CustomName, Id, "Subsystems");
                    _errsMngr.AddErrorDescription($"For module '{Id}'");
                    return 1;
                }


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
                return null;
            }

            public int ToggleProperty(string propertyName)
            {
                return 100;
            }

            public int SetProperty(string propertyName, bool state)
            {
                return 100;
            }
        }
    }
}
