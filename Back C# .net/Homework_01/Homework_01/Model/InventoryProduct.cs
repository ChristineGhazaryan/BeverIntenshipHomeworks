using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homework_01.Model
{
    internal class InventoryProduct
    {
        public Guid transactionCurrencyId { get; set; }
        public Guid invevnetoryProductId { get; set; }
        public Guid invevnetoryId { get; set; }
        public Guid productId { get; set; }
        public decimal pricePerUnit { get; set; } 
        public decimal totalAmount { get; set; }
        public int quantity { get; set; }
        public bool IsEmpty()
        {
            return pricePerUnit == 0m &&
                   quantity == 0 &&
                   invevnetoryId == Guid.Empty &&
                   productId == Guid.Empty;
        }
    }
}
