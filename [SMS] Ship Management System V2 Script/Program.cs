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
            Logger.LogInfo("Script Init Starting");

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

            // Set Global configs
            VerboseLogs = PBConfigs.Get("SMS", "Verbose").ToBoolean();

            // Load Modules
            Logger.LogInfo("Seraching Modules");
            foreach (var section in sections)
            {
                Logger.LogInfo($"Found module '{section}'");

                string terminalName = PBConfigs.Get(section, "Terminal Name").ToString();
                if (terminalName == "")
                {
                    ErrsMngr.AddIniMissingKey(Me.CustomName, section, "TerminalName");
                    continue;
                }

                string type = PBConfigs.Get(section, "Type").ToString();
                if (type == "")
                {
                    ErrsMngr.AddIniMissingKey(Me.CustomName, section, "Type");
                    continue;
                }

                string subtype = PBConfigs.Get(section, "Subtype").ToString();
                if (subtype == "")
                {
                    ErrsMngr.AddIniMissingKey(Me.CustomName, section, "Subtype");
                    continue;
                }

                IModule module = ModuleFactory.CreateModule(this, section, terminalName, type, subtype);
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
