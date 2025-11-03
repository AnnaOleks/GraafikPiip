using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraafikPiip.Models
{
    [Table("tootaja")]
    public class Tootaja
    {
        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }
        [Column("nimi")]
        public string Nimi { get; set; }

        [Column("telefon")]
        public string Telefon { get; set; }

        [Column ("varv")]
        public string Varv { get; set; }
        [Column("photo")]
        public string Photo { get; set; }
    }
}
