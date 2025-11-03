using GraafikPiip.Models;
using GraafikPiip.Services;

namespace GraafikPiip
{
    public partial class App : Application
    {
        public App(AppDatabase db) // <-- получаем базу из DI
        {
            InitializeComponent();
            MainPage = new AppShell();
            _ = Task.Run(async () => await db.InitAsync());
        }
    }
}