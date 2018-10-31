using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class StockUsageMap : EntityTypeConfiguration<I_StockUsage>
    {
        public StockUsageMap()
        {
            this.ToTable("I_StockUsage");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ItemId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ItemName).IsRequired().HasMaxLength(250);
            this.Property(aa => aa.Quantity).IsRequired();
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.Mode).IsRequired();
        }
    }
}
