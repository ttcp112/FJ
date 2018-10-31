using NuWebNCloud.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace NuWebNCloud.Data
{
    public class PurchaseOrderMap : EntityTypeConfiguration<I_Purchase_Order>
    {
        public PurchaseOrderMap()
        {
            this.ToTable("I_Purchase_Order");
            this.HasKey(x => x.Id);
            this.Property(x => x.Id).HasMaxLength(50);

            this.Property(x => x.StoreId).IsRequired().HasMaxLength(50);
            this.Property(x => x.Code).IsRequired().HasMaxLength(50);
            this.Property(x => x.SupplierId).IsRequired().HasMaxLength(50);
            this.Property(x => x.PODate).IsRequired();
            this.Property(x => x.DeliveryDate).IsRequired();

            this.Property(x => x.TaxType).IsRequired();
            this.Property(x => x.TaxPercen).IsRequired();
            this.Property(x => x.TaxPercen).IsRequired();
            this.Property(x => x.TaxAmount).IsRequired();
            this.Property(x => x.Additional).IsRequired();
            this.Property(x => x.AdditionalReason).IsOptional().HasMaxLength(3000);
            this.Property(x => x.Total).IsRequired();
            this.Property(x => x.Status).IsRequired();

            this.Property(x => x.CreatedBy).IsRequired().HasMaxLength(50);
            this.Property(x => x.CreatedDate).IsRequired();

            this.Property(x => x.ModifierBy).IsRequired().HasMaxLength(50);
            this.Property(x => x.ModifierDate).IsRequired();

            this.Property(x => x.IsActived).IsRequired();
            this.Property(x => x.Note).IsOptional().HasMaxLength(3000);
        }
    }
}
