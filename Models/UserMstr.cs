using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class UserMstr
    {
        public UserMstr()
        {
            AddressTbl = new HashSet<AddressTbl>();
            CardsTbl = new HashSet<CardsTbl>();
            CartTbl = new HashSet<CartTbl>();
            OrderTbl = new HashSet<OrderTbl>();
            RatingTbl = new HashSet<RatingTbl>();
            ReturnTbl = new HashSet<ReturnTbl>();
            WishlistTbl = new HashSet<WishlistTbl>();
        }

        public int Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Gender { get; set; }
        public DateTime Dob { get; set; }
        public decimal ContactNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string CurrentOtp { get; set; }
        public DateTime? OtpExTime { get; set; }
        public string Token { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<AddressTbl> AddressTbl { get; set; }
        public ICollection<CardsTbl> CardsTbl { get; set; }
        public ICollection<CartTbl> CartTbl { get; set; }
        public ICollection<OrderTbl> OrderTbl { get; set; }
        public ICollection<RatingTbl> RatingTbl { get; set; }
        public ICollection<ReturnTbl> ReturnTbl { get; set; }
        public ICollection<WishlistTbl> WishlistTbl { get; set; }
    }
}
