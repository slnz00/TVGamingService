using BackgroundService.Source.Providers;
using System.Runtime.InteropServices;
using static Core.WinAPI.InputAPI;

namespace BackgroundService.Source.Services.OS
{
    internal class InputService : Service
    {
        public InputService(ServiceProvider services) : base(services) { }

        public void PressKey(ushort vkCode)
        {
            var inputs = new Input[] {
                new Input()
                {
                    type = InputType.Keyboard,
                    union = new InputUnion()
                    {
                        keyboard = new KeyboardInput()
                        {
                            wVk = vkCode,
                            wScan = 0,
                            dwFlags = KeyEventF.KeyDown,
                            time = 0,
                            dwExtraInfo = GetMessageExtraInfo()
                        }
                    }
                },
                new Input()
                {
                    type = InputType.Keyboard,
                    union = new InputUnion()
                    {
                        keyboard = new KeyboardInput()
                        {
                            wVk = vkCode,
                            wScan = 0,
                            dwFlags = KeyEventF.KeyUp,
                            time = 0,
                            dwExtraInfo = GetMessageExtraInfo()
                        }
                    }
                }
            };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
        }
    }
}
