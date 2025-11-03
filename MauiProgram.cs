using GraafikPiip.Services;
using CommunityToolkit.Maui;


namespace GraafikPiip
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            // Views
            builder.Services.AddTransient<GraafikPiip.Views.StartPage>();
            builder.Services.AddTransient<GraafikPiip.Views.MonthCalendarPage>();
            builder.Services.AddTransient<GraafikPiip.Views.EmployeesPage>();
            builder.Services.AddTransient<GraafikPiip.Views.SettingsPage>();

            // ViewModels
            builder.Services.AddTransient<GraafikPiip.ViewModels.StartPageViewModel>();
            builder.Services.AddTransient<GraafikPiip.ViewModels.MonthCalendarPageViewModel>();
            builder.Services.AddTransient<GraafikPiip.ViewModels.EmployeesPageViewModel>();

            // Services
            builder.Services.AddSingleton<GraafikPiip.Interface.INavigationService, GraafikPiip.Services.NavigationService>();
            builder.Services.AddSingleton<GraafikPiip.Interface.ICalendarService, GraafikPiip.Services.CalendarService>();
            builder.Services.AddSingleton<AppDatabase>();


#endif

            return builder.Build();
        }
    }
}
