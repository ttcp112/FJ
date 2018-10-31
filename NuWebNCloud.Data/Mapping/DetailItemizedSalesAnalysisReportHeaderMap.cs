using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class DetailItemizedSalesAnalysisReportHeaderMap:EntityTypeConfiguration<R_DetailItemizedSalesAnalysisReportHeader>
    {
        public DetailItemizedSalesAnalysisReportHeaderMap()
        {
            this.ToTable("R_DetailItemizedSalesAnalysisReportHeader");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired();
            this.Property(aa => aa.CategoryId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.CategoryName).HasMaxLength(350).IsRequired();
            this.Property(aa => aa.ItemId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.ItemName).HasMaxLength(350).IsRequired();
            this.Property(aa => aa.ItemTypeId).IsRequired();
            this.Property(aa => aa.Qty).IsRequired();
            this.Property(aa => aa.Price).IsRequired();
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.Mode).IsRequired();
        }
    }
}
