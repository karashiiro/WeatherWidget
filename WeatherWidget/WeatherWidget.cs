using Dalamud.Game.ClientState;
using Dalamud.Game.Internal;
using Dalamud.Hooking;
using Dalamud.Plugin;
using FFXIVWeather.Lumina;
using Lumina;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
        private WeatherWidgetUI ui;

        private bool uiHidden;
        private Hook<ToggleUIDelegate> toggleUIHook;

        private delegate IntPtr ToggleUIDelegate(IntPtr baseAddress, byte unknownByte);

        public string Name => "WeatherWidget";

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;

            this.config = (WeatherWidgetConfiguration)this.pluginInterface.GetPluginConfig() ?? new WeatherWidgetConfiguration();
            this.config.Initialize(this.pluginInterface);

            this.weatherService = new FFXIVWeatherLuminaService(new Cyalume(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "sqpack"), new LuminaOptions
            {
                DefaultExcelLanguage = this.config.Lang,
            }));

            this.pluginInterface.Framework.OnUpdateEvent += OnFrameworkUpdate;

            this.ui = new WeatherWidgetUI(this.config, this.weatherService);
            this.pluginInterface.UiBuilder.OnBuildUi += this.ui.Draw;
            this.pluginInterface.UiBuilder.OnBuildUi += this.ui.DrawConfig;
            this.pluginInterface.UiBuilder.OnOpenConfigUi += (sender, e) => this.ui.IsConfigVisible = true;

            // Lifted from FPSPlugin, hook the ScrLk UI toggle; the client condition doesn't handle this
            var toggleUiPtr = this.pluginInterface.TargetModuleScanner.ScanText("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 0F B6 B9 ?? ?? ?? ?? B8 ?? ?? ?? ??");
            this.toggleUIHook = new Hook<ToggleUIDelegate>(toggleUiPtr, new ToggleUIDelegate((ptr, b) =>
            {
                this.uiHidden = (Marshal.ReadByte(ptr, 104008) & 4) == 0;
                return this.toggleUIHook.Original(ptr, b);
            }));
            this.toggleUIHook.Enable();

            this.commandManager = new PluginCommandManager<WeatherWidget>(this, this.pluginInterface);
        }

        private void OnFrameworkUpdate(Framework framework)
        {
            this.ui.CutsceneActive = this.pluginInterface.ClientState.Condition[ConditionFlag.OccupiedInCutSceneEvent] ||
                                     this.pluginInterface.ClientState.Condition[ConditionFlag.WatchingCutscene] ||
                                     this.pluginInterface.ClientState.Condition[ConditionFlag.WatchingCutscene78] ||
                                     this.uiHidden;
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

                this.pluginInterface.SavePluginConfig(this.config);

                this.pluginInterface.Framework.OnUpdateEvent -= OnFrameworkUpdate;

                this.pluginInterface.UiBuilder.OnBuildUi -= this.ui.Draw;
                this.pluginInterface.UiBuilder.OnBuildUi -= this.ui.DrawConfig;
                this.pluginInterface.UiBuilder.OnOpenConfigUi -= (sender, e) => this.ui.IsConfigVisible = true;

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
