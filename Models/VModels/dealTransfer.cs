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
        public decimal currentRating { get; set; }
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
    public class orderByV
    {
        public int id { get; set; }
        public string displayBusinessName { get; set; }
        public string venderFullName { get; set; }
        public int totalCount { get; set; }
        public decimal totalAmount { get; set; }

    }
    public class VMCategoryMstr
    {
        public int Id { get; set; }
        public int? PCatId { get; set; }
        public string Name { get; set; }
    }
    public class VGrowth
    {
        public decimal seleInRupee { get; set; }
        public int seleInUnint { get; set; }
        public int returnUint { get; set; }
        public decimal avgrating { get; set; }
    }
    public class PaymentOverView
    {
        public DateTime nextPay_date { get; set; }
        public Decimal nPostPay { get; set; }
        public Decimal nPrePay { get; set; }
        public Decimal ntotal { get; set; }
        public DateTime lastPay_date { get; set; }
        public Decimal lPostPay { get; set; }
        public Decimal lPrePay { get; set; }
        public Decimal ltotal { get; set; }

    }
    public class returnorderGet
    {
        public int id { get; set; }
        public string sku { get; set; }
        public string productName { get; set; }
        public int qty { get; set; }
        public decimal price { get; set; }
        public string color { get; set; }
        public string size { get; set; }
        public string username { get; set; }
    }
    public class vender_orderget
    {
        public int Id { get; set; }
        public string OrderIdV { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
        public string UserName { get; set; }
        public byte OrderSubStatus { get; set; }

    }
    public class ProductForVender
    {
        public int Id { get; set; }
        public string pic { get; set; }
        public string sku { get; set; }
        public string catname { get; set; }
        public string name { get; set; }
        public decimal CurrentRating { get; set; }
        public bool UserListing { get; set; }
        public List<string> ColoursList { get; set; }
    }


    public class CustProduct
    {

        public int Id { get; set; }
        public int CatId { get; set; }
        public CategoryMstr Cat { get; set; }
        public int VenderId { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }
        public bool IsReturnable { get; set; }
        public decimal ReturnDays { get; set; }
        public string Policy { get; set; }
        public decimal CurrentRating { get; set; }
        public int RatingCount { get; set; }
        public int ReviewCount { get; set; }
        public bool UserListing { get; set; }
        public decimal PackWeight { get; set; }
        public decimal PackLenght { get; set; }
        public decimal PackBreadth { get; set; }
        public decimal PackHeight { get; set; }

        public ICollection<CostSubProduct> SubProductTbl { get; set; }
    }
    public class CostSubProduct
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int SizeId { get; set; }
        public SizesTbl Sizes { get; set; }
        public int ColorId { get; set; }
        public ColoursTbl Color { get; set; }
        public decimal Price { get; set; }
        public decimal OfferPrice { get; set; }
        public decimal Qty { get; set; }
        public ICollection<CustProductImg> ProductImg { get; set; }
        public CustProduct Product { get; set; }
    }
    public class CustProductImg
    {
        public int Id { get; set; }
        public int SubProducatId { get; set; }
        public string Path { get; set; }
    }
    public class CoutOrderTbl
    {
        public CoutOrderTbl()
        {
            ProductImg = new HashSet<string>();
        }

        public int Id { get; set; }
        public string OrderIdV { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalPrice { get; set; }
        public byte Status { get; set; }
        public int PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime CreateDt { get; set; }
        public string CouponCode { get; set; }
        public ICollection<string> ProductImg { get; set; }
    }
    public class CoutOrderDetails
    {
        public CoutOrderDetails()
        {
            MyOrders = new HashSet<OrderDetailsTbl>();
        }
        public int Id { get; set; }
        public string OrderIdV { get; set; }

        public decimal TotalQty { get; set; }
        public decimal TotalPrice { get; set; }
        public byte Status { get; set; }
        public int PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime CreateDt { get; set; }
        public string CouponCode { get; set; }

        public UserMstr User { get; set; }
        public AddressTbl orderaddresses { get; set; }
        public ICollection<OrderDetailsTbl> MyOrders { get; set; }

    }


}
