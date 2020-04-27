using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class VenderKycDocumentsTbl
    {
        public int Id { get; set; }
        public int VenderId { get; set; }
        public string DocName { get; set; }
        public bool BusinessType { get; set; }
        public string PanNumber { get; set; }
        public string AddressProof { get; set; }
        public string CanceledCheque { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public VenderMstr Vender { get; set; }
    }
}
