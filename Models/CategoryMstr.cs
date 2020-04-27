using System;
using System.Collections.Generic;

namespace ShopCart.Models
{
    public partial class CategoryMstr
    {
        public CategoryMstr()
        {
            InversePCat = new HashSet<CategoryMstr>();
            ProductMstr = new HashSet<ProductMstr>();
        }

        public int Id { get; set; }
        public int? PCatId { get; set; }
        public string Name { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public CategoryMstr PCat { get; set; }
        public ICollection<CategoryMstr> InversePCat { get; set; }
        public ICollection<ProductMstr> ProductMstr { get; set; }
    }
}
