using BackgroundService.Source.Controllers;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.OS;
using System;
using System.IO;

namespace BackgroundService.Source
{
    static class Program
    {
        static void Main()
        {
            try
            {
                CursorService.EnsureCursorIsVisible();

                MainController mainController = new MainController();

                mainController.Run();
            }
            catch (Exception ex)
            {
                LoggerProvider.Global.Error($"Unhandled exception: {ex}");

                CursorService.EnsureCursorIsVisible();
                Environment.Exit(1);
            }
        }
    }
}
