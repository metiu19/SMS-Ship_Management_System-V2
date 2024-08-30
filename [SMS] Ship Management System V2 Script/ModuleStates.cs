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
    [Flags]
    public enum ModuleStates
    {
        None = 0,
        Offline = 1,
        BootingUp = 2,
        Online = 4,
        ShuttingDown = 8,
        Error = 16,
        Standby = 32,
        Active = Online | Standby
    }

    public static class ModuleStateExtensions
    {
        public static bool ToBool(this ModuleStates state) => state == ModuleStates.Online;
    }
}
