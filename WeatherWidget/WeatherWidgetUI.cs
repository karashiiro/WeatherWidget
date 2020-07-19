using FFXIVWeather;
using FFXIVWeather.Models;
using ImGuiNET;
using System;

namespace WeatherWidget
{
    public class WeatherWidgetUI
    {
        public bool IsVisible { get; set; }
        public bool IsConfigVisible { get; set; }

        private readonly FFXIVWeatherService weatherService;

        private (Weather, DateTime)[] forecast;
        private long frameCounter;

        public WeatherWidgetUI(FFXIVWeatherService weatherService)
        {
            this.weatherService = weatherService;
            this.forecast = Array.Empty<(Weather, DateTime)>();
        }

        public void Draw()
        {
            frameCounter++;
            if (frameCounter > 200)
            {
                this.forecast = this.weatherService.GetForecast(0, count: 16);
                frameCounter = 0;
            }

            if (!IsVisible)
                return;

            ImGui.Begin("WeatherWidget Overlay");
            foreach (var (weather, startTime) in this.forecast)
            {
                ImGui.Text($"{startTime.ToLongTimeString()}: {weather}");
            }
            ImGui.End();
        }
    }
}
