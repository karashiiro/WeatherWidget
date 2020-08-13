using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.Numerics;
using FFXIVWeather.Lumina;
using Lumina.Data;
using Lumina.Excel.GeneratedSheets;

namespace WeatherWidget
{
    public class WeatherWidgetUi
    {
        private readonly IDictionary<string, TextureWrap> weatherIcons;
        private readonly FFXIVWeatherLuminaService weatherService;
        private readonly WeatherWidgetConfiguration config;

        private IList<(Weather, DateTime)> forecast;
        private long frameCounter;
        private bool configVisible;

        public bool CutsceneActive { get; set; }
        public bool IsVisible { get; set; }
        public bool IsConfigVisible
        {
            get => this.configVisible;
            set => this.configVisible = value;
        }

        public WeatherWidgetUi(WeatherWidgetConfiguration config, FFXIVWeatherLuminaService weatherService)
        {
            this.config = config;
            this.weatherService = weatherService;

            this.weatherIcons = new Dictionary<string, TextureWrap>();
            this.forecast = Array.Empty<(Weather, DateTime)>();
        }

        public void DrawConfig()
        {
            if (!IsConfigVisible || this.config.HideOverlaysDuringCutscenes && CutsceneActive)
                return;

            ImGui.SetNextWindowSize(new Vector2(400, 332), ImGuiCond.Always);

            ImGui.Begin("WeatherWidget Configuration", ref configVisible, ImGuiWindowFlags.NoResize);
            var lockWindows = this.config.LockWindows;
            if (ImGui.Checkbox("Lock plugin windows", ref lockWindows))
            {
                this.config.LockWindows = lockWindows;
                this.config.Save();
            }

            var clickThrough = this.config.ClickThrough;
            if (ImGui.Checkbox("Click through plugin windows", ref clickThrough))
            {
                this.config.ClickThrough = clickThrough;
                this.config.Save();
            }

            var hideDuringCutscenes = this.config.HideOverlaysDuringCutscenes;
            if (ImGui.Checkbox("Hide overlays during cutscenes", ref hideDuringCutscenes))
            {
                this.config.HideOverlaysDuringCutscenes = hideDuringCutscenes;
                this.config.Save();
            }

            var supportedLanguages = new[] { "Japanese", "English", "German", "French", "Chinese" };
            var currentItem = (int)this.config.Lang;
            if (ImGui.Combo("Language", ref currentItem, supportedLanguages, supportedLanguages.Length))
            {
                this.config.Lang = (Language)(currentItem + 1);
                this.config.Save();
            }

            if (ImGui.Button("Defaults"))
            {
                this.config.RestoreDefaults();
                this.config.Save();
            }
            ImGui.End();
        }

        public void Draw()
        {
            const int forecastCount = 16;

            this.frameCounter++;
            if (this.frameCounter > 200)
            {
                this.forecast = this.weatherService.GetForecast("Eulmore", forecastCount);
                this.frameCounter = 0;
            }

            if (!IsVisible || this.config.HideOverlaysDuringCutscenes && CutsceneActive)
                return;

            ImGui.Begin("WeatherWidget Overlay");
            ImGui.Columns(forecastCount);
            foreach (var (weather, startTime) in this.forecast)
            {
                ImGui.Text($"{startTime.ToLongTimeString()}: {weather.Name}");
                ImGui.NextColumn();
            }
            ImGui.End();
        }
    }
}
