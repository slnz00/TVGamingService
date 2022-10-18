using System;

namespace BackgroundService.Source.Services.System.Models
{
    [Flags]
    internal enum KeyModifiers : uint
    {
        Alt = 0x0001,
        Control = 0x0002,
        Shift = 0x0004,
        Win = 0x0008
    }
}
