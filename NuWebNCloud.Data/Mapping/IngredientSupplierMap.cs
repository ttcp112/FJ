using NuWebNCloud.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace NuWebNCloud.Data
{
    public class IngredientSupplierMap : EntityTypeConfiguration<I_Ingredient_Supplier>
    {
        public IngredientSupplierMap()
        {
            this.ToTable("I_Ingredient_Supplier");
            this.HasKey(x => x.Id);
            this.Property(x => x.Id).HasMaxLength(50);
            this.Property(x => x.IngredientId).IsRequired().HasMaxLength(50);
            this.Property(x => x.SupplierId).IsRequired().HasMaxLength(50);

            this.Property(x => x.CreatedBy).IsRequired().HasMaxLength(50);
            this.Property(x => x.CreatedDate).IsRequired();
            this.Property(x => x.ModifierBy).IsRequired().HasMaxLength(50);
            this.Property(x => x.ModifierDate).IsRequired();
            this.Property(x => x.IsActived).IsRequired();
        }
    }
}
