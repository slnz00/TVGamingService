using BackgroundService.Source.Controllers;
using BackgroundService.Source.Providers;
using System;

namespace BackgroundService.Source
{
    static class Program
    {
        static void Main()
        {
            try
            {
                MainController mainController = new MainController();

                mainController.Run();
            }
            catch (Exception ex) {
                LoggerProvider.Global.Error($"Unhandled exception: {ex}");
            }
        }
    }
}
