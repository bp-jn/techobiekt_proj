using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace db_modelling_gui_2026.Models
{
    public class Relationship
    {
        public string FromTable { get; set; }
        public string ToTable { get; set; }
        public string Type { get; set; }

    }
}
