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
            public SMSErrorsManager(Program program) : base(program) { }
            /// <inheritdoc/>
            /// <param name="program">Program instance</param>
            /// <param name="surface">Surface to write err messages to</param>
            public SMSErrorsManager(Program program, IMyTextSurface surface) : base(program, surface) { }
            /// <inheritdoc/>
            /// <param name="program">Program instace</param>
            /// <param name="provider">Surface Provider of output screen</param>
            /// <param name="index">Surface Index</param>
            public SMSErrorsManager(Program program, IMyTextSurfaceProvider provider, int index = 0) : base(program, provider, index) { }


            public void AddGroupNoSupportedError(string group) =>
                RegisterError($"[ERR_SMS_GRP_NSUP] Group '{group}' doesn't contain any supported blocks");

            public void AddInvalidTypeFunctionsError(string moduleId) =>
                RegisterError($"[ERR_SMS_INV_TFUN] Invalid type functions for module: '{moduleId}'");

            public void AddModuleTypeInvalidError(string moduleId) =>
                RegisterError($"[ERR_SMS_MDL_TYP_INV] Module '{moduleId}' type invalid");

            public void AddModuleSubtypeInvalidError(string moduleId) =>
                RegisterError($"[ERR_SMS_MDL_STY_INV] Module '{moduleId}' subtype invalid");

            public void AddModuleSubtypeNotImplementedError(ModuleSubtype subtype) =>
                RegisterError($"[ERR_SMS_MDL_STY_NIMP] Module subtype '{subtype}' not implemented yet");
        }
    }
}
