using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyNBRB.Models
{
    public class RateShort
    {
        public int Cur_ID { get; set; }
        [Key]
        public System.DateTime Date { get; set; }
        public decimal? Cur_OfficialRate { get; set; }
    }
}
