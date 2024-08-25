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
        public struct Subsystem
        {
            public readonly string Name;
            public readonly float StopDelay;
            public readonly float StartDelay;
            public readonly bool DefaultState;
            public bool Enabled;

            public Subsystem(string name, float startDelay, float stopDelay, bool defualtState = false)
            {
                Name = name;
                StartDelay = startDelay;
                StopDelay = stopDelay;
                DefaultState = defualtState;
                Enabled = defualtState;
            }
        }
    }
}
