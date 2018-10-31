using NuWebNCloud.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace NuWebNCloud.Data
{
    public class StockTransferDetailMap : EntityTypeConfiguration<I_Stock_Transfer_Detail>
    {
        public StockTransferDetailMap()
        {
            this.ToTable("I_Stock_Transfer_Detail");
            this.HasKey(x => x.Id);
            this.Property(x => x.Id).HasMaxLength(50);

            this.Property(x => x.StockTransferId).IsRequired().HasMaxLength(50);
            this.Property(x => x.IngredientId).HasMaxLength(50);
            

            this.Property(x => x.RequestQty).IsRequired();
            this.Property(x => x.IssueQty).IsRequired();
            this.Property(x => x.ReceiveQty).IsRequired();

            this.Property(x => x.UOMId).IsRequired().HasMaxLength(50);

            this.Property(x => x.IssueBaseQty).IsOptional();
            this.Property(x => x.ReceiveBaseQty).IsOptional();
        }
    }
}
