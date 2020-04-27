using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class OrderTbl
    {
        public OrderTbl()
        {
            OrderDetailsTbl = new HashSet<OrderDetailsTbl>();
            ReturnTbl = new HashSet<ReturnTbl>();
        }

        public int Id { get; set; }
        public int OrderIdV { get; set; }
        public int UserId { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalPrice { get; set; }
        public int AddressId { get; set; }
        public string CouponCode { get; set; }
        public int PaymentId { get; set; }
        public byte Status { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public AddressTbl Address { get; set; }
        public PaymentTbl Payment { get; set; }
        public UserMstr User { get; set; }
        public ICollection<OrderDetailsTbl> OrderDetailsTbl { get; set; }
        public ICollection<ReturnTbl> ReturnTbl { get; set; }
    }
}
