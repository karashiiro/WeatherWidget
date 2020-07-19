using FFXIVWeather;
using FFXIVWeather.Models;
using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace WeatherWidget
{
    public class WeatherWidgetUI
    {
        private readonly IDictionary<string, TextureWrap> weatherIcons;
        private readonly FFXIVWeatherService weatherService;
        private readonly WeatherWidgetConfiguration config;

        private (Weather, DateTime)[] forecast;
        private long frameCounter;
        private bool configVisible;

        public bool CutsceneActive { get; set; }
        public bool IsVisible { get; set; }
        public bool IsConfigVisible
        {
            get => this.configVisible;
            set => this.configVisible = value;
        }

        public WeatherWidgetUI(WeatherWidgetConfiguration config, FFXIVWeatherService weatherService)
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

            var hideDuringCutscences = this.config.HideOverlaysDuringCutscenes;
            if (ImGui.Checkbox("Hide overlays during cutscenes", ref hideDuringCutscences))
            {
                this.config.HideOverlaysDuringCutscenes = hideDuringCutscences;
                this.config.Save();
            }

            var supportedLanguages = new[] { "English", "German", "French", "Japanese", "Chinese" };
            var currentItem = (int)this.config.Lang;
            if (ImGui.Combo("Language", ref currentItem, supportedLanguages, supportedLanguages.Length))
            {
                this.config.Lang = (LangKind)currentItem;
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
            frameCounter++;
            if (frameCounter > 200)
            {
                this.forecast = this.weatherService.GetForecast("Eulmore", count: 16);
                frameCounter = 0;
            }

            if (!IsVisible || this.config.HideOverlaysDuringCutscenes && CutsceneActive)
                return;

            ImGui.Begin("WeatherWidget Overlay");
            foreach (var (weather, startTime) in this.forecast)
            {
                ImGui.Text($"{startTime.ToLongTimeString()}: {weather.GetName(this.config.Lang)}");
            }
            ImGui.End();
        }
    }
}
