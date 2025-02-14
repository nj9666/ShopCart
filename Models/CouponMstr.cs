﻿using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class CouponMstr
    {
        public CouponMstr()
        {
            OrderTbl = new HashSet<OrderTbl>();
        }

        public int Id { get; set; }
        public int AdminId { get; set; }
        public string CoupCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public byte DiscountType { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<OrderTbl> OrderTbl { get; set; }
    }
}
