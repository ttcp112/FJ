using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class StockCountMap : EntityTypeConfiguration<I_StockCount>
    {
        public StockCountMap()
        {
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.ToTable("I_StockCount");
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.Code).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.StockCountDate).IsRequired();
            this.Property(aa => aa.BusinessId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CreatedBy).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ModifierBy).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.ModifierDate).IsRequired();
            this.Property(aa => aa.IsActived).IsRequired();
            this.Property(aa => aa.IsAutoCreated).IsOptional();
            this.Property(aa => aa.Status).IsRequired();
            this.Property(aa => aa.StartedOn).IsOptional();
            this.Property(aa => aa.ClosedOn).IsOptional();
        }
    }
}
