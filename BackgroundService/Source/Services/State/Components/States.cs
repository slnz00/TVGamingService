using BackgroundService.Source.Services.OS.Models;
using System.Collections.Generic;

using static BackgroundService.Source.Services.OS.CursorService;

namespace BackgroundService.Source.Services.State.Components
{
    internal enum States
    {
        [StateEntry("CursorRegistrySnapshot", typeof(List<CursorRegistryValue>))]
        CursorRegistrySnapshot,

        [StateEntry("DisplaySettingsSnapshot", typeof(DisplaySettingsSnapshot))]
        DisplaySettingsSnapshot,
    }
}
