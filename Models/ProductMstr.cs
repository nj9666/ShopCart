using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class ProductMstr
    {
        public ProductMstr()
        {
            RatingTbl = new HashSet<RatingTbl>();
            SubProductTbl = new HashSet<SubProductTbl>();
        }

        public int Id { get; set; }
        public int CatId { get; set; }
        public int VenderId { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }
        public bool IsReturnable { get; set; }
        public decimal ReturnDays { get; set; }
        public string Policy { get; set; }
        public double CurrentRating { get; set; }
        public int RatingCount { get; set; }
        public int ReviewCount { get; set; }
        public bool UserListing { get; set; }
        public decimal PackWeight { get; set; }
        public decimal PackLenght { get; set; }
        public decimal PackBreadth { get; set; }
        public decimal PackHeight { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public CategoryMstr Cat { get; set; }
        public VenderMstr Vender { get; set; }
        public ICollection<RatingTbl> RatingTbl { get; set; }
        public ICollection<SubProductTbl> SubProductTbl { get; set; }
    }
}
