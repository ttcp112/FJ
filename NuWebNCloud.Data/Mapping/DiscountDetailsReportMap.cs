using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class DiscountDetailsReportMap:EntityTypeConfiguration<R_DiscountDetailsReport>
    {
        public DiscountDetailsReportMap()
        {
            this.ToTable("R_DiscountDetailsReport");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired();
            this.Property(aa => aa.BusinessDayId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.ReceiptId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.ReceiptNo).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.CashierId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.CashierName).HasMaxLength(350).IsRequired();
            this.Property(aa => aa.DiscountId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.DiscountName).HasMaxLength(350).IsRequired();
            this.Property(aa => aa.ItemId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.ItemCode).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.ItemName).HasMaxLength(500).IsRequired();
            this.Property(aa => aa.Qty).IsRequired();
            this.Property(aa => aa.ItemPrice).IsRequired();
            this.Property(aa => aa.DiscountAmount).IsRequired();
            this.Property(aa => aa.IsDiscountValue).IsRequired();
            this.Property(aa => aa.BillTotal).IsRequired();
            this.Property(aa => aa.Mode).IsRequired();
            this.Property(aa => aa.UserDiscount).IsOptional().HasMaxLength(350);
            this.Property(aa => aa.PromotionId).IsOptional().HasMaxLength(60);
            this.Property(aa => aa.PromotionName).IsOptional().HasMaxLength(2000);
            this.Property(aa => aa.PromotionValue).IsOptional();
            this.Property(aa => aa.PromotionType).IsOptional();
        }
    }
}
