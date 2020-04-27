using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class ReturnTbl
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string ReasonTital { get; set; }
        public string ReasonMsg { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public OrderTbl Order { get; set; }
        public UserMstr User { get; set; }
    }
}
