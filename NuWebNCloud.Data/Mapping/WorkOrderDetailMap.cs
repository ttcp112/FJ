using NuWebNCloud.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace NuWebNCloud.Data
{
    public class WorkOrderDetailMap : EntityTypeConfiguration<I_Work_Order_Detail>
    {
        public WorkOrderDetailMap()
        {
            this.ToTable("I_Work_Order_Detail");
            this.HasKey(x => x.Id);
            this.Property(x => x.Id).HasMaxLength(50);

            this.Property(x => x.WorkOrderId).IsRequired().HasMaxLength(50);
            this.Property(x => x.IngredientId).IsRequired().HasMaxLength(50);
            this.Property(x => x.Qty).IsRequired();
            this.Property(x => x.UnitPrice).IsOptional();
            this.Property(x => x.Amount).IsOptional();
            this.Property(x => x.ReceiptNoteQty).IsOptional();
            this.Property(x => x.ReturnReceiptNoteQty).IsOptional();
            this.Property(x => x.Status).IsOptional();
            this.Property(ss => ss.BaseQty).IsOptional();
        }
    }
}
