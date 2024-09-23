using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public  class ConfigINI
        {
            public string ConnectionId { get; private set; }
            public string Name { get; private set; }
            public string Tag { get; private set; }
            public Color BackgroundColor { get; private set; }
            public Color TextColor { get; private set; }


            public void InitializeConfigSettings(MyIni PBConfigs)
            {

                ConnectionId = PBConfigs.Get("settings", "Connection_ID").ToString();
                if (PBConfigs.ContainsKey("settings", "Name"))
                    Name = PBConfigs.Get("settings", "Name").ToString();
                else
                    Name = PBConfigs.Get("settings", "Tag").ToString();

                Color _color = new Color();
                if (_color.TryGetColorFromString(PBConfigs.Get("settings", "BackgroundColor").ToString(), out _color))
                    BackgroundColor = _color;

                if (_color.TryGetColorFromRGBA(PBConfigs.Get("settings", "TextColor").ToString(), out _color))
                    TextColor = _color;
            }


            public  string InitializeMyIniConfig()
            {
                MyIni configs = new MyIni();
                configs.Set("settings", "Connection_ID", "52345424345");
                configs.Set("settings", "Name", "LCD_01");
                configs.Set("settings", "Tag", "LCD_01");
                configs.Set("settings", "Background Color", "RGBA(255, 255, 255, 10)");
                configs.Set("settings", "Text Color", "RGBA(255, 0, 0, 10)");
                return configs.ToString();
            }

            public  bool checkMyIniConfig(out string Error)
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
}
