using Microsoft.WindowsAPICodePack.ApplicationServices;
using System;

namespace BackgroundService.Source.Services.OS.Models
{
    internal class DisplaysStatus
    {
        public struct DisplayStatusState
        {
            public long? ReadyAt;
            public DisplaySettings SettingWhenTurnedOn;
            public bool TurnedOn;
        }

        private readonly object threadLock = new object();
        private DisplayStatusState state = new DisplayStatusState()
        {
            ReadyAt = null,
            SettingWhenTurnedOn = null,
            TurnedOn = true,
        };

        public DisplayStatusState GetState()
        {
            lock (threadLock)
            {
                return state;
            }
        }

        public void Update(Func<DisplaySettings> GetActiveDisplaySettings)
        {
            lock (threadLock)
            {
                bool newStatus = PowerManager.IsMonitorOn;
                bool isMonitorTurnedOn = !state.TurnedOn && newStatus;

                if (isMonitorTurnedOn)
                {
                    state.ReadyAt = DateTimeOffset.Now.ToUnixTimeMilliseconds() + InternalSettings.TimeoutDisplayTurnOn;
                    state.SettingWhenTurnedOn = GetActiveDisplaySettings();
                }
                state.TurnedOn = newStatus;
            }
        }
    }
}
