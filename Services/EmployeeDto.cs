using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraafikPiip.Services
{
    public class EmployeeDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Color { get; set; }  // hex типа "#92C5FF"
    }
}
