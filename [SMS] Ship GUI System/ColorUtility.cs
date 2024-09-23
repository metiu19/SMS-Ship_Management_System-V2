﻿using System;
using VRageMath;

namespace IngameScript
{
        public static class ColorExtensions
        {
            public static bool TryGetColorFromString(this Color color, string colorString, out Color resultColor) // TODO: Ho cambiato idea meglio aggiungere all'inizio la dicitura di tipo es (RGBA o HEX)
            {
                char[] div = { ' ', ';', ',' };

                string[] splitStringColor = colorString.Split(div);
                if (splitStringColor.Length == 4)
                {
                    try
                    {
                        resultColor = new Color()
                        {
                            R = byte.Parse(splitStringColor[0]),
                            G = byte.Parse(splitStringColor[1]),
                            B = byte.Parse(splitStringColor[2]),
                            A = byte.Parse(splitStringColor[3]),
                        };
                        return true;
                    }
                    catch (Exception)
                    {
                        resultColor = new Color();
                        return false;
                    }
                }

                resultColor = new Color();
                return false;

            }
        }
    }
}
