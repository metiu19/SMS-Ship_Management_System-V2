using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public  class ConfigINI
        {
            public string BroadcastTag { get; private set; } // ID for Broadcast Connectioon
            public string LCDName { get; private set; }
            public string LCDTag { get; private set; }
            public Color BackgroundColor { get; private set; }
            public Color TextColor { get; private set; }


            public void InitializeConfigSettings(MyIni PBConfigs)
            {
                /* Broadcast Tag */
                BroadcastTag = PBConfigs.Get("settings", "Broadcast Tag").ToString();

                /* LCDName or LCDTag */
                if (PBConfigs.ContainsKey("settings", "LCDName"))
                    LCDName = PBConfigs.Get("settings", "LCD Name").ToString();
                else
                    LCDTag = PBConfigs.Get("settings", "LCD Tag").ToString();

                /* BackgroundColor */
                Color _color = new Color();
                if (_color.TryGetColorFromString(PBConfigs.Get("settings", "Background Color").ToString(), out _color))
                    BackgroundColor = _color;

                /* TextColor */
                if (_color.TryGetColorFromString(PBConfigs.Get("settings", "Text Color").ToString(), out _color))
                    TextColor = _color;
            }

            public string InitializeMyIniConfig()
            {
                MyIni configs = new MyIni();
                configs.Set("settings", "Broadcast Tag", "52345424345");
                configs.Set("settings", "LCD Name", "LCD_01");
                configs.Set("settings", "LCD Tag", "LCD_01");
                configs.Set("settings", "Background Color", "RGBA(255, 255, 255, 10)");
                configs.Set("settings", "Text Color", "RGBA(255, 0, 0, 10)");
                return configs.ToString();
            }

            public  bool checkMyIniConfig(out string Error)
            {
                if (string.IsNullOrEmpty(BroadcastTag))
                {
                    Error = "Error Broadcast Tag not set, ex: Broadcast Tag:52345424345";
                    return false;
                }
                if (string.IsNullOrEmpty(LCDName) && string.IsNullOrEmpty(LCDTag))
                {
                    Error = "Error LCD Name or LCD Tag not set, ex: LCD Name:LCD_01";
                    return false;
                }
                if (BackgroundColor == null)
                {
                    Error = "Error BackgroundColor not set, ex: Background Color:RGBA(255, 255, 255, 10)";
                    return false;
                }
                if (TextColor == null)
                {
                    Error = "Error TextColor not set, ex: Text Color:RGBA(255, 255, 255, 10)";
                    return false;
                }

                Error = "";
                return true;

            }
        }
    }
}
