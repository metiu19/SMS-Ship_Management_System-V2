using System;
using VRageMath;

namespace IngameScript
{
    public static class ColorExtensions
    {
        /// <summary>
        /// Attempts to convert a string representing a color in RGBA or HEX(ARGB) format to a Color object.
        /// </summary>
        /// <param name="stringColor"> 
        /// Text with the color to convert
        /// Accepted formats:
        /// - RGBA(255,255,255,0)
        /// - HEX(0080FF80)
        /// </param>
        /// <param name="resultColor"></param>
        /// <returns></returns>
        public static bool TryGetColorFromString(this Color color, string stringColor, out Color resultColor)
        {
            byte _r, _g, _b, _a;
            var matcRGBA = System.Text.RegularExpressions.Regex.Match(stringColor, @"RGBA\((\d+),\s*(\d+),\s*(\d+),\s*(\d+)\)");
            var matchHEX = System.Text.RegularExpressions.Regex.Match(stringColor, @"HEX\(([0-9A-Fa-f]{8})\)");
            if (matcRGBA.Success)
            {
                _r = byte.Parse(matcRGBA.Groups[1].Value);
                _g = byte.Parse(matcRGBA.Groups[2].Value);
                _b = byte.Parse(matcRGBA.Groups[3].Value);
                _a = byte.Parse(matcRGBA.Groups[4].Value);
            }
     
            else if (matchHEX.Success)
            {
                string _hexValue = matchHEX.Groups[1].Value;
                _a = byte.Parse(_hexValue.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                _r = byte.Parse(_hexValue.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                _g = byte.Parse(_hexValue.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                _b = byte.Parse(_hexValue.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }
            else 
            {
                resultColor = new Color();
                return false;
            }

            resultColor = new Color()
            {
                R = _r,
                G = _g,
                B = _b,
                A = _a,
            };
            return true;
        }
    }
}
