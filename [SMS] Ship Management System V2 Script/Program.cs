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

        private const UpdateType _argCall = UpdateType.Terminal | UpdateType.Trigger | UpdateType.Mod | UpdateType.Script;
        private bool _inited = false;
        private readonly List<string> _requiredSections = new List<string>() { "SMS" };
        private readonly List<IModule> _modules = new List<IModule>();

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
                Logger.LogInfo($"Found module '{moduleId}'");

                string tName = PBConfigs.Get(moduleId, "Terminal Name").ToString();
                string tGroup = PBConfigs.Get(moduleId, "Terminal Group").ToString();
                string tTag = PBConfigs.Get(moduleId, "Terminal Tag").ToString();
                string terminalName;
                ModuleTypes type;
                if (tName != "" && tGroup == "" && tTag == "")
                {
                    terminalName = tName;
                    type = ModuleTypes.Block;
                }
                else if (tName == "" && tGroup != "" && tTag == "")
                {
                    terminalName = tGroup;
                    type = ModuleTypes.Group;
                }
                else if (tName == "" && tGroup == "" && tTag != "")
                {
                    terminalName = tTag;
                    type = ModuleTypes.Tag;
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
            Logger.LogInfo($"{_modules.Count}/{sections.Count} modules registered");

            //Runtime.UpdateFrequency = UpdateFrequency.Update10;
            Logger.LogInfo("Script Init Finished");
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (!_inited)
                Init();

            if ((updateSource & UpdateType.Update10) > 0)
            {
                CheckModules();
            }

            if ((updateSource & _argCall) > 0)
            {
                Echo(argument);
            }
        }

        private void Init()
        {
            _inited = true;
            Logger.LogInfo("Modules Init Started");
            _modules.ForEach(module => module.Init());
            Logger.LogInfo("Modules Init Finished");
            ErrsMngr.PrintErrors();
        }

        private void CheckModules()
        {
            _modules.ForEach(module => module.CheckState());
        }
    }
}
