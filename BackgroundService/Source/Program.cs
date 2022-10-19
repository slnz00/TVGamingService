using BackgroundService.Source.Controllers;
using System;
using System.Runtime.InteropServices;

namespace BackgroundService.Source
{
    static class Program
    {

        static void Main()
        {
            MainController mainController = new MainController();

            mainController.Run();
        }
    }
}
