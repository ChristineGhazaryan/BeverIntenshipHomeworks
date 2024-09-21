using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homework_01.Model
{
    internal class PriceListItem
    {
        public Guid priceListItemId { get; set; }
        public Guid priceListId { get; set; }
        public Guid productId { get; set; }
        public Guid transactionCurrencyId { get; set; }
        public Money priceMoney { get; set; }
        public string priceListItemName { get; set; }
    }
}
