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
        public interface IModuleType
        {
            bool NeedsStateChange(bool correctState);

            void ForceState(bool state);
        }

        public interface IModule
        {
            string Id { get; }

            bool IsReady { get; }

            string Name { get; }

            ModuleSubtype Subtype { get; }

            ModuleStates State { get; }


            int Init();

            bool CheckState();

            int TryFixError();

            int ToggleState();

            int SetState(ModuleStates state);

            int Standby();


            bool? GetProperty(string propertyName);

            int ToggleProperty(string propertyName);

            int SetProperty(string propertyName, bool state);
        }
    }
}
