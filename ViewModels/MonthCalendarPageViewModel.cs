using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GraafikPiip.Interface;
using GraafikPiip.Models;
using GraafikPiip.Popups;
using System.Collections.ObjectModel;
using System.Globalization;

namespace GraafikPiip.ViewModels;

public partial class MonthCalendarPageViewModel : ObservableObject
{
    private readonly ICalendarService _calendar;
    private CancellationTokenSource? _loadCts;

    [ObservableProperty] private string pealkiriKuu = "—";

    [ObservableProperty] private PaevLahter? valitudPaev;
    public ObservableCollection<PaevLahter> Paevad { get; } = new();

    public ObservableCollection<EmployeeOption> Tootajad { get; } = new();

    public IReadOnlyList<string> PaevaPelkiri { get; } = new[] { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };

    private DateTime _cursor;

    public MonthCalendarPageViewModel(ICalendarService calendar)
    {
        _calendar = calendar;
        _cursor = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        _ = LaadiKuuAsync(_cursor);
        _ = LaadiTootajadAsync();
    }

    [RelayCommand] private Task EelmineKuu() => LaadiKuuAsync(_cursor.AddMonths(-1));
    [RelayCommand] private Task JargmineKuu() => LaadiKuuAsync(_cursor.AddMonths(1));
    [RelayCommand]
    private Task AvatudPaev(PaevLahter? paev)
    {
        ValitudPaev = paev;
        return Task.CompletedTask;
    }

    private bool CanEdit() => ValitudPaev is { OnTeineKuu: false };

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private async Task Lisa()
    {
       

        // Если список пуст — подгрузим на всякий
        if (Tootajad.Count == 0)
            await LaadiTootajadAsync();

        var popup = new AddShiftPopup(ValitudPaev.Kuupaev, Tootajad); // <-- передаём список
        var resultObj = await Application.Current.MainPage.ShowPopupAsync(popup);

        if (resultObj is not ShiftInput result) return;

        await _calendar.AddShiftAsync(ValitudPaev.Kuupaev, result.EmployeeId, result.Start, result.End, result.ColorHex); // Раскомментировать и убедиться, что метод возвращает ID новой смены

        ValitudPaev.Tootajad.Add(new TootajaVaade
        {
            TootajaId = result.EmployeeId, // Сохраняем ID работника
            ShiftStart = new DateTime(result.Start.Ticks), // Сохраняем полное время
            ShiftEnd = new DateTime(result.End.Ticks), // Сохраняем полное время
            Nimi = result.Name,
            Kellad = $"{new DateTime(result.Start.Ticks):HH\\:mm}–{new DateTime(result.End.Ticks):HH\\:mm}",
            Varv = Color.FromArgb(result.ColorHex)
        });

        ValitudPaev.OnKinni = false;
        ValitudPaev.TootajadRida = string.Join(", ", ValitudPaev.Tootajad.Select(t => t.Nimi));
        RefreshDay(ValitudPaev);

        // Сохраняем в БД
        // await _calendar.AddShiftAsync(ValitudPaev.Kuupaev, result.EmployeeId, result.Start, result.End, result.ColorHex);
    }
    [RelayCommand(CanExecute = nameof(CanEdit))]
    private async Task Uuenda()
    {
        if (ValitudPaev is null || ValitudPaev.Tootajad.Count == 0) return;

        // DEMO: обновим первую смену
        var t = ValitudPaev.Tootajad[0];
        t.Kellad = "10:30–20:00";
        if (!t.Nimi.EndsWith(" ★")) t.Nimi += " ★";

        ValitudPaev.TootajadRida = string.Join(", ", ValitudPaev.Tootajad.Select(x => x.Nimi));
        RefreshDay(ValitudPaev);

        // TODO: await _calendar.UpdateShiftAsync(ValitudPaev.Kuupaev, ...);
        await Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private async Task Kustuta()
    {
        if (ValitudPaev is null || ValitudPaev.Tootajad.Count == 0) return;

        ValitudPaev.Tootajad.RemoveAt(ValitudPaev.Tootajad.Count - 1);
        ValitudPaev.OnKinni = ValitudPaev.Tootajad.Count == 0;
        ValitudPaev.TootajadRida = ValitudPaev.Tootajad.Count == 0
            ? string.Empty
            : string.Join(", ", ValitudPaev.Tootajad.Select(x => x.Nimi));

        RefreshDay(ValitudPaev);

        // TODO: await _calendar.DeleteLastShiftAsync(ValitudPaev.Kuupaev);
        await Task.CompletedTask;
    }

    partial void OnValitudPaevChanged(PaevLahter? value)
    {
        LisaCommand.NotifyCanExecuteChanged();
        UuendaCommand.NotifyCanExecuteChanged();
        KustutaCommand.NotifyCanExecuteChanged();
    }

    private async Task LaadiKuuAsync(DateTime kuu)
    {
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        var ct = _loadCts.Token;

        _cursor = new DateTime(kuu.Year, kuu.Month, 1);
        PealkiriKuu = _cursor.ToString("MMMM yyyy", new CultureInfo("ru-RU"));

        Paevad.Clear();

        // Стартовая ячейка с понедельника
        var esimeneLahter = _cursor;
        while (esimeneLahter.DayOfWeek != DayOfWeek.Monday)
            esimeneLahter = esimeneLahter.AddDays(-1);

        for (int i = 0; i < 42; i++)
        {
            ct.ThrowIfCancellationRequested();

            var kuupaev = esimeneLahter.AddDays(i);
            var kokku = await _calendar.VotaPaevaKokkuvoteAsync(kuupaev);


            var tootajadList = new List<TootajaVaade>();

            if (kokku.Tootajad.Any())
            {
                foreach (var t in kokku.Tootajad)
                {
                    tootajadList.Add(new TootajaVaade
                    {
                        Nimi = t.Name,
                        Varv = Color.FromArgb(t.Color),
                        Kellad = $"{t.Start:HH\\:mm}–{t.End:HH\\:mm}"
                    });
                }
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (ct.IsCancellationRequested) return;

                Paevad.Add(new PaevLahter
                {
                    Kuupaev = kuupaev,
                    PaevaNumber = kuupaev.Day.ToString(),
                    OnTeineKuu = kuupaev.Month != _cursor.Month,
                    OnTana = kuupaev.Date == DateTime.Today,
                    OnKinni = kokku.OnKinni,
                    OnTuhik = kokku.Tuhimik.Any(),
                    TootajadRida = string.Join(", ", kokku.Tootajad.Select(t => t.Name)),
                    TuhikRida = kokku.Tuhimik.Any()
                        ? "Дыры: " + string.Join(", ", kokku.Tuhimik.Select(g => $"{g.Start:HH\\:mm}–{g.End:HH\\:mm}"))
                        : string.Empty,
                    Tootajad = tootajadList
                });
            });
        }
        if (!ct.IsCancellationRequested)
        {
            ValitudPaev = Paevad.FirstOrDefault(p => p.OnTana && !p.OnTeineKuu) ??
                          Paevad.FirstOrDefault(p => !p.OnTeineKuu) ??
                          Paevad.FirstOrDefault();
        }
    }

