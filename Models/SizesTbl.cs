using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class SizesTbl
    {
        public SizesTbl()
        {
            SubProductTbl = new HashSet<SubProductTbl>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int? VenderId { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public VenderMstr Vender { get; set; }
        public ICollection<SubProductTbl> SubProductTbl { get; set; }
    }
}
