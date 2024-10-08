﻿using Sandbox.Game.EntityComponents;
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
        public class Subsystem
        {
            public string Name { get; }
            public bool DefaultState { get; }
            public float StartDelay { get; }
            public float StopDelay { get; }
            public bool State { get; set; }


            public Subsystem(string name, bool defualtState, float startDelay, float stopDelay)
            {
                Name = name;
                DefaultState = defualtState;
                StartDelay = startDelay;
                StopDelay = stopDelay;
                State = defualtState;
            }



            /// <summary>
            /// Try parse a string in to a <see cref="Subsystem"/>.
            /// </summary>
            /// <param name="subsysString">Input string to convert</param>
            /// <param name="subsystem">Objected parsed from <paramref name="subsysString"/></param>
            /// <returns><see cref="true"> if conversion succeded, false otherwise</returns>
            public static bool TryParse(string subsysString, out Subsystem subsystem)
            {
                subsystem = default(Subsystem);

                string[] props = subsysString.Trim().Split(' ');
                if (props.Length != 4)
                    return false;

                string name = props[0].Trim();
                if (name == "")
                    return false;

                bool defState;
                if (!bool.TryParse(props[1].Trim(), out defState))
                    return false;

                float upT, downT;
                if (!float.TryParse(props[2].Trim(), out upT))
                    return false;

                if (!float.TryParse(props[3].Trim(), out downT))
                    return false;

                subsystem = new Subsystem(name, defState, upT, downT);
                return true;
            }
        }

        public class SequenceStep
        {
            public string Name { get; }
            public bool State { get; }

            public SequenceStep(string name, bool state)
            {
                Name = name;
                State = state;
            }



            public static bool TryParse(string actionString, out SequenceStep moduleAction)
            {
                moduleAction = default(SequenceStep);
                actionString = actionString.Trim();

                if (string.IsNullOrEmpty(actionString))
                    return false;

                string[] fields = actionString.Split(' ');
                if (fields.Length != 2)
                    return false;

                fields[0] = fields[0].Trim();
                if (string.IsNullOrEmpty(fields[0]))
                    return false;

                bool state;
                if (!bool.TryParse(fields[1], out state))
                    return false;

                moduleAction = new SequenceStep(fields[0], state);
                return true;
            }
        }
    }
}
