using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class AdminMstr
    {
        public AdminMstr()
        {
            CouponMstr = new HashSet<CouponMstr>();
            TodayDealsTbl = new HashSet<TodayDealsTbl>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public decimal ContactNumber { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<CouponMstr> CouponMstr { get; set; }
        public ICollection<TodayDealsTbl> TodayDealsTbl { get; set; }
    }
}
