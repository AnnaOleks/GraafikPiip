using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GraafikPiip.Interface;
using GraafikPiip.Views;
using System.Globalization;


namespace GraafikPiip.ViewModels
{
    public partial class StartPageViewModel : GraafikPiip.Infrastructure.BaseViewModel
    {
        private readonly INavigationService _nav;
        private readonly ICalendarService _calendar;

        [ObservableProperty]
        private string pealkiriKuu = "—";

        [ObservableProperty]
        private string tanaKohtAvatud = "";

        [ObservableProperty]
        private string tanaTootajadRida = "";

        [ObservableProperty]
        private string tananeTuhimikuteRida = "";

        [ObservableProperty]
        private Color tananeTuhimikuteVarv = Colors.Black;

        public StartPageViewModel(
            // INavigationService nav,
            ICalendarService calendar)
        {
            // _nav = nav;
            _calendar = calendar;

            var culture = CultureInfo.GetCultureInfo("ru-RU");
            PealkiriKuu = DateTime.Now.ToString("MMMM yyyy", culture);

            // Демотекст до загрузки
            TanaKohtAvatud = "—";
            TanaTootajadRida = "—";
            TananeTuhimikuteRida = "—";
            TananeTuhimikuteVarv = Colors.Gray;

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                IsBusy = true;

                var today = DateTime.Today;
                var summary = await _calendar.VotaPaevaKokkuvoteAsync(today);

                TanaKohtAvatud = summary.OnKinni
                    ? "Заведение закрыто"
                    : $"Часы работы: \n{summary.Open:hh\\:mm}–{summary.Close:hh\\:mm}";

                TanaTootajadRida = summary.Tootajad.Any()
                    ? "В смене: \n" + string.Join(",\n",
                        summary.Tootajad.Select(w => $"{w.Name} {w.Start:hh\\:mm}–{w.End:hh\\:mm}"))
                    : "Сотрудников нет";

                if (summary.Tuhimik.Any())
                {
                    TananeTuhimikuteRida = "Дыры: \n" + string.Join(", ",
                        summary.Tuhimik.Select(g => $"{g.Start:hh\\:mm}–{g.End:hh\\:mm}"));
                    TananeTuhimikuteVarv = Color.FromArgb("#f04e9e"); 
                }
                else
                {
                    TananeTuhimikuteRida = summary.OnKinni ? "—" : "Покрыто полностью";
                    TananeTuhimikuteVarv = Colors.Green;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private Task AvaKuuKalender() => Shell.Current.GoToAsync(nameof(MonthCalendarPage));



        [RelayCommand]
        private Task AvaTootajad() => Shell.Current.GoToAsync(nameof(EmployeesPage));


        [RelayCommand]
        private Task AvaSeaded() => Shell.Current.GoToAsync(nameof(SettingsPage));


    }
}
