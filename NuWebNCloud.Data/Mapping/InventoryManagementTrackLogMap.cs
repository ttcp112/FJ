using NuWebNCloud.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace NuWebNCloud.Data
{
    public class InventoryManagementTrackLogMap : EntityTypeConfiguration<I_InventoryManagementTrackLog>
    {
        public InventoryManagementTrackLogMap()
        {
            this.ToTable("I_InventoryManagementTrackLog");
            this.HasKey(x => x.Id);
            this.Property(x => x.Id).HasMaxLength(50);
            this.Property(x => x.StoreId).IsRequired().HasMaxLength(50);
            this.Property(x => x.IngredientId).IsRequired().HasMaxLength(50);
            this.Property(x => x.TypeCode).IsRequired();
            this.Property(x => x.TypeCodeId).IsOptional().HasMaxLength(50);
            this.Property(x => x.CurrentQty).IsRequired();
            this.Property(x => x.NewQty).IsRequired();
            this.Property(x => x.CreatedBy).IsRequired().HasMaxLength(50);
            this.Property(x => x.CreatedDate).IsRequired();

        }
    }
}