    private void RefreshDay(PaevLahter day)
    {
        var idx = Paevad.IndexOf(day);
        if (idx >= 0)
        {
            Paevad.RemoveAt(idx);
            Paevad.Insert(idx, day);
        }
    }

    private async Task LaadiTootajadAsync()
    {

        var emps = await _calendar.GetEmployeesAsync();
        // ожидается IEnumerable<{ Id, Name, Color }>
        Tootajad.Clear();
        foreach (var e in emps)
            Tootajad.Add(new EmployeeOption { Id = e.Id, Name = e.Name, ColorHex = e.Color });
    }

    public partial class PaevLahter : ObservableObject
    {
        public DateTime Kuupaev { get; set; }
        public string PaevaNumber { get; set; } = "";
        public bool OnTeineKuu { get; set; }
        public bool OnTana { get; set; }

        [ObservableProperty] private bool onKinni;
        [ObservableProperty] private bool onTuhik;

        [ObservableProperty] private string tootajadRida = "";
        [ObservableProperty] private string tuhikRida = "";

        public List<TootajaVaade> Tootajad { get; set; } = new();
    }

    public class TootajaVaade
    {
        public int VahetusId { get; set; } // Добавление ID смены
        public int TootajaId { get; set; } // Добавление ID работника
        public string Nimi { get; set; } = "";
        public Color Varv { get; set; } = Colors.White;
        public string Kellad { get; set; } = "";
        // Дополнительно: хранить полные даты/время для обновления
        public DateTime ShiftStart { get; set; }
        public DateTime ShiftEnd { get; set; }
    }
}
