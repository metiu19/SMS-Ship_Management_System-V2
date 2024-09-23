using Sandbox.ModAPI.Ingame;
using System;
using VRage.Game.ModAPI.Ingame.Utilities;
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

        public ConfigINI Configs = new ConfigINI();

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
                Me.CustomData = Configs.InitializeMyIniConfig();

            if (!PBConfigs.TryParse(Me.CustomData, out iniParseRes))
            {
                Logger.LogCritical("Could not parse PB Custom Data");
                throw new Exception("Could not parse PB Custom Data"); //TODO Gestire exception
            }

            Configs.InitializeConfigSettings(PBConfigs);

            string _errorMessage;
            if (!Configs.checkMyIniConfig(out _errorMessage))
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
