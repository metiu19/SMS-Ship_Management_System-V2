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
        public class BroadcastManager
        {
            private MyGridProgram _gridProgram;
            private Program _program;
            private IMyBroadcastListener _broadcastListener;
            private Action<MyIGCMessage> _broadcastMessageHandlers;


            public BroadcastManager(MyGridProgram myGridProgram) 
            {
                _gridProgram = myGridProgram;
                _program = (Program)myGridProgram;
                _program.Logger.LogInfo("Init Broadcast");

            }

            public bool SubcriberBrodcastMessage(string BroadcastTag, bool setCallback = true)
            {
                _broadcastListener = _gridProgram.IGC.RegisterBroadcastListener(BroadcastTag);
                if (setCallback) _broadcastListener.SetMessageCallback("BROADCAST");
                _program.Logger.LogInfo("Startup Broadcast Handler");
                return true;
            }


                public IMyBroadcastListener GetBroadcastListener() => _broadcastListener;


        }
    }
}
