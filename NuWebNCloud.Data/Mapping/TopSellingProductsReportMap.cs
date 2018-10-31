using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class TopSellingProductsReportMap: EntityTypeConfiguration<R_TopSellingProductsReport>
    {
        public TopSellingProductsReportMap()
        {
            this.ToTable("R_TopSellingProductsReport");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.BusinessDayId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.OrderId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.OrderDetailId).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.StoreId).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.CreatedDate).IsRequired();
            this.Property(ss => ss.ItemId).HasMaxLength(50).IsRequired();
            this.Property(ss => ss.ItemCode).HasMaxLength(50).IsRequired();
            this.Property(ss => ss.ItemName).HasMaxLength(500).IsRequired();
            this.Property(ss => ss.Qty).IsRequired();
            this.Property(ss => ss.Discount).IsRequired();
            this.Property(ss => ss.Amount).IsRequired();
            this.Property(ss => ss.PromotionAmount).IsRequired();
            this.Property(ss => ss.Tax).IsRequired();
            this.Property(ss => ss.ServiceCharged).IsRequired();
            this.Property(ss => ss.Mode).IsRequired();
        }
    }
}
