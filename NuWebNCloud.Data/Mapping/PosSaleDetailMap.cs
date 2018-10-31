using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class PosSaleDetailMap : EntityTypeConfiguration<R_PosSaleDetail>
    {
        public PosSaleDetailMap()
        {
            this.ToTable("R_PosSaleDetail");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(60);
            this.Property(aa => aa.BusinessId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.OrderId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.OrderDetailId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ItemId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ItemTypeId).IsRequired();
            this.Property(aa => aa.ParentId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ItemCode).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ItemName).IsRequired().HasMaxLength(350);
            this.Property(aa => aa.CategoryId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CategoryName).IsRequired().HasMaxLength(350);
            this.Property(aa => aa.GLAccountCode).IsOptional().HasMaxLength(50);
            this.Property(aa => aa.Quantity).IsRequired();
            this.Property(aa => aa.Price).IsRequired();
            this.Property(aa => aa.ExtraPrice).IsRequired();
            this.Property(aa => aa.TotalAmount).IsRequired();
            this.Property(aa => aa.Discount).IsRequired();
            this.Property(aa => aa.Cost).IsRequired();
            this.Property(aa => aa.ServiceCharge).IsRequired();
            this.Property(aa => aa.Tax).IsRequired();
            this.Property(aa => aa.PromotionAmount).IsRequired();
            this.Property(aa => aa.PoinsOrderId).IsOptional().HasMaxLength(50);
            this.Property(aa => aa.GiftCardId).IsOptional().HasMaxLength(50);
            this.Property(aa => aa.IsIncludeSale).IsRequired();
            this.Property(aa => aa.CreatedDate).IsRequired();         
            this.Property(aa => aa.Mode).IsRequired();
            this.Property(aa => aa.IsDiscountTotal).IsOptional();
            this.Property(aa => aa.CancelUser).IsOptional().HasMaxLength(350);
            this.Property(aa => aa.RefundUser).IsOptional().HasMaxLength(350);
            this.Property(aa => aa.TaxType).IsOptional();
        }
    }
}
