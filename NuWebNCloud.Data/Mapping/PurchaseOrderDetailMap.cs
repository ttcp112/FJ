using NuWebNCloud.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace NuWebNCloud.Data
{
    public class PurchaseOrderDetailMap : EntityTypeConfiguration<I_Purchase_Order_Detail>
    {
        public PurchaseOrderDetailMap()
        {
            this.ToTable("I_Purchase_Order_Detail");
            this.HasKey(x => x.Id);
            this.Property(x => x.Id).HasMaxLength(50);

            this.Property(x => x.PurchaseOrderId).IsRequired().HasMaxLength(50);
            this.Property(x => x.IngredientId).IsRequired().HasMaxLength(50);

            this.Property(x => x.Qty).IsRequired();
            this.Property(x => x.UnitPrice).IsRequired();
            this.Property(x => x.Amount).IsRequired();
            this.Property(x => x.ReceiptNoteQty).IsRequired();
            this.Property(x => x.ReturnReceiptNoteQty).IsRequired();
            this.Property(x => x.Status).IsOptional();
            this.Property(ss => ss.BaseQty).IsOptional();

            //2018-09-06
            this.Property(ss => ss.TaxPercen).IsOptional();
            this.Property(ss => ss.TaxAmount).IsOptional();
            this.Property(ss => ss.TaxId).IsOptional().HasMaxLength(50);
        }
    }
}
