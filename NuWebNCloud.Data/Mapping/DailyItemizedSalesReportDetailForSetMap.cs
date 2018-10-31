using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class DailyItemizedSalesReportDetailForSetMap : EntityTypeConfiguration<R_DailyItemizedSalesReportDetailForSet>
    {
        public DailyItemizedSalesReportDetailForSetMap()
        {
            this.ToTable("R_DailyItemizedSalesReportDetailForSet");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(100);
            this.Property(aa => aa.BusinessId).IsRequired().HasMaxLength(100);
            this.Property(aa => aa.CategoryId).IsRequired().HasMaxLength(100);
            this.Property(aa => aa.CategoryName).IsRequired().HasMaxLength(350);
            this.Property(aa => aa.CategoryTypeId).IsRequired();
            this.Property(aa => aa.Price).IsRequired();
            this.Property(aa => aa.Quantity).IsRequired();
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.Mode).IsRequired();
        }
    }
}
