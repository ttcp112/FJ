using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class ItemizedSalesAnalysisReportDetailMap : EntityTypeConfiguration<R_ItemizedSalesAnalysisReportDetail>
    {
        public ItemizedSalesAnalysisReportDetailMap()
        {
            this.ToTable("R_ItemizedSalesAnalysisReportDetail");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.BusinessId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ItemId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.ItemCode).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ItemTypeId).IsRequired();
            this.Property(aa => aa.ItemName).HasMaxLength(350).IsRequired();
            this.Property(aa => aa.ParentId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.Price).IsRequired();
            this.Property(aa => aa.Quantity).IsRequired();
            this.Property(aa => aa.TotalAmount).IsRequired();
            this.Property(aa => aa.CategoryId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.CategoryName).HasMaxLength(350).IsRequired();
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.Mode).IsRequired();
        }
    }
}
