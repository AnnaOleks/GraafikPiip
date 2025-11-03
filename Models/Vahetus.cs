using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColumnAttribute = SQLite.ColumnAttribute;
using TableAttribute = SQLite.TableAttribute;

namespace GraafikPiip.Models
{
    [Table ("vahetus")]
    public class Vahetus
    {
        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }

        [Column("kuupaev")]
        public DateTime Kuupaev { get; set; }

        [Column("tootaja_id")]
        public int TootajaId { get; set; }

        [Column("vahetuse_algus")]
        public TimeSpan VahetuseAlgus { get; set; }

        [Column("vahetuse_lopp")]
        public TimeSpan VahetuseLopp { get; set; }
    }
}
