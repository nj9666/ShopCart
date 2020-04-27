using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class TodayDealsTbl
    {
        public int Id { get; set; }
        public int AdminId { get; set; }
        public int SubProId { get; set; }
        public bool DiscountType { get; set; }
        public int DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public AdminMstr Admin { get; set; }
        public SubProductTbl SubPro { get; set; }
    }
}
