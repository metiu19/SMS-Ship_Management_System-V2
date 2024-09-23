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

        public readonly string Title = "[SMS-GUI]";
        public readonly string Version = "V1.0.0";

        public ScreenLogger Logger { get; }
        private MyCommandLine _commandLine = new MyCommandLine();
        public MyIni PBConfigs { get; } = new MyIni();

        public Program()
        {
            // Init Logs Screen
            IMyTextSurfaceProvider logsLCD = GridTerminalSystem.GetBlockWithName("SMS Logs LCD") as IMyTextSurfaceProvider;
            Logger = new ScreenLogger(this, "Logs", logsLCD, 0);
#if DEBUG
            Logger.Debug = true;
#endif
            Logger.LogInfo("Script Init Starting");


            // Parse PB Custom Data
            Logger.LogInfo("Loading configs");
            MyIniParseResult iniParseRes;

            if (string.IsNullOrEmpty(Me.CustomData))
                Me.CustomData = ConfigINI.InitializeMyIniConfig();

            if (!PBConfigs.TryParse(Me.CustomData, out iniParseRes))
            {
                Logger.LogCritical("Could not parse PB Custom Data");
                throw new Exception("Could not parse PB Custom Data"); //TODO Gestire exception
            }

            ConfigINI.InitializeConfigSettings(PBConfigs);

            string _errorMessage;
            if(!ConfigINI.checkMyIniConfig(out _errorMessage)) 
            {
                Logger.LogCritical($"Error too configure MyINI Config: {_errorMessage}");
                throw new Exception($"Error too configure MyINI Config: {_errorMessage}"); //TODO Gestire exception
            }
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
        }
    }
}
