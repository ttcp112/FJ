using NuWebNCloud.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace NuWebNCloud.Data
{
    public class WorkOrderMap : EntityTypeConfiguration<I_Work_Order>
    {
        public WorkOrderMap()
        {
            this.ToTable("I_Work_Order");
            this.HasKey(x => x.Id);
            this.Property(x => x.Id).HasMaxLength(50);

            this.Property(x => x.StoreId).IsRequired().HasMaxLength(50);
            this.Property(x => x.Code).IsRequired().HasMaxLength(50);
            this.Property(x => x.WODate).IsRequired();
            this.Property(x => x.DateCompleted).IsRequired();

            this.Property(x => x.Total).IsOptional();
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
