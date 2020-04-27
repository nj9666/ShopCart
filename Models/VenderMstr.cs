using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class VenderMstr
    {
        public VenderMstr()
        {
            ColoursTbl = new HashSet<ColoursTbl>();
            ProductMstr = new HashSet<ProductMstr>();
            SizesTbl = new HashSet<SizesTbl>();
            VanderBankDetailsTbl = new HashSet<VanderBankDetailsTbl>();
        }

        public int Id { get; set; }
        public decimal MobileNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string DisplayBusinessName { get; set; }
        public string DisplayBusinessDescription { get; set; }
        public string VenderFullName { get; set; }
        public string PreferredTimeSlot { get; set; }
        public string PreferredLanguage { get; set; }
        public string AddLine1 { get; set; }
        public string AddLine2 { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public int CountryId { get; set; }
        public string Landmark { get; set; }
        public decimal PinCode { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public CityMstr City { get; set; }
        public CountryMstr Country { get; set; }
        public StateMstr State { get; set; }
        public ICollection<ColoursTbl> ColoursTbl { get; set; }
        public ICollection<ProductMstr> ProductMstr { get; set; }
        public ICollection<SizesTbl> SizesTbl { get; set; }
        public ICollection<VanderBankDetailsTbl> VanderBankDetailsTbl { get; set; }
    }
}
