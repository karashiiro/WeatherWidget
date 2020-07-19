using Dalamud.Configuration;
using Dalamud.Plugin;
using FFXIVWeather;
using Newtonsoft.Json;

namespace WeatherWidget
{
    public class WeatherWidgetConfiguration : IPluginConfiguration
    {
        public int Version { get; set; }

        public bool LockWindows { get; set; }
        public bool ClickThrough { get; set; }
        public bool HideOverlaysDuringCutscenes { get; set; }
        public LangKind Lang { get; set; }

        [JsonIgnore] private DalamudPluginInterface pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void RestoreDefaults()
        {
            LockWindows = false;
            ClickThrough = false;
            HideOverlaysDuringCutscenes = false;
        }

        public void Save()
        {
            this.pluginInterface.SavePluginConfig(this);
        }
    }
}
