using GraafikPiip.Interface;
using GraafikPiip.Models;

namespace GraafikPiip.Services
{
    public class CalendarService : ICalendarService
    {
        private readonly AppDatabase _db;

        public CalendarService(AppDatabase db)
        {
            _db = db;
        }
        public async Task<PaevaKokkuvote> VotaPaevaKokkuvoteAsync(DateTime localDate)
        {
            var date = localDate.Date;
            var (openTs, closeTs) = AppDatabase.WorkHours(date.DayOfWeek);

            var openDt = date + openTs;
            var closeDt = date + closeTs;
            if (closeTs < openTs) closeDt = closeDt.AddDays(1); // после полуночи

            // Загружаем данные из базы
            var vahetused = await _db.GetVahetusedByDateAsync(date);
            var tootajad = await _db.GetTootajadAsync();

            // Если данных нет — возвращаем "пустой" день, но с графиком работы
            if (vahetused.Count == 0)
            {
                return new PaevaKokkuvote(
                    OnKinni: false,
                    Open: openTs,
                    Close: closeTs,
                    Tootajad: Array.Empty<PaevaTootaja>(),
                    Tuhimik: new[] { new PaevaTuhimik(openTs, closeTs) } // вся смена пустая
                );
            }

            // Собираем рабочие интервалы работников
            var workerIntervals = vahetused
                .OrderBy(v => v.VahetuseAlgus)
                .Select(v =>
                {
                    var t = tootajad.FirstOrDefault(x => x.Id == v.TootajaId);
                    var name = t?.Nimi ?? $"Работник #{v.TootajaId}";
                    var start = date + v.VahetuseAlgus;
                    var end = date + v.VahetuseLopp;
                    if (v.VahetuseLopp < v.VahetuseAlgus) end = end.AddDays(1); // смена за полночь
                    return (name, start, end);
                })
                .Where(w => w.end > openDt && w.start < closeDt)
                .Select(w => (w.name, start: Max(w.start, openDt), end: Min(w.end, closeDt)))
                .OrderBy(w => w.start)
                .ToList();

            // === вычисляем "дыры" ===
            var gaps = new List<PaevaTuhimik>();
            var cursor = openDt;

            foreach (var w in workerIntervals)
            {
                if (w.start > cursor)
                    gaps.Add(new PaevaTuhimik(cursor.TimeOfDay, w.start.TimeOfDay));
                if (w.end > cursor) cursor = w.end;
                if (cursor >= closeDt) break;
            }

            if (cursor < closeDt)
                gaps.Add(new PaevaTuhimik(cursor.TimeOfDay, closeDt.TimeOfDay));

            // === финальная сводка ===
            var workers = workerIntervals
                .Select(w =>
                {
                    var color = tootajad.FirstOrDefault(t => t.Nimi == w.name)?.Varv ?? "#FFFFFF";
                    return new PaevaTootaja(w.name, w.start.TimeOfDay, w.end.TimeOfDay, color);
                })
                .ToList();

            return new PaevaKokkuvote(
                OnKinni: false,
                Open: openTs,
                Close: closeTs,
                Tootajad: workers,
                Tuhimik: gaps
            );
        }

        public async Task<IReadOnlyList<PaevaKokkuvote>> VotaKuuAsync(int year, int month)
        {
            var first = new DateTime(year, month, 1);
            var days = DateTime.DaysInMonth(year, month);
            var list = new List<PaevaKokkuvote>(days);

            for (int i = 0; i < days; i++)
            {
                var summary = await VotaPaevaKokkuvoteAsync(first.AddDays(i));
                list.Add(summary);
            }

            return list;
        }

        // Вспомогательные функции
        private static DateTime Max(DateTime a, DateTime b) => a >= b ? a : b;
        private static DateTime Min(DateTime a, DateTime b) => a <= b ? a : b;
    }
}