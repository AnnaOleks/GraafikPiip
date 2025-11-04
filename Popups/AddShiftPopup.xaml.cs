using CommunityToolkit.Maui.Views;
using GraafikPiip.Models;
using GraafikPiip.ViewModels;

namespace GraafikPiip.Popups
{
    public record ShiftInput(string? EmployeeId, string Name, TimeSpan Start, TimeSpan End, string ColorHex);
    public partial class AddShiftPopup : CommunityToolkit.Maui.Views.Popup
    {
        private readonly DateTime _date;
        private readonly List<EmployeeOption> _tootajad;
        private const string OtherOptionName = "Другой…";

        public AddShiftPopup(DateTime date, IEnumerable<EmployeeOption> tootajad)
        {
            InitializeComponent();
            _date = date;
            _tootajad = tootajad.ToList();

            LblDate.Text = date.ToString("dddd, dd MMMM yyyy", new System.Globalization.CultureInfo("ru-RU"));

            var items = new List<EmployeeOption>(_tootajad)
        {
            new EmployeeOption { Id = "", Name = OtherOptionName, ColorHex = null }
        };

            EmployeePicker.ItemsSource = items;

            // дефолт: выбираем первого сотрудника, если есть; иначе «Другой…»
            EmployeePicker.SelectedIndex = items.Count > 1 ? 0 : items.Count - 1;

            // дефолтный цвет — из сотрудника или «Розовый»
            ColorPicker.SelectedIndex = 0;
        }

        private void EmployeePicker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (EmployeePicker.SelectedItem is not EmployeeOption opt) return;

            // Если «Другой…» — показываем ручной ввод имени
            var manual = opt.Name == OtherOptionName;
            ManualNameBlock.IsVisible = manual;

            if (!manual)
            {
                NameEntry.Text = opt.Name; // на всякий, но поле скрыто
                                           // Подставим цвет сотрудника, если указан
                if (!string.IsNullOrWhiteSpace(opt.ColorHex))
                    ColorPicker.SelectedIndex = IndexFromColor(opt.ColorHex);
            }
            else
            {
                NameEntry.Text = string.Empty;
            }
        }

        private async void Cancel_Clicked(object? sender, EventArgs e) => await this.CloseAsync();

        private async void Save_Clicked(object? sender, EventArgs e)
        {
            if (EmployeePicker.SelectedItem is not EmployeeOption opt) return;

            var manual = opt.Name == OtherOptionName;
            var name = manual ? NameEntry.Text?.Trim() : opt.Name;

            if (string.IsNullOrWhiteSpace(name))
            {
                await Application.Current.MainPage.DisplayAlert("Упс", "Введите имя.", "OK");
                return;
            }

            var start = StartPicker.Time;
            var end = EndPicker.Time;

            if (end <= start)
            {
                await Application.Current.MainPage.DisplayAlert("Упс", "Конец должен быть позже начала.", "OK");
                return;
            }

            var colorHex = SelectedPresetHex()
                           ?? opt.ColorHex
                           ?? "#92C5FF";

            this.Close(new ShiftInput(
    EmployeeId: manual ? null : opt.Id,
    Name: name,
    Start: start,
    End: end,
    ColorHex: colorHex));
        }
        private string? SelectedPresetHex() => ColorPicker.SelectedItem?.ToString() switch
        {
            "Розовый" => "#F39FB8",
            "Голубой" => "#92C5FF",
            "Зелёный" => "#7EE787",
            "Жёлтый" => "#FACC15",
            "Фиолетовый" => "#C084FC",
            _ => null
        };

        private int IndexFromColor(string hex) => hex.ToUpperInvariant() switch
        {
            "#F39FB8" => 0,
            "#92C5FF" => 1,
            "#7EE787" => 2,
            "#FACC15" => 3,
            "#C084FC" => 4,
            _ => 0
        };
    }

}