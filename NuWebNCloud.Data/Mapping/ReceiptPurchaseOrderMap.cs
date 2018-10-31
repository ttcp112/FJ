using NuWebNCloud.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace NuWebNCloud.Data
{
    public class ReceiptPurchaseOrderMap : EntityTypeConfiguration<I_Receipt_Purchase_Order>
    {
        public ReceiptPurchaseOrderMap()
        {
            this.ToTable("I_Receipt_Purchase_Order");
            this.HasKey(x => x.Id);
            this.Property(x => x.Id).HasMaxLength(50);

            this.Property(x => x.ReceiptNoteId).IsRequired().HasMaxLength(50);
            this.Property(x => x.PurchaseOrderId).IsRequired().HasMaxLength(50);
            //this.Property(x => x.PurchaseOrderNo).IsRequired().HasMaxLength(50);
            this.Property(x => x.CreatedBy).IsRequired().HasMaxLength(50);
            this.Property(x => x.CreatedDate).IsRequired();
            this.Property(x => x.ModifierBy).IsRequired().HasMaxLength(50);
            this.Property(x => x.ModifierDate).IsRequired();
            this.Property(x => x.IsActived).IsRequired();
        }
    }
}
