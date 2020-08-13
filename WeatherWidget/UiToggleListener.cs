using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Hooking;
using Dalamud.Plugin;

namespace WeatherWidget
{
    public class UiToggleListener : IDisposable
    {
        private delegate IntPtr ToggleUiDelegate(IntPtr baseAddress, byte unknownByte);

        private readonly Hook<ToggleUiDelegate> hook;

        public bool Hidden { get; private set; }

        public UiToggleListener(SigScanner targetModuleScanner)
        {
            try
            {
                // Lifted from FPSPlugin, hook the ScrLk UI toggle; the client condition doesn't handle this
                var toggleUiPtr = targetModuleScanner.ScanText("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 0F B6 B9 ?? ?? ?? ?? B8 ?? ?? ?? ??");
                this.hook = new Hook<ToggleUiDelegate>(toggleUiPtr, new ToggleUiDelegate((ptr, b) =>
                {
                    Hidden = (Marshal.ReadByte(ptr, 105168) & 4) == 0;
                    return this.hook.Original(ptr, b);
                }));
                this.hook.Enable();
            }
            catch
            {
                PluginLog.LogError("UI toggle hook failed to install! It may be out of date.");
            }
        }

        public void Dispose()
        {
            this.hook?.Disable();
            this.hook?.Dispose();
        }
    }
}