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
        public class ModuleFactory
        {
            public static IModule CreateModule(Program program, string id, string terminalName, string type, string subtypeString)
            {
                IModuleType moduleType;
                ModuleSubtype subtype;
                if (!Enum.TryParse(subtypeString, out subtype))
                {
                    program.Logger.LogError($"Module '{id}' subtype invalid");
                    program.ErrsMngr.AddModuleSubtypeInvalidError(id);
                    return null;
                }

                // Type
                switch (type.ToLower())
                {
                    case "block":
                        program.Logger.LogInfo("Module Type: Block");
                        IMyFunctionalBlock block = program.GridTerminalSystem.GetBlockWithName(terminalName) as IMyFunctionalBlock;
                        if (block == null)
                        {
                            program.Logger.LogError($"Block '{terminalName}' not found for module '{id}'");
                            program.ErrsMngr.AddBlockNotFoundError(terminalName);
                            program.ErrsMngr.AddErrorDescription($"Module '{id}'");
                            return null;
                        }
                        moduleType = new ModuleTypeBlock(block);
                        break;

                    case "group":
                        program.Logger.LogInfo("Module Type: Group");
                        IMyBlockGroup group = program.GridTerminalSystem.GetBlockGroupWithName(terminalName);
                        if (group == null)
                        {
                            program.Logger.LogError($"Group '{terminalName}' not found for module '{id}'");
                            program.ErrsMngr.AddGroupNotFoundError(terminalName);
                            program.ErrsMngr.AddErrorDescription($"Module '{id}'");
                            return null;
                        }
                        List<IMyFunctionalBlock> blocks = new List<IMyFunctionalBlock>();
                        group.GetBlocksOfType(blocks);
                        if (blocks.Count == 0)
                        {
                            program.Logger.LogError($"Group '{terminalName}' doesn't have any supported block");
                            program.ErrsMngr.AddGroupNoSupportedError(terminalName);
                            program.ErrsMngr.AddErrorDescription($"Module '{id}'");
                            return null;
                        }
                        moduleType = new ModuleTypeGroup(blocks);
                        break;

                    default:
                        program.Logger.LogError($"Module '{id}' type invalid");
                        program.ErrsMngr.AddModuleTypeInvalidError(id);
                        return null;
                }

                // Configs
                MyIni moduleConfigs = new MyIni();
                MyIniParseResult res;
                if (!moduleConfigs.TryParse(program.Me.CustomData, id, out res))
                {
                    program.Logger.LogError($"Could not parse module '{id}' settings");
                    program.ErrsMngr.AddIniParseError(terminalName, res);
                    return null;
                }

                // Subtype
                switch (subtype)
                {
                    case ModuleSubtype.Generic:
                        program.Logger.LogInfo("Module Subtype: Generic");
                        return new ModuleBase(program, id, moduleType, subtype, moduleConfigs);

                    default:
                        program.Logger.LogError($"Module Subtype '{subtype}' not implemented yet");
                        program.ErrsMngr.AddModuleSubtypeNotImplementedError(subtype);
                        break;
                }

                return null;
            }
        }
    }
}
