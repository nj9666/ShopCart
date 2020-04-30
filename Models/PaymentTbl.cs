using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class PaymentTbl
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public byte Type { get; set; }
        public int CardId { get; set; }
        public string TransectionId { get; set; }
        public byte Status { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public CardsTbl Card { get; set; }
        public OrderTbl Order { get; set; }
    }
}
