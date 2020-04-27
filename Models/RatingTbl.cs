using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class RatingTbl
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public decimal Rating { get; set; }
        public string Review { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public ProductMstr Product { get; set; }
        public UserMstr User { get; set; }
    }
}
