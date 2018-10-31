using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class ItemizedSalesAnalysisReportMap:EntityTypeConfiguration<R_ItemizedSalesAnalysisReport>
    {
        public ItemizedSalesAnalysisReportMap()
        {
            this.ToTable("R_ItemizedSalesAnalysisReport");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ItemId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.ItemTypeId).IsRequired();
            this.Property(aa => aa.ItemName).HasMaxLength(350).IsRequired();
            this.Property(aa => aa.Price).IsRequired();
            this.Property(aa => aa.ExtraPrice).IsRequired();
            this.Property(aa => aa.TotalPrice).IsRequired();
            this.Property(aa => aa.Quantity).IsRequired();
            this.Property(aa => aa.Discount).IsRequired();
            this.Property(aa => aa.Cost).IsRequired();
            this.Property(aa => aa.CategoryId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.CategoryName).HasMaxLength(350).IsRequired();
            this.Property(aa => aa.GLAccountCode).IsOptional();
            this.Property(aa => aa.ServiceCharge).IsRequired();
            this.Property(aa => aa.Tax).IsRequired();
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.ItemCode).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.PromotionAmount).IsRequired();
            this.Property(aa => aa.Mode).IsRequired();
            this.Property(aa => aa.BusinessId).IsOptional();
            this.Property(aa => aa.IsIncludeSale).IsOptional();
            this.Property(aa => aa.TotalAmount).IsOptional();
            this.Property(aa => aa.TotalDiscount).IsOptional();
            this.Property(aa => aa.ExtraAmount).IsOptional();
            this.Property(aa => aa.ReceiptId).IsOptional();
            this.Property(aa => aa.PoinsOrderId).HasMaxLength(60).IsOptional();
            this.Property(aa => aa.GiftCardId).HasMaxLength(60).IsOptional();
            this.Property(aa => aa.TaxType).IsOptional();
        }
    }
}
