using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class HourlyItemizedSalesReportMap:EntityTypeConfiguration<R_HourlyItemizedSalesReport>
    {
        public HourlyItemizedSalesReportMap()
        {
            this.ToTable("R_HourlyItemizedSalesReport");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ItemTypeId).IsRequired();
            this.Property(aa => aa.CategoryId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.CategoryName).HasMaxLength(350).IsRequired();
            this.Property(aa => aa.TotalPrice).IsRequired();
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.Mode).IsRequired();

            this.Property(aa => aa.BusinessId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.Discount).IsRequired();
            this.Property(aa => aa.Promotion).IsRequired();
            this.Property(aa => aa.ItemId).IsOptional().HasMaxLength(60);
            this.Property(aa => aa.IsDiscountTotal).IsOptional();
        }
    }
}
