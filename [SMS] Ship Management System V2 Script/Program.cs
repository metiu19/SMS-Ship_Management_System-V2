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
    partial class Program : MyGridProgram
    {
        public readonly string Title = "[SMS]";
        public readonly string Version = "V2.0.0";

        public List<IMyBlockGroup> Groups { get; } = new List<IMyBlockGroup>();
        public HashSet<IMyFunctionalBlock> Blocks { get; } = new HashSet<IMyFunctionalBlock>();
        public ScreenLogger Logger { get; }
        public SMSErrorsManager ErrsMngr { get; }
        public MyIni PBConfigs { get; } = new MyIni();
        public bool VerboseLogs { get; }
        public double Time { get; private set; } = 0;

        private const UpdateType _argCall = UpdateType.Terminal | UpdateType.Trigger | UpdateType.Mod | UpdateType.Script;
        private readonly List<string> _requiredSections = new List<string>() { "SMS" };
        private readonly List<IModule> _modules = new List<IModule>();
        private readonly MyCommandLine _commandLine = new MyCommandLine();
        private readonly Dictionary<string, Action> _commands = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase);
        private bool _inited = false;

        public Program()
        {
            // Init Errors Manager
            ErrsMngr = new SMSErrorsManager(this);


            // Init Logs Screen
            IMyTextSurfaceProvider logsLCD = GridTerminalSystem.GetBlockWithName("SMS Logs LCD") as IMyTextSurfaceProvider;
            Logger = new ScreenLogger(this, "Logs", logsLCD, 0);
#if DEBUG
            Logger.Debug = true;
#endif
            Logger.LogInfo("Script Init Starting");


            // Fill Groups & Blocks
            Logger.LogDebug("Fetching blocks and groups");
            GridTerminalSystem.GetBlockGroups(Groups);
            List<IMyFunctionalBlock> blocks = new List<IMyFunctionalBlock>();
            Groups.RemoveAll(g =>
            {
                blocks.Clear();
                g.GetBlocksOfType(blocks, b => b.IsSameConstructAs(Me));
                return blocks.Count == 0;
            });
            Logger.LogDebug($"Groups Length {Groups.Count}");

            blocks.Clear();
            GridTerminalSystem.GetBlocksOfType(blocks, b => b.IsSameConstructAs(Me));
            Blocks = blocks.ToHashSet();
            Logger.LogDebug($"Blocks Lenght {Blocks.Count}");


            // Parse PB Custom Data
            Logger.LogInfo("Loading configs");
            MyIniParseResult iniParseRes;
            if (!PBConfigs.TryParse(Me.CustomData, out iniParseRes))
            {
                Logger.LogCritical("Could not parse PB Custom Data");
                ErrsMngr.AddIniParseError(Me.CustomName, iniParseRes);
                ErrsMngr.PrintErrorsAndThrowException("Could not parse PB Custom Data");
            }


            // Check for missing sections
            List<string> sections = new List<string>();
            PBConfigs.GetSections(sections);
            List<string> missingSections = _requiredSections.Except(sections).ToList();
            if (missingSections.Count() != 0)
            {
                Logger.LogCritical("Missing required section/s check errors screen");
                ErrsMngr.AddConfigMissingError("Missing required section/s in PB Custom Data");
                missingSections.ForEach(ms => ErrsMngr.AddIniMissingSection(Me.CustomName, ms));
                ErrsMngr.PrintErrorsAndThrowException("Missing required section/s in configs");
            }
            sections.RemoveAll(s => new HashSet<string>(_requiredSections).Contains(s));


            // Load Modules
            Logger.LogInfo("Seraching Modules");
            foreach (string moduleId in sections)
            {
                Logger.LogInfo($"Found possible module '{moduleId}'");

                string tName = PBConfigs.Get(moduleId, "Terminal Name").ToString();
                string tGroup = PBConfigs.Get(moduleId, "Terminal Group").ToString();
                string tTag = PBConfigs.Get(moduleId, "Terminal Tag").ToString();
                string terminalName;
                ModuleType type;
                if (tName != "" && tGroup == "" && tTag == "")
                {
                    terminalName = tName;
                    type = ModuleType.Block;
                }
                else if (tName == "" && tGroup != "" && tTag == "")
                {
                    terminalName = tGroup;
                    type = ModuleType.Group;
                }
                else if (tName == "" && tGroup == "" && tTag != "")
                {
                    terminalName = tTag;
                    type = ModuleType.Tag;
                }
                else
                {
                    Logger.LogError("Multiple or none types specified");
                    ErrsMngr.AddModuleTypeInvalidError(moduleId);
                    continue;
                }

                string subtype = PBConfigs.Get(moduleId, "Subtype").ToString();
                if (subtype == "")
                {
                    Logger.LogError("Subtype not found");
                    ErrsMngr.AddIniMissingKey(Me.CustomName, moduleId, "Subtype");
                    continue;
                }

                IModule module = ModuleFactory.CreateModule(this, moduleId, terminalName, type, subtype);
                if (module != null)
                    _modules.Add(module);
            }
            ErrsMngr.PrintErrors();
            Logger.LogInfo($"Modules registered: {_modules.Count}/{sections.Count}");


            // Registering Args Commands
            _commands.Add("module", HandleModuleCommands);
            _commands.Add("subsystem", HandleSubsystemCommands);
            _commands.Add("check", CheckModules);
            _commands.Add("clear", Logger.ClearScreen);


            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            Logger.LogInfo("Script Init Finished");
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            Time += Runtime.TimeSinceLastRun.TotalSeconds;

            if (!_inited)
                Init();

            if ((updateSource & UpdateType.Update10) > 0)
            {
                CheckModules();
            }

            if ((updateSource & _argCall) > 0)
            {
                if (_commandLine.TryParse(argument))
                {
                    Logger.LogDebug($"Executing command: '{argument}'");

                    Action commandAction;
                    string command = _commandLine.Argument(0);

                    if (command == null)
                        Logger.LogWarning("No command specified");
                    else if (_commands.TryGetValue(command, out commandAction))
                        commandAction();
                    else
                        Logger.LogWarning($"Unknown command {command}");
                }
            }
        }

        private void Init()
        {
            List<IModule> badModules = new List<IModule>();
            Logger.LogInfo("Modules Init Started");
            foreach (var module in _modules)
            {
                if (!module.Init())
                    badModules.Add(module);
            }
            Logger.LogInfo("Modules Init Finished");

            _modules.RemoveAll(m => badModules.Contains(m));
            Logger.LogInfo($"Functional modules: {_modules.Count}");
            ErrsMngr.PrintErrors();

            _inited = true;
        }

        private void CheckModules()
        {
            _modules.ForEach(module => module.CheckState());
        }

        //Commands handling
        private void HandleModuleCommands()
        {
            Logger.LogDebug($"Module command");
            if (!_commandLine.Switch("n"))
            {
                Logger.LogWarning("Command 'module' requires argument '-n <module name>'");
                return;
            }

            string moduleId = _commandLine.Switch("n", 0);
            IModule module = _modules.Find(m => m.Id == moduleId);
            if (module == null)
            {
                Logger.LogWarning($"Couldn't find any module with id '{moduleId}'");
                return;
            }

            if (_commandLine.Switch("g"))
            {
                Logger.LogDebug("Action: Get state");
                Logger.LogInfo($"State '{module.State}'  |  Module '{module.Id}'");
                Echo(module.State.ToString());
            }
            else if (_commandLine.Switch("s"))
            {
                ModuleStates state;
                if (!Enum.TryParse(_commandLine.Switch("s", 0), out state))
                {
                    Logger.LogWarning($"Invail state '{_commandLine.Switch("s", 0)}'");
                    return;
                }

                if (state == ModuleStates.Standby)
                {
                    Logger.LogDebug("Action: Toggle standby");
                    Echo(module.Standby().ToString());
                }
                else
                {
                    Logger.LogDebug("Action: Set state");
                    Echo(module.SetState(state).ToString());
                }
            }
            else if (_commandLine.Switch("t"))
            {
                Logger.LogDebug("Action: Toggle state");
                Echo(module.ToggleState().ToString());
            }
            else if (_commandLine.Switch("r"))
            {
                Logger.LogDebug("Action: Reset error");
                Echo(module.ResetErrorState().ToString());
            }
            else
            {
                Logger.LogWarning("Command is missing required arguments");
            }
        }

        private void HandleSubsystemCommands()
        {
            Logger.LogDebug($"Subsystem command");
            if (!_commandLine.Switch("n") && !_commandLine.Switch("a"))
            {
                Logger.LogWarning("Command 'subsystem' requires arguments '-n <module name>' and '-a <subsystem name>'");
                return;
            }

            string moduleId = _commandLine.Switch("n", 0);
            IModule module = _modules.Find(m => m.Id == moduleId);
            if (module == null)
            {
                Logger.LogWarning($"Couldn't find any module with id '{moduleId}'");
                return;
            }

            string subsystemName = _commandLine.Switch("a", 0);

            if (_commandLine.Switch("g"))
            {
                Logger.LogDebug("Action: Get state");
                bool? subsysState = module.GetSubsystemState(subsystemName);
                Echo(subsysState == null ? "Subsystem not found!" : subsysState.ToString());
            }
            else if (_commandLine.Switch("s"))
            {
                Logger.LogDebug("Action: Set state");
                bool wantedState;
                if (!bool.TryParse(_commandLine.Switch("s",0), out wantedState))
                {
                    Logger.LogWarning($"Invalid state '{_commandLine.Switch("s", 0)}'");
                    return;
                }

                Echo(module.SetSubsystemState(subsystemName, wantedState).ToString());
            }
            else if (_commandLine.Switch("t"))
            {
                Logger.LogDebug("Action: Toggle State");
                Echo(module.ToggleSubsystemState(subsystemName).ToString());
            }
            else
            {
                Logger.LogWarning("Command is missing required arguments");
            }
        }
    }
}
