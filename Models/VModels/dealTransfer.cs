using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopCart.Models.VModels
{
    public class dealTransfer
    {
        public int Id { get; set; }
        public int AdminId { get; set; }
        public byte DiscountType { get; set; }
        public int DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public List<product_for_deal> ProList { get; set; }
    }

    public class product_for_deal
    {
        public int id { get; set; }
        public int vid { get; set; }
        public string sku { get; set; }
        public string name { get; set; }
        public double currentRating { get; set; }
        public int ratingCount { get; set; }
        public string cat { get; set; }
    }
}
