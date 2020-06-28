using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class AddressTbl
    {
        public AddressTbl()
        {
            OrderTbl = new HashSet<OrderTbl>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public int CountryId { get; set; }
        public string Landmark { get; set; }
        public decimal Zip { get; set; }
        public byte Type { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public CityMstr City { get; set; }
        public CountryMstr Country { get; set; }
        public StateMstr State { get; set; }
        public UserMstr User { get; set; }
        public ICollection<OrderTbl> OrderTbl { get; set; }
    }
}
