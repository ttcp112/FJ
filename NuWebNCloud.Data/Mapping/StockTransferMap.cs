using NuWebNCloud.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace NuWebNCloud.Data
{
    public class StockTransferMap : EntityTypeConfiguration<I_Stock_Transfer>
    {
        public StockTransferMap()
        {
            this.ToTable("I_Stock_Transfer");
            this.HasKey(x => x.Id);
            this.Property(x => x.Id).HasMaxLength(50);
            this.Property(x => x.StockTransferNo).IsRequired().HasMaxLength(50);
            this.Property(x => x.IssueStoreId).IsRequired().HasMaxLength(50);
            this.Property(x => x.ReceiveStoreId).HasMaxLength(50);
            this.Property(x => x.RequestBy).IsRequired().HasMaxLength(50);

            this.Property(x => x.RequestDate).IsRequired();
            this.Property(x => x.IssueBy).IsRequired().HasMaxLength(50);
            this.Property(x => x.IssueDate).IsRequired();
            this.Property(x => x.ReceiveBy).IsRequired().HasMaxLength(50);
            this.Property(x => x.ReceiveDate).IsRequired();
            this.Property(x => x.IsActive).IsRequired();
            this.Property(x => x.BusinessId).IsOptional().HasMaxLength(50);
            this.Property(x => x.BusinessReceiveId).IsOptional().HasMaxLength(50);
        }
    }
}
