using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraafikPiip.ViewModels;

{
    public class EmployeeOption
    {
        public string Id { get; set; } = "";          // GUID/строка/id из БД
        public string Name { get; set; } = "";        // Отображаемое имя
        public string? ColorHex { get; set; }         // Брендовый цвет (может быть null)
        public override string ToString() => Name;    // чтобы Picker умел показывать имя
    }
}
