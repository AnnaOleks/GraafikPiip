using GraafikPiip.Views;

namespace GraafikPiip
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(MonthCalendarPage), typeof(GraafikPiip.Views.MonthCalendarPage));
            Routing.RegisterRoute(nameof(EmployeesPage), typeof(GraafikPiip.Views.EmployeesPage));
            Routing.RegisterRoute("SettingsPage", typeof(GraafikPiip.Views.SettingsPage));
        }
    }
}
