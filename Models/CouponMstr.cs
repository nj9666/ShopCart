using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class CouponMstr
    {
        public int Id { get; set; }
        public int AdminId { get; set; }
        public string CoupCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public byte DiscountType { get; set; }
        public string DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public AdminMstr Admin { get; set; }
    }
}
