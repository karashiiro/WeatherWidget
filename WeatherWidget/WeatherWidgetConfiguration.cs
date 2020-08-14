using Dalamud.Configuration;
using Dalamud.Plugin;
using Lumina.Data;
using Newtonsoft.Json;

namespace WeatherWidget
{
    public class WeatherWidgetConfiguration : IPluginConfiguration
    {
        public int Version { get; set; }

        public bool LockWindows { get; set; }
        public bool ClickThrough { get; set; }
        public Language Lang { get; set; }

        public WeatherWidgetConfiguration()
        {
            Lang = Language.English;
        }

        [JsonIgnore] private DalamudPluginInterface pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void RestoreDefaults()
        {
            LockWindows = false;
            ClickThrough = false;
        }

        public void Save()
        {
            this.pluginInterface.SavePluginConfig(this);
        }
    }
}
