using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace IngameScript
{
    internal class ColorUtility
    {
        public bool TryGetColorFromString(string colorString, out Color color) // TODO: Ho cambiato idea meglio aggiungere all'inizio la dicitura di tipo es (RGBA o HEX)
        {
            char[] div = { ' ', ';', ',' };

            string[] splitStringColor = colorString.Split(div);
            if (splitStringColor.Length == 4)
            {
                try
                {
                    color = new Color()
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
                    color = new Color();
                    return false;
                }
            }

            color = new Color();
            return false;

        }
    }
}
