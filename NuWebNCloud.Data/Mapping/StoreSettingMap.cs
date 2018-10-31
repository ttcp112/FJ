using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class StoreSettingMap : EntityTypeConfiguration<I_StoreSetting>
    {
        public StoreSettingMap()
        {
            this.ToTable("I_StoreSetting");
            this.HasKey(ss => ss.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.IngredientId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ReorderingQuantity).IsRequired();
            this.Property(aa => aa.MinAltert).IsRequired();
            this.Property(aa => aa.CreatedBy).IsRequired().HasMaxLength(250);
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.UpdatedBy).IsOptional().HasMaxLength(250);
            this.Property(aa => aa.UpdatedDate).IsOptional();
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
        }
    }
}
