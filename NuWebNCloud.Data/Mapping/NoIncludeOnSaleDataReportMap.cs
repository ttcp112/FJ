using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class NoIncludeOnSaleDataReportMap : EntityTypeConfiguration<R_NoIncludeOnSaleDataReport> 
    {
        public NoIncludeOnSaleDataReportMap()
        {
            this.ToTable("R_NoIncludeOnSaleDataReport");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CreatedDate).IsRequired();

            this.Property(aa => aa.OrderId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.ReceiptNo).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.BusinessId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.CategoryId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.CategoryName).HasMaxLength(350).IsRequired();
            this.Property(aa => aa.GLAccountCode).IsOptional();
            this.Property(aa => aa.ProductId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.ProductName).IsRequired().HasMaxLength(350);
            this.Property(aa => aa.Qty).IsRequired();
            this.Property(aa => aa.Price).IsRequired();
            this.Property(aa => aa.Amount).IsRequired();
            this.Property(aa => aa.Tax).IsRequired();
            this.Property(aa => aa.ServiceCharged).IsRequired();
            this.Property(aa => aa.DiscountAmount).IsRequired();
            this.Property(aa => aa.PromotionAmount).IsRequired();
            this.Property(aa => aa.Mode).IsRequired();
            
        }
    }
}
