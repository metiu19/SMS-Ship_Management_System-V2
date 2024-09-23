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
            public  IModule CreateModule(Program program, string id, string terminalName, ModuleType type, string subtypeString)
            {
                IModuleType moduleType;
                ModuleSubtype subtype;
                if (!Enum.TryParse(subtypeString, out subtype))
                {
                    program.Logger.LogError("Subtype invalid");
                    program.ErrsMngr.AddModuleSubtypeInvalidError(id);
                    return null;
                }

                // Type
                switch (type)
                {
                    case ModuleType.Block:
                        program.Logger.LogInfo("Type: Terminal Block");
                        if (!TryCreateTypeBlock(program, id, terminalName, out moduleType))
                            return null;
                        break;

                    case ModuleType.Group:
                        program.Logger.LogInfo("Type: Terminal Group");
                        if (!TryCreateTypeGroup(program, id, terminalName, out moduleType))
                            return null;
                        break;

                    case ModuleType.Tag:
                        var blockMatches = program.Blocks.Where(b => b.CustomName.Contains(terminalName)).ToList();
                        if (blockMatches.Count() == 1)  // Block
                        {
                            program.Logger.LogInfo("Type: Terminal Block");
                            if (!TryCreateTypeBlock(program, id, terminalName, out moduleType))
                                return null;
                        }
                        else if (blockMatches.Count() > 1) // Blocks Group
                        {
                            program.Logger.LogInfo("Type: SMS Group");
                            moduleType = new ModuleTypeGroup(blockMatches);
                        }
                        else if (program.Groups.Any(g => g.Name.Contains(terminalName)))  // Terminal Group
                        {
                            program.Logger.LogInfo("Type: Terminal Group");
                            if (!TryCreateTypeGroup(program, id, terminalName, out moduleType))
                                return null;
                        }
                        else    // Not Found
                        {
                            program.Logger.LogError("No block/s or group with tag found");
                            program.ErrsMngr.AddTagBlocksNotFoundError(terminalName);
                            program.ErrsMngr.AddErrorDescription($"Module '{id}'");
                            return null;
                        }
                        break;

                    default:
                        program.Logger.LogError("Type invalid");
                        program.ErrsMngr.AddModuleTypeInvalidError(id);
                        return null;
                }

                // Configs
                MyIni moduleConfigs = new MyIni();
                MyIniParseResult res;
                if (!moduleConfigs.TryParse(program.Me.CustomData, id, out res))
                {
                    program.Logger.LogError($"Could not parse module settings");
                    program.ErrsMngr.AddIniParseError(terminalName, res);
                    return null;
                }

                // Subtype
                switch (subtype)
                {
                    case ModuleSubtype.Generic:
                        program.Logger.LogInfo("Subtype: Generic");
                        return new ModuleBase(program, id, moduleType, subtype, moduleConfigs);

                    default:
                        program.Logger.LogError($"Subtype '{subtype}' not implemented yet");
                        program.ErrsMngr.AddModuleSubtypeNotImplementedError(subtype);
                        break;
                }

                return null;
            }

            private  bool TryCreateTypeBlock(Program program, string moduleId, string blockName, out IModuleType moduleType)
            {
                moduleType = null;

                IMyFunctionalBlock block = program.Blocks.FirstOrDefault(b => b.CustomName.Contains(blockName));
                if (block == null || !block.IsSameConstructAs(program.Me))
                {
                    program.Logger.LogError($"Block '{blockName}' not found");
                    program.ErrsMngr.AddBlockNotFoundError(blockName);
                    program.ErrsMngr.AddErrorDescription($"Module '{moduleId}'");
                    return false;
                }

                moduleType = new ModuleTypeBlock(block);
                return true;
            }

            private  bool TryCreateTypeGroup(Program program, string moduleId, string groupName, out IModuleType moduleType)
            {
                moduleType = null;

                IMyBlockGroup group = program.Groups.Find(g => g.Name.Contains(groupName));
                if (group == null)
                {
                    program.Logger.LogError($"Group '{groupName}' not found");
                    program.ErrsMngr.AddGroupNotFoundError(groupName);
                    program.ErrsMngr.AddErrorDescription($"Module '{moduleId}'");
                    return false;
                }
                List<IMyFunctionalBlock> blocks = new List<IMyFunctionalBlock>();
                group.GetBlocksOfType(blocks, b => b.IsSameConstructAs(program.Me));
                if (blocks.Count == 0)
                {
                    program.Logger.LogError($"Group '{groupName}' doesn't have any supported blocks");
                    program.ErrsMngr.AddGroupNoSupportedError(groupName);
                    program.ErrsMngr.AddErrorDescription($"Module '{moduleId}'");
                    return false;
                }

                moduleType = new ModuleTypeGroup(blocks);
                return true;
            }
        }
    }
}
