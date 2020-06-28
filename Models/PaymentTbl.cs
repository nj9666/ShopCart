using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class PaymentTbl
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Type { get; set; }
        public string TransectionId { get; set; }
        public string RespMsg { get; set; }
        public byte Status { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public OrderTbl Order { get; set; }
    }
}
