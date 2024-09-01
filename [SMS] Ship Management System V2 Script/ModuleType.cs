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
        public enum ModuleType
        {
            Block,
            Group,
            Tag
        }

        public class ModuleTypeBlock : IModuleType
        {
            private readonly IMyFunctionalBlock _block;

            public ModuleTypeBlock(IMyFunctionalBlock block)
            {
                _block = block;
            }

            public bool NeedsStateChange(bool correctState) =>
                _block.Enabled != correctState;

            public void ForceState(bool state) =>
                _block.Enabled = state;

        }

        public class ModuleTypeGroup : IModuleType
        {
            private readonly List<IMyFunctionalBlock> _blocks = new List<IMyFunctionalBlock>();

            public ModuleTypeGroup(List<IMyFunctionalBlock> blocks)
            {
                _blocks = blocks;
            }

            public bool NeedsStateChange(bool correctState)
            {
                bool ret = false;
                foreach (var block in _blocks)
                    if (block.Enabled != correctState)
                        ret = true;
                return ret;
            }

            public void ForceState(bool state) =>
                _blocks.ForEach(block => block.Enabled = state);
        }
    }
}
