using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class WishlistTbl
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SubProducatId { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public SubProductTbl SubProducat { get; set; }
        public UserMstr User { get; set; }
    }
}
