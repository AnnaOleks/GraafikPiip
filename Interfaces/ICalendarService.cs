using GraafikPiip.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraafikPiip.Interface
{
    public record PaevaTuhimik(TimeSpan Start, TimeSpan End);
    public record PaevaTootaja(string Name, TimeSpan Start, TimeSpan End, string Color);

    public record PaevaKokkuvote(
    bool OnKinni,
    TimeSpan Open,
    TimeSpan Close,
    IReadOnlyList<PaevaTootaja> Tootajad,
    IReadOnlyList<PaevaTuhimik> Tuhimik);

    public interface ICalendarService
    {
        Task<PaevaKokkuvote> VotaPaevaKokkuvoteAsync(DateTime localDate);
        Task<IReadOnlyList<PaevaKokkuvote>> VotaKuuAsync(int year, int month);
        Task<IEnumerable<EmployeeDto>> GetEmployeesAsync();
    }
}
