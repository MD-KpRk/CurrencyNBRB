using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CurrencyNBRB.Models
{
    public class Currency 
    {

        [Key]
        public int Cur_ID { get; set; } // 1
        public int? Cur_ParentID { get; set; } 
        public string? Cur_Code { get; set; }
        public string? Cur_Name { get; set; }
        public string? Cur_Abbreviation { get; set; } // USD
        public string? Cur_QuotName { get; set; }
        public string? Cur_NameMulti { get; set; }
        public int Cur_Scale { get; set; }
        public int Cur_Periodicity { get; set; }
        public System.DateTime Cur_DateStart { get; set; }
        public System.DateTime Cur_DateEnd { get; set; }

        public override string ToString()
        {
            return Cur_Code + " " + Cur_Name;
        }
    }
}
