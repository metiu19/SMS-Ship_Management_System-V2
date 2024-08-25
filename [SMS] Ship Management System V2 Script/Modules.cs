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
        public enum ModuleSubtype
        {
            Generic,
            Test,
            Invalid
        }

        public class TestModule : ModuleBase
        {
            public string Yes { get; } = "Hello World!";

            public TestModule(Program program, string id, IModuleType type, ModuleSubtype subtype, MyIni iniConfigs) : base(program, id, type, subtype, iniConfigs)
            { }
        }
    }
}
