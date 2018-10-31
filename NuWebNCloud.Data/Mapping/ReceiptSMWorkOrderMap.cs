using NuWebNCloud.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace NuWebNCloud.Data
{
    public class ReceiptSMWorkOrderMap : EntityTypeConfiguration<I_ReceiptSelfMade_Work_Order>
    {
        public ReceiptSMWorkOrderMap()
        {
            this.ToTable("I_ReceiptSelfMade_Work_Order");
            this.HasKey(x => x.Id);
            this.Property(x => x.Id).HasMaxLength(50);

            this.Property(x => x.RNSelfMadeId).IsRequired().HasMaxLength(50);
            this.Property(x => x.WorkOrderId).IsRequired().HasMaxLength(50);
            this.Property(x => x.CreatedBy).IsRequired().HasMaxLength(50);
            this.Property(x => x.CreatedDate).IsRequired();
            this.Property(x => x.ModifierBy).IsRequired().HasMaxLength(50);
            this.Property(x => x.ModifierDate).IsRequired();
            this.Property(x => x.IsActived).IsRequired();
        }
    }
}
