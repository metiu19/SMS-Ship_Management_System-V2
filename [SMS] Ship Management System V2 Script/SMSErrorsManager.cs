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
        public class SMSErrorsManager : ErrorsManager
        {
            /// <inheritdoc/>
            /// <param name="program"></param>
            public SMSErrorsManager(ScreenLogger logger) : base(logger) { }


            public void AddInvalidModuleIdError(string moduleId, ErrorsType errType = ErrorsType.Error) =>
                RegisterError($"[SMS_ID_INV] Invalid module id '{moduleId}'", errType);

            public void AddGroupNoSupportedError(string group, ErrorsType errType = ErrorsType.Error) =>
                RegisterError($"[SMS_GRP_NSUP] Group '{group}' doesn't contain any supported blocks", errType);

            public void AddInvalidTypeFunctionsError(string moduleId, ErrorsType errType = ErrorsType.Error) =>
                RegisterError($"[SMS_INV_TFUN] Invalid type functions for module: '{moduleId}'", errType);

            public void AddModuleTypeInvalidError(string moduleId, ErrorsType errType = ErrorsType.Error) =>
                RegisterError($"[SMS_MDL_TYP_INV] Module '{moduleId}' type invalid", errType);

            public void AddModuleSubtypeInvalidError(string moduleId, ErrorsType errType = ErrorsType.Error) =>
                RegisterError($"[SMS_MDL_STY_INV] Module '{moduleId}' subtype invalid", errType);

            public void AddModuleSubtypeNotImplementedError(ModuleSubtype subtype, ErrorsType errType = ErrorsType.Error) =>
                RegisterError($"[SMS_MDL_STY_NIMP] Module subtype '{subtype}' not implemented yet", errType);

            public void AddTagBlocksNotFoundError(string tag, ErrorsType errType = ErrorsType.Error) =>
                RegisterError($"[SMS_TAG_BLK_404] Couldn't find any block or group with given tag '{tag}'", errType);

            public void AddSubsystemParseError(string moduleId, int subsysIndex, ErrorsType errType = ErrorsType.Error) =>
                RegisterError($"[SMS_SBS_PRS] Couldn't parse subsystem {subsysIndex} of module '{moduleId}'", errType);

            public void AddSequenceStepParseError(string moduleId, string sequenceName, int stepIndex, ErrorsType errType = ErrorsType.Error) =>
                RegisterError($"[SMS_SQN_PRS] Couldn't parse {sequenceName} sequence step {stepIndex} of module '{moduleId}'", errType);

            public void AddSequenceStepInvalidError(string moduleId, string sequenceName, string stepName, ErrorsType errType = ErrorsType.Error) =>
                RegisterError($"[SMS_SQN_INV] Module '{moduleId}' doesn't have any subsystem named '{stepName}' requested by {sequenceName} sequence", errType);
        }
    }
}
