using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace IngameScript
{
    public static class ConfigINI
    {
        public static string ConnectionId { get; private set; }
        public static string Name { get; private set; }
        public static string Tag { get; private set; }
        public static Color BackgroundColor { get; private set; }
        public static Color TextColor { get; private set; }


        public static void InitializeConfigSettings(MyIni PBConfigs)
        {

            ConnectionId = PBConfigs.Get("settings", "Connection_ID").ToString();
            if (PBConfigs.ContainsKey("settings", "Name"))
                Name = PBConfigs.Get("settings", "Name").ToString();
            else
                Name = PBConfigs.Get("settings", "Tag").ToString();

            Color _color;
            ColorUtility _colorUtility = new ColorUtility();

            if (_colorUtility.TryGetColorFromString(PBConfigs.Get("settings", "BackgroundColor").ToString(), out _color))
                BackgroundColor = _color;

            if (_colorUtility.TryGetColorFromString(PBConfigs.Get("settings", "TextColor").ToString(), out _color))
                TextColor = _color;
        }


        public static string InitializeMyIniConfig()
        {
            MyIni configs = new MyIni();
            configs.Set("settings", "Connection_ID", "52345424345");
            configs.Set("settings", "Name", "LCD_01");
            configs.Set("settings", "Tag", "LCD_01");
            configs.Set("settings", "Background Color", "RGBA(255, 255, 255, 10)");
            configs.Set("settings", "Text Color", "RGBA(255, 0, 0, 10)");
            return configs.ToString();
        }

        public static bool checkMyIniConfig(out string Error)
        {
            if (string.IsNullOrEmpty(ConnectionId))
            {
                Error = "Error Connection_ID not set, ex: Connection_ID:52345424345";
                return false;
            }
            if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Tag))
            {
                Error = "Error Name or Tag not set, ex: Name:LCD_01";
                return false;
            }
            // TODO: Check Color
            Error = "";
            return true;

        }
    }
}
