using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homework_01.Model
{
    internal class PriceList
    {
        public Guid priceListId { get; set; }
        public string priceListName { get; set; }
        public Guid currencyId { get; set; }
    }
}
