using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace db_modelling_gui_2026.Models
{
    public class Table
    {
        public string Name { get; set; }
        public List<Column> Columns { get; set; }
        public Table() 
        {
        Columns = new List<Column>();
        }
    }
}
