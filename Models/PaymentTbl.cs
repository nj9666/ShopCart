using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class PaymentTbl
    {
        public PaymentTbl()
        {
            OrderTbl = new HashSet<OrderTbl>();
        }

        public int Id { get; set; }
        public byte Type { get; set; }
        public int CardId { get; set; }
        public string TransectionId { get; set; }
        public byte Status { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public CardsTbl Card { get; set; }
        public ICollection<OrderTbl> OrderTbl { get; set; }
    }
}
