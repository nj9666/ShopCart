using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class ProductImg
    {
        public int Id { get; set; }
        public int SubProducatId { get; set; }
        public string Path { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public SubProductTbl SubProducat { get; set; }
    }
}
