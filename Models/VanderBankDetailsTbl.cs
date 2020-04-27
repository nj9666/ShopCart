using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class VanderBankDetailsTbl
    {
        public int Id { get; set; }
        public int VenderId { get; set; }
        public string AccountHolderName { get; set; }
        public decimal AccountNumber { get; set; }
        public string IfscCode { get; set; }
        public string BankName { get; set; }
        public string Branch { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public CityMstr City { get; set; }
        public StateMstr State { get; set; }
        public VenderMstr Vender { get; set; }
    }
}
