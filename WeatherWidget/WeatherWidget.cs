using Dalamud.Game.ClientState;
using Dalamud.Game.Internal;
using Dalamud.Plugin;
using FFXIVWeather.Lumina;
using Lumina;
using System;
using System.Diagnostics;
using System.IO;
using WeatherWidget.Attributes;
using Cyalume = Lumina.Lumina;

namespace WeatherWidget
{
    public class WeatherWidget : IDalamudPlugin
    {
        private DalamudPluginInterface pluginInterface;

        private FFXIVWeatherLuminaService weatherService;
        private PluginCommandManager<WeatherWidget> commandManager;
        private WeatherWidgetConfiguration config;
        private WeatherWidgetUi ui;

        public string Name => "WeatherWidget";

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;

            this.config = (WeatherWidgetConfiguration)this.pluginInterface.GetPluginConfig() ?? new WeatherWidgetConfiguration();
            this.config.Initialize(this.pluginInterface);

            var sqpackPath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "sqpack");
            this.weatherService = new FFXIVWeatherLuminaService(new Cyalume(sqpackPath, new LuminaOptions
            {
                DefaultExcelLanguage = this.config.Lang,
            }));

            this.ui = new WeatherWidgetUi(this.config, this.weatherService);
            this.pluginInterface.UiBuilder.OnBuildUi += this.ui.Draw;
            this.pluginInterface.UiBuilder.OnBuildUi += this.ui.DrawConfig;
            this.pluginInterface.UiBuilder.OnOpenConfigUi += (sender, e) => this.ui.IsConfigVisible = true;

            this.commandManager = new PluginCommandManager<WeatherWidget>(this, this.pluginInterface);
        }

        [Command("/weatherwidget")]
        [Aliases("/ww")]
        [HelpMessage("Show/hide the WeatherWidget overlay.")]
        public void ToggleWidget(string command, string args)
        {
            this.ui.IsVisible = !this.ui.IsVisible;
        }

        [Command("/wwconfig")]
        [HelpMessage("Show/hide the WeatherWidget configuration.")]
        public void ToggleConfig(string command, string args)
        {
            this.ui.IsConfigVisible = !this.ui.IsConfigVisible;
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            this.commandManager.Dispose();

            this.pluginInterface.SavePluginConfig(this.config);

            this.pluginInterface.Framework.OnUpdateEvent -= OnFrameworkUpdate;

            this.pluginInterface.UiBuilder.OnBuildUi -= this.ui.Draw;
            this.pluginInterface.UiBuilder.OnBuildUi -= this.ui.DrawConfig;
            this.pluginInterface.UiBuilder.OnOpenConfigUi -= (sender, e) => this.ui.IsConfigVisible = true;

            this.pluginInterface.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
