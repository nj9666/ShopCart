﻿using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class VenderPaymants
    {
        public int Id { get; set; }
        public int VenderId { get; set; }
        public DateTime PaymantDate { get; set; }
        public decimal BankAccount { get; set; }
        public int TransactionId { get; set; }
        public decimal Amount { get; set; }

        public VenderMstr Vender { get; set; }
    }
}
