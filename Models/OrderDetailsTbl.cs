using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class OrderDetailsTbl
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int SubProducatId { get; set; }
        public decimal Qty { get; set; }
        public byte OrderSubStatus { get; set; }
        public byte VenderPaymantStatus { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public OrderTbl Order { get; set; }
        public SubProductTbl SubProducat { get; set; }
    }
}
