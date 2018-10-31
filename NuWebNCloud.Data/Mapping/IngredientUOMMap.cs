using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class IngredientUOMMap : EntityTypeConfiguration<I_Ingredient_UOM>
    { 
        public IngredientUOMMap()
        {
            this.ToTable("I_Ingredient_UOM");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);

            this.Property(aa => aa.IngredientId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.UOMId).IsOptional().HasMaxLength(50);
            this.Property(aa => aa.ReceivingQty).IsRequired();

            this.Property(aa => aa.BaseUOM).IsRequired();

            this.Property(aa => aa.CreatedBy).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.UpdatedBy).IsOptional().HasMaxLength(50);
            this.Property(aa => aa.UpdatedDate).IsOptional();
            this.Property(aa => aa.IsActived).IsRequired();

        }
    }
}
