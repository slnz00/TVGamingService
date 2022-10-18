using BackgroundService.Source.Controllers;

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
