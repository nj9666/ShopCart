using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class CityMstr
    {
        public CityMstr()
        {
            AddressTbl = new HashSet<AddressTbl>();
            VanderBankDetailsTbl = new HashSet<VanderBankDetailsTbl>();
            VenderMstr = new HashSet<VenderMstr>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<AddressTbl> AddressTbl { get; set; }
        public ICollection<VanderBankDetailsTbl> VanderBankDetailsTbl { get; set; }
        public ICollection<VenderMstr> VenderMstr { get; set; }
    }
}
