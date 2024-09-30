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
        public BroadcastManager broadcastManager;

        public Program()
        {
            // Init Logs Screen
            IMyTextSurfaceProvider logsLCD = GridTerminalSystem.GetBlockWithName("SMS GUI Logs LCD") as IMyTextSurfaceProvider;
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
                Logger.LogFatal("Could not parse PB Custom Data");
            }

            Configs.InitializeConfigSettings(PBConfigs);

            string _errorMessage;
            if (!Configs.checkMyIniConfig(out _errorMessage))
            {
                Logger.LogFatal(_errorMessage);
            }


            broadcastManager = new BroadcastManager(this);
            broadcastManager.SubcriberBrodcastMessage(Configs.BroadcastTag);

        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & UpdateType.IGC) > 0)
            {
                Logger.LogDebug("IGC Trigger");
                TestBroadcastHandler(broadcastManager.GetBroadcastListener().AcceptMessage());
            }
        }


        void TestBroadcastHandler(MyIGCMessage msg)
        {
            // NOTE: called on ALL received messages; not just 'our' tag

            if (msg.Tag != Configs.BroadcastTag) 
            {
                Logger.LogWarning("Tag not valid");
                return; // not our message

            }
                

            if (msg.Data is string)
            {
                Logger.LogInfo("Received Test Message");
                Logger.LogInfo(" Source=" + msg.Source.ToString("X"));
                Logger.LogInfo(" Data=\"" + msg.Data + "\"");
                Logger.LogInfo(" Tag=" + msg.Tag);
            }
        }


    }
}
