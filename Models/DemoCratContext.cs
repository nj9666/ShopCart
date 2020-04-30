using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ShopCart.Models
{
    public partial class DemoCratContext : DbContext
    {
        public DemoCratContext()
        {
        }

        public DemoCratContext(DbContextOptions<DemoCratContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AddressTbl> AddressTbl { get; set; }
        public virtual DbSet<AdminMstr> AdminMstr { get; set; }
        public virtual DbSet<CardsTbl> CardsTbl { get; set; }
        public virtual DbSet<CartTbl> CartTbl { get; set; }
        public virtual DbSet<CategoryMstr> CategoryMstr { get; set; }
        public virtual DbSet<CityMstr> CityMstr { get; set; }
        public virtual DbSet<ColoursTbl> ColoursTbl { get; set; }
        public virtual DbSet<CountryMstr> CountryMstr { get; set; }
        public virtual DbSet<CouponMstr> CouponMstr { get; set; }
        public virtual DbSet<OrderDetailsTbl> OrderDetailsTbl { get; set; }
        public virtual DbSet<OrderTbl> OrderTbl { get; set; }
        public virtual DbSet<PaymentTbl> PaymentTbl { get; set; }
        public virtual DbSet<ProductImg> ProductImg { get; set; }
        public virtual DbSet<ProductMstr> ProductMstr { get; set; }
        public virtual DbSet<RatingTbl> RatingTbl { get; set; }
        public virtual DbSet<ReturnTbl> ReturnTbl { get; set; }
        public virtual DbSet<SizesTbl> SizesTbl { get; set; }
        public virtual DbSet<StateMstr> StateMstr { get; set; }
        public virtual DbSet<SubProductTbl> SubProductTbl { get; set; }
        public virtual DbSet<TodayDealsTbl> TodayDealsTbl { get; set; }
        public virtual DbSet<UserMstr> UserMstr { get; set; }
        public virtual DbSet<VanderBankDetailsTbl> VanderBankDetailsTbl { get; set; }
        public virtual DbSet<VenderMstr> VenderMstr { get; set; }
        public virtual DbSet<WishlistTbl> WishlistTbl { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Data Source=DESKTOP-5A3V7O6;Initial Catalog=DemoCrat;Persist Security Info=True;User ID=sa;Password=123;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AddressTbl>(entity =>
            {
                entity.ToTable("address_tbl");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CityId).HasColumnName("city_id");

                entity.Property(e => e.CountryId).HasColumnName("country_id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDefault).HasColumnName("is_default");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.Landmark)
                    .IsRequired()
                    .HasColumnName("landmark")
                    .HasMaxLength(50);

                entity.Property(e => e.Line1)
                    .IsRequired()
                    .HasColumnName("line_1");

                entity.Property(e => e.Line2).HasColumnName("line_2");

                entity.Property(e => e.Mobile)
                    .HasColumnName("mobile")
                    .HasColumnType("numeric(10, 0)");

                entity.Property(e => e.StateId).HasColumnName("state_id");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.Zip)
                    .HasColumnName("zip")
                    .HasColumnType("numeric(10, 0)");

                entity.HasOne(d => d.City)
                    .WithMany(p => p.AddressTbl)
                    .HasForeignKey(d => d.CityId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_address_tbl_city_mstr");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.AddressTbl)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_address_tbl_country_mstr");

                entity.HasOne(d => d.State)
                    .WithMany(p => p.AddressTbl)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_address_tbl_state_mstr");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AddressTbl)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_address_tbl_user_mstr");
            });

            modelBuilder.Entity<AdminMstr>(entity =>
            {
                entity.ToTable("admin_mstr");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ContactNumber)
                    .HasColumnName("contact_number")
                    .HasColumnType("numeric(16, 0)");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasColumnName("first_name")
                    .HasMaxLength(30);

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasColumnName("last_name")
                    .HasMaxLength(30);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password");

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasColumnName("user_name")
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<CardsTbl>(entity =>
            {
                entity.ToTable("cards_tbl");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CardLabel)
                    .HasColumnName("card_label")
                    .HasMaxLength(30);

                entity.Property(e => e.CardNumber)
                    .HasColumnName("card_number")
                    .HasColumnType("numeric(19, 0)");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.ExMonth)
                    .HasColumnName("ex_month")
                    .HasColumnType("numeric(2, 0)");

                entity.Property(e => e.ExYear)
                    .HasColumnName("ex_year")
                    .HasColumnType("numeric(4, 0)");

                entity.Property(e => e.HolderName)
                    .IsRequired()
                    .HasColumnName("holder_name")
                    .HasMaxLength(50);

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDefault).HasColumnName("is_default");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.CardsTbl)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_cards_tbl_user_mstr");
            });

            modelBuilder.Entity<CartTbl>(entity =>
            {
                entity.ToTable("cart_tbl");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.Qty)
                    .HasColumnName("qty")
                    .HasColumnType("numeric(5, 0)");

                entity.Property(e => e.SubProducatId).HasColumnName("sub_producat_id");

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.SubProducat)
                    .WithMany(p => p.CartTbl)
                    .HasForeignKey(d => d.SubProducatId)
                    .HasConstraintName("FK_cart_tbl_sub_product_tbl");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.CartTbl)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_cart_tbl_user_mstr");
            });

            modelBuilder.Entity<CategoryMstr>(entity =>
            {
                entity.ToTable("category_mstr");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(50);

                entity.Property(e => e.PCatId).HasColumnName("p_cat_id");

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.PCat)
                    .WithMany(p => p.InversePCat)
                    .HasForeignKey(d => d.PCatId)
                    .HasConstraintName("FK_category_mstr_category_mstr");
            });

            modelBuilder.Entity<CityMstr>(entity =>
            {
                entity.ToTable("city_mstr");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(20);

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<ColoursTbl>(entity =>
            {
                entity.ToTable("colours_tbl");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(20);

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.VenderId).HasColumnName("vender_id");

                entity.HasOne(d => d.Vender)
                    .WithMany(p => p.ColoursTbl)
                    .HasForeignKey(d => d.VenderId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_colours_tbl_vender_mstr");
            });

            modelBuilder.Entity<CountryMstr>(entity =>
            {
                entity.ToTable("country_mstr");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(20);

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<CouponMstr>(entity =>
            {
                entity.ToTable("coupon_mstr");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AdminId).HasColumnName("admin_id");

                entity.Property(e => e.CoupCode)
                    .IsRequired()
                    .HasColumnName("coup_code")
                    .HasMaxLength(20);

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description");

                entity.Property(e => e.DiscountAmount)
                    .IsRequired()
                    .HasColumnName("discount_amount")
                    .HasMaxLength(20);

                entity.Property(e => e.DiscountType).HasColumnName("discount_type");

                entity.Property(e => e.EndDate)
                    .HasColumnName("end_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(30);

                entity.Property(e => e.StartDate)
                    .HasColumnName("start_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Admin)
                    .WithMany(p => p.CouponMstr)
                    .HasForeignKey(d => d.AdminId)
                    .HasConstraintName("FK_coupon_mstr_admin_mstr");
            });

            modelBuilder.Entity<OrderDetailsTbl>(entity =>
            {
                entity.ToTable("order_details_tbl");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.OrderId).HasColumnName("order_id");

                entity.Property(e => e.OrderSubStatus).HasColumnName("order_sub_status");

                entity.Property(e => e.Qty)
                    .HasColumnName("qty")
                    .HasColumnType("numeric(5, 0)");

                entity.Property(e => e.SubProducatId).HasColumnName("sub_producat_id");

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.VenderPaymantStatus).HasColumnName("vender_paymant_status");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderDetailsTbl)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_order_details_tbl_order_tbl1");

                entity.HasOne(d => d.SubProducat)
                    .WithMany(p => p.OrderDetailsTbl)
                    .HasForeignKey(d => d.SubProducatId)
                    .HasConstraintName("FK_order_details_tbl_sub_product_tbl");
            });

            modelBuilder.Entity<OrderTbl>(entity =>
            {
                entity.ToTable("order_tbl");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AddressId).HasColumnName("address_id");

                entity.Property(e => e.CouponCode)
                    .IsRequired()
                    .HasColumnName("coupon_code")
                    .HasMaxLength(50);

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.OrderIdV).HasColumnName("order_id_v");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.TotalPrice)
                    .HasColumnName("total_price")
                    .HasColumnType("money");

                entity.Property(e => e.TotalQty)
                    .HasColumnName("total_qty")
                    .HasColumnType("numeric(10, 0)");

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Address)
                    .WithMany(p => p.OrderTbl)
                    .HasForeignKey(d => d.AddressId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_order_tbl_address_tbl");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.OrderTbl)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_order_tbl_user_mstr");
            });

            modelBuilder.Entity<PaymentTbl>(entity =>
            {
                entity.ToTable("payment_tbl");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CardId).HasColumnName("card_id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.OrderId).HasColumnName("order_id");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.TransectionId)
                    .IsRequired()
                    .HasColumnName("transection_id");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Card)
                    .WithMany(p => p.PaymentTbl)
                    .HasForeignKey(d => d.CardId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_payment_tbl_cards_tbl");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.PaymentTbl)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_payment_tbl_order_tbl");
            });

            modelBuilder.Entity<ProductImg>(entity =>
            {
                entity.ToTable("product_img");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.Path)
                    .IsRequired()
                    .HasColumnName("path")
                    .HasMaxLength(50);

                entity.Property(e => e.SubProducatId).HasColumnName("sub_producat_id");

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.SubProducat)
                    .WithMany(p => p.ProductImg)
                    .HasForeignKey(d => d.SubProducatId)
                    .HasConstraintName("FK_product_img_sub_product_tbl");
            });

            modelBuilder.Entity<ProductMstr>(entity =>
            {
                entity.ToTable("product_mstr");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CatId).HasColumnName("cat_id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.CurrentRating).HasColumnName("current_rating");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.IsReturnable).HasColumnName("is_returnable");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name");

                entity.Property(e => e.PackBreadth)
                    .HasColumnName("pack_breadth")
                    .HasColumnType("numeric(5, 0)");

                entity.Property(e => e.PackHeight)
                    .HasColumnName("pack_height")
                    .HasColumnType("numeric(5, 0)");

                entity.Property(e => e.PackLenght)
                    .HasColumnName("pack_lenght")
                    .HasColumnType("numeric(5, 0)");

                entity.Property(e => e.PackWeight)
                    .HasColumnName("pack_weight")
                    .HasColumnType("numeric(5, 0)");

                entity.Property(e => e.Policy)
                    .IsRequired()
                    .HasColumnName("policy");

                entity.Property(e => e.RatingCount).HasColumnName("rating_count");

                entity.Property(e => e.ReturnDays)
                    .HasColumnName("return_days")
                    .HasColumnType("numeric(2, 0)");

                entity.Property(e => e.ReviewCount).HasColumnName("review_count");

                entity.Property(e => e.Sku)
                    .IsRequired()
                    .HasColumnName("sku")
                    .HasMaxLength(10);

                entity.Property(e => e.Tags)
                    .IsRequired()
                    .HasColumnName("tags");

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.UserListing).HasColumnName("user_listing");

                entity.Property(e => e.VenderId).HasColumnName("vender_id");

                entity.HasOne(d => d.Cat)
                    .WithMany(p => p.ProductMstr)
                    .HasForeignKey(d => d.CatId)
                    .HasConstraintName("FK_product_mstr_category_mstr");

                entity.HasOne(d => d.Vender)
                    .WithMany(p => p.ProductMstr)
                    .HasForeignKey(d => d.VenderId)
                    .HasConstraintName("FK_product_mstr_vender_mstr");
            });

            modelBuilder.Entity<RatingTbl>(entity =>
            {
                entity.ToTable("rating_tbl");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ProductId).HasColumnName("product_id");

                entity.Property(e => e.Rating)
                    .HasColumnName("rating")
                    .HasColumnType("numeric(2, 0)");

                entity.Property(e => e.Review)
                    .IsRequired()
                    .HasColumnName("review");

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.RatingTbl)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rating_tbl_product_mstr");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.RatingTbl)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_rating_tbl_user_mstr");
            });

            modelBuilder.Entity<ReturnTbl>(entity =>
            {
                entity.ToTable("return_tbl");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.OrderId).HasColumnName("order_id");

                entity.Property(e => e.ReasonMsg)
                    .IsRequired()
                    .HasColumnName("reason_msg");

                entity.Property(e => e.ReasonTital)
                    .IsRequired()
                    .HasColumnName("reason_tital")
                    .HasMaxLength(250);

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.ReturnTbl)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_return_tbl_order_tbl");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ReturnTbl)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_return_tbl_user_mstr");
            });

            modelBuilder.Entity<SizesTbl>(entity =>
            {
                entity.ToTable("sizes_tbl");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(20);

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.VenderId).HasColumnName("vender_id");

                entity.HasOne(d => d.Vender)
                    .WithMany(p => p.SizesTbl)
                    .HasForeignKey(d => d.VenderId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_sizes_tbl_vender_mstr");
            });

            modelBuilder.Entity<StateMstr>(entity =>
            {
                entity.ToTable("state_mstr");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(20);

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<SubProductTbl>(entity =>
            {
                entity.ToTable("sub_product_tbl");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ColorId).HasColumnName("color_id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.OfferPrice)
                    .HasColumnName("offer_price")
                    .HasColumnType("money");

                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasColumnType("money");

                entity.Property(e => e.ProductId).HasColumnName("product_id");

                entity.Property(e => e.Qty)
                    .HasColumnName("qty")
                    .HasColumnType("numeric(10, 0)");

                entity.Property(e => e.SizeId).HasColumnName("size_id");

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Color)
                    .WithMany(p => p.SubProductTbl)
                    .HasForeignKey(d => d.ColorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sub_product_tbl_colours_tbl");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.SubProductTbl)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_sub_product_tbl_product_mstr");

                entity.HasOne(d => d.Size)
                    .WithMany(p => p.SubProductTbl)
                    .HasForeignKey(d => d.SizeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sub_product_tbl_sizes_tbl");
            });

            modelBuilder.Entity<TodayDealsTbl>(entity =>
            {
                entity.ToTable("today_deals_tbl");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AdminId).HasColumnName("admin_id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.DiscountAmount).HasColumnName("discount_amount");

                entity.Property(e => e.DiscountType).HasColumnName("discount_type");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.StartDate)
                    .HasColumnName("start_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.SubProId).HasColumnName("sub_pro_id");

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Admin)
                    .WithMany(p => p.TodayDealsTbl)
                    .HasForeignKey(d => d.AdminId)
                    .HasConstraintName("FK_today_deals_tbl_admin_mstr");

                entity.HasOne(d => d.SubPro)
                    .WithMany(p => p.TodayDealsTbl)
                    .HasForeignKey(d => d.SubProId)
                    .HasConstraintName("FK_today_deals_tbl_sub_product_tbl");
            });

            modelBuilder.Entity<UserMstr>(entity =>
            {
                entity.ToTable("user_mstr");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ContactNumber)
                    .HasColumnName("contact_number")
                    .HasColumnType("numeric(16, 0)");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.CurrentOtp)
                    .HasColumnName("current_otp")
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.Dob)
                    .HasColumnName("dob")
                    .HasColumnType("date");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email")
                    .HasMaxLength(50);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasColumnName("first_name")
                    .HasMaxLength(50);

                entity.Property(e => e.Gender).HasColumnName("gender");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasColumnName("last_name")
                    .HasMaxLength(50);

                entity.Property(e => e.OtpExTime)
                    .HasColumnName("otp_ex_time")
                    .HasColumnType("datetime");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password");

                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasColumnName("token");

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasColumnName("user_name")
                    .HasMaxLength(30);
            });

            modelBuilder.Entity<VanderBankDetailsTbl>(entity =>
            {
                entity.ToTable("vander_bank_details_tbl");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountHolderName)
                    .IsRequired()
                    .HasColumnName("account_holder_name")
                    .HasMaxLength(50);

                entity.Property(e => e.AccountNumber)
                    .HasColumnName("account_number")
                    .HasColumnType("numeric(34, 0)");

                entity.Property(e => e.BankName)
                    .IsRequired()
                    .HasColumnName("bank_name")
                    .HasMaxLength(50);

                entity.Property(e => e.Branch)
                    .IsRequired()
                    .HasColumnName("branch")
                    .HasMaxLength(20);

                entity.Property(e => e.CityId).HasColumnName("city_id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.IfscCode)
                    .IsRequired()
                    .HasColumnName("ifsc_code")
                    .HasMaxLength(15);

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.StateId).HasColumnName("state_id");

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.VenderId).HasColumnName("vender_Id");

                entity.HasOne(d => d.City)
                    .WithMany(p => p.VanderBankDetailsTbl)
                    .HasForeignKey(d => d.CityId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_vander_bank_details_tbl_city_mstr");

                entity.HasOne(d => d.State)
                    .WithMany(p => p.VanderBankDetailsTbl)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_vander_bank_details_tbl_state_mstr");

                entity.HasOne(d => d.Vender)
                    .WithMany(p => p.VanderBankDetailsTbl)
                    .HasForeignKey(d => d.VenderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_vander_bank_details_tbl_vender_mstr");
            });

            modelBuilder.Entity<VenderMstr>(entity =>
            {
                entity.ToTable("vender_mstr");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AddLine1)
                    .IsRequired()
                    .HasColumnName("add_line_1");

                entity.Property(e => e.AddLine2).HasColumnName("add_line_2");

                entity.Property(e => e.CityId).HasColumnName("city_id");

                entity.Property(e => e.CountryId).HasColumnName("country_id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.DisplayBusinessDescription)
                    .IsRequired()
                    .HasColumnName("display_business_description");

                entity.Property(e => e.DisplayBusinessName)
                    .IsRequired()
                    .HasColumnName("display_business_name")
                    .HasMaxLength(50);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email")
                    .HasMaxLength(50);

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.Landmark)
                    .IsRequired()
                    .HasColumnName("landmark")
                    .HasMaxLength(20);

                entity.Property(e => e.MobileNumber)
                    .HasColumnName("mobile_number")
                    .HasColumnType("numeric(16, 0)");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasMaxLength(50);

                entity.Property(e => e.PinCode)
                    .HasColumnName("pin_code")
                    .HasColumnType("numeric(10, 0)");

                entity.Property(e => e.PreferredLanguage)
                    .IsRequired()
                    .HasColumnName("preferred_language")
                    .HasMaxLength(20);

                entity.Property(e => e.PreferredTimeSlot)
                    .IsRequired()
                    .HasColumnName("preferred_time_slot")
                    .HasMaxLength(50);

                entity.Property(e => e.StateId).HasColumnName("state_id");

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.VenderFullName)
                    .IsRequired()
                    .HasColumnName("vender_full_name");

                entity.HasOne(d => d.City)
                    .WithMany(p => p.VenderMstr)
                    .HasForeignKey(d => d.CityId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_vender_mstr_city_mstr");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.VenderMstr)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_vender_mstr_country_mstr");

                entity.HasOne(d => d.State)
                    .WithMany(p => p.VenderMstr)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_vender_mstr_state_mstr");
            });

            modelBuilder.Entity<WishlistTbl>(entity =>
            {
                entity.ToTable("wishlist_tbl");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateDt)
                    .HasColumnName("create_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.SubProducatId).HasColumnName("sub_producat_id");

                entity.Property(e => e.UpdateDt)
                    .HasColumnName("update_dt")
                    .HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.SubProducat)
                    .WithMany(p => p.WishlistTbl)
                    .HasForeignKey(d => d.SubProducatId)
                    .HasConstraintName("FK_wishlist_tbl_sub_product_tbl");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.WishlistTbl)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_wishlist_tbl_user_mstr");
            });
        }
    }
}
