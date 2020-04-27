using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class SubProductTbl
    {
        public SubProductTbl()
        {
            CartTbl = new HashSet<CartTbl>();
            OrderDetailsTbl = new HashSet<OrderDetailsTbl>();
            ProductImg = new HashSet<ProductImg>();
            TodayDealsTbl = new HashSet<TodayDealsTbl>();
            WishlistTbl = new HashSet<WishlistTbl>();
        }

        public int Id { get; set; }
        public int ProductId { get; set; }
        public int SizeId { get; set; }
        public int ColorId { get; set; }
        public decimal Price { get; set; }
        public decimal OfferPrice { get; set; }
        public decimal Qty { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public ColoursTbl Color { get; set; }
        public ProductMstr Product { get; set; }
        public SizesTbl Size { get; set; }
        public ICollection<CartTbl> CartTbl { get; set; }
        public ICollection<OrderDetailsTbl> OrderDetailsTbl { get; set; }
        public ICollection<ProductImg> ProductImg { get; set; }
        public ICollection<TodayDealsTbl> TodayDealsTbl { get; set; }
        public ICollection<WishlistTbl> WishlistTbl { get; set; }
    }
}
