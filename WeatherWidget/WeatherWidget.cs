using System;
using Dalamud.Plugin;
using FFXIVWeather;
using WeatherWidget.Attributes;

namespace WeatherWidget
{
    public class WeatherWidget : IDalamudPlugin
    {
        private DalamudPluginInterface pluginInterface;
        private FFXIVWeatherService weatherService;
        private PluginCommandManager<WeatherWidget> commandManager;
        private WeatherWidgetConfiguration config;
        private WeatherWidgetUI ui;

        public string Name => "WeatherWidget";

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;

            this.weatherService = new FFXIVWeatherService();

            this.config = (WeatherWidgetConfiguration)this.pluginInterface.GetPluginConfig() ?? new WeatherWidgetConfiguration();
            this.config.Initialize(this.pluginInterface);

            this.ui = new WeatherWidgetUI(this.weatherService);
            this.pluginInterface.UiBuilder.OnBuildUi += this.ui.Draw;

            this.commandManager = new PluginCommandManager<WeatherWidget>(this, this.pluginInterface);
        }

        [Command("/weatherwidget")]
        [Aliases("/ww")]
        [HelpMessage("Show/hide the WeatherWidget overlay.")]
        [ShowInHelp]
        public void ToggleWidget(string command, string args)
        {
            this.ui.IsVisible = !this.ui.IsVisible;
        }

        [Command("/wwconfig")]
        [HelpMessage("Show/hide the WeatherWidget configuration.")]
        [ShowInHelp]
        public void ToggleConfig(string command, string args)
        {
            this.ui.IsConfigVisible = !this.ui.IsConfigVisible;
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.commandManager.Dispose();

                // You may not want to save a configuration until after you're done tweaking the class layout.
                //this.pluginInterface.SavePluginConfig(this.config);

                this.pluginInterface.UiBuilder.OnBuildUi -= this.ui.Draw;

                this.pluginInterface.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
