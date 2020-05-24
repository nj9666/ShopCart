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
    public class venderFadminList
    {
        public int Id { get; set; }
        public string DisplayBusinessName { get; set; }
        public string VenderFullName { get; set; }
        public decimal MobileNumber { get; set; }
        public string Email { get; set; }
        public int Paymant { get; set; }
        public decimal PaymantAmount { get; set; }
        public DateTime? lastPayDate { get; set; }

        public string AccountHolderName { get; set; }
        public decimal AccountNumber { get; set; }
        public string IfscCode { get; set; }
        public string BankName { get; set; }
        public string Branch { get; set; }
    }
    public class orderByP
    {
        public int id { get; set; }
        public string pic { get; set; }
        public string name { get; set; }
        public string sku { get; set; }
        public decimal price { get; set; }
        public string color { get; set; }
        public string size { get; set; }
        public int totalCount { get; set; }
        public decimal totalAmount { get; set; }
        
    }
}
