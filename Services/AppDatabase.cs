using SQLite;
using GraafikPiip.Models;

namespace GraafikPiip.Services
{
    public class AppDatabase
    {
        private readonly string _path;
        private SQLiteAsyncConnection? _conn;

        public AppDatabase()
        {
#if ANDROID
            _path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "graafikpiip.db3");
#else
            _path = Path.Combine(FileSystem.AppDataDirectory, "graafikpiip.db3");
#endif
        }

        // Подключение к БД
        private SQLiteAsyncConnection Conn =>
            _conn ??= new SQLiteAsyncConnection(
                _path,
                SQLiteOpenFlags.ReadWrite |
                SQLiteOpenFlags.Create |
                SQLiteOpenFlags.SharedCache);

        // Инициализация базы (создание таблиц, индексов)
        public async Task InitAsync()
        {
            await Conn.CreateTableAsync<Tootaja>();
            await Conn.CreateTableAsync<Vahetus>();

            // Индексы для ускорения запросов
            await Conn.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_vahetus_kuupaev ON vahetus(kuupaev);");
            await Conn.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_vahetus_tootaja ON vahetus(tootaja_id);");
        }

        // === ГРАФИК РАБОТЫ КАЛЬЯННОЙ ===
        // Пн–Чт: 12–23 | Пт: 12–01 | Сб: 15–01 | Вс: 15–01
        public static (TimeSpan open, TimeSpan close) WorkHours(DayOfWeek dow) => dow switch
        {
            DayOfWeek.Monday => (new(12, 0, 0), new(23, 0, 0)),
            DayOfWeek.Tuesday => (new(12, 0, 0), new(23, 0, 0)),
            DayOfWeek.Wednesday => (new(12, 0, 0), new(23, 0, 0)),
            DayOfWeek.Thursday => (new(12, 0, 0), new(23, 0, 0)),
            DayOfWeek.Friday => (new(12, 0, 0), new(1, 0, 0)),  
            DayOfWeek.Saturday => (new(15, 0, 0), new(1, 0, 0)),
            DayOfWeek.Sunday => (new(15, 0, 0), new(23, 0, 0)),  
            _ => (new(12, 0, 0), new(23, 0, 0))
        };

        // === CRUD для работников ===

        public async Task<int> LisaTootajaAsync(Tootaja t)
        {
            // если цвет не указан — присваиваем случайный (пастельный)
            if (string.IsNullOrWhiteSpace(t.Varv))
                t.Varv = GetRandomColor();

            return await Conn.InsertAsync(t);
        }
        public Task<int> UuendaTootajaAsync(Tootaja t) => Conn.UpdateAsync(t);
        public Task<int> KustutaTootajaAsync(Tootaja t) => Conn.DeleteAsync(t);
        public Task<List<Tootaja>> GetTootajadAsync() => Conn.Table<Tootaja>().OrderBy(t => t.Nimi).ToListAsync();

        // === CRUD для смен ===

        public Task<int> LisaVahetusAsync(Vahetus v) => Conn.InsertAsync(v);
        public Task<int> UuendaVahetusAsync(Vahetus v) => Conn.UpdateAsync(v);
        public Task<int> KustutaVahetusAsync(Vahetus v) => Conn.DeleteAsync(v);

        public Task<List<Vahetus>> GetVahetusedByDateAsync(DateTime date) =>
            Conn.Table<Vahetus>()
                .Where(v => v.Kuupaev == date.Date)
                .OrderBy(v => v.VahetuseAlgus)
                .ToListAsync();

        public Task<List<Vahetus>> GetVahetusedByMonthAsync(int year, int month) =>
            Conn.Table<Vahetus>()
                .Where(v => v.Kuupaev >= new DateTime(year, month, 1)
                         && v.Kuupaev < new DateTime(year, month, 1).AddMonths(1))
                .OrderBy(v => v.Kuupaev)
                .ThenBy(v => v.VahetuseAlgus)
                .ToListAsync();

        public Task<List<Vahetus>> GetVahetusedByTootajaAsync(int tootajaId) =>
            Conn.Table<Vahetus>()
                .Where(v => v.TootajaId == tootajaId)
                .OrderBy(v => v.Kuupaev)
                .ThenBy(v => v.VahetuseAlgus)
                .ToListAsync();

        private static string GetRandomColor()
        {
            var colors = new[]
            {
                "#FF7F7F", // розовый
                "#FFD27F", // оранжевый
                "#7FFF7F", // зелёный
                "#7FBFFF", // голубой
                "#C97FFF", // фиолетовый
                "#FFB6C1", // светло-розовый
                "#FFF57F"  // жёлтый
            };
            var rnd = new Random();
            return colors[rnd.Next(colors.Length)];
        }
    }
}
