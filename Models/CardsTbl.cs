using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class CardsTbl
    {
        public CardsTbl()
        {
            PaymentTbl = new HashSet<PaymentTbl>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal CardNumber { get; set; }
        public decimal ExMonth { get; set; }
        public decimal ExYear { get; set; }
        public string HolderName { get; set; }
        public string CardLabel { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public UserMstr User { get; set; }
        public ICollection<PaymentTbl> PaymentTbl { get; set; }
    }
}
