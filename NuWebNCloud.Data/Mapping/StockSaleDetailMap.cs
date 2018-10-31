using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class StockSaleDetailMap : EntityTypeConfiguration<I_StockSaleDetail>
    {
        public StockSaleDetailMap()
        {
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.ToTable("I_StockSaleDetail");
            this.Property(aa => aa.StockSaleId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ProductId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ProductName).IsOptional().HasMaxLength(350);
            this.Property(aa => aa.Qty).IsRequired();
            this.Property(aa => aa.IsCheckStock).IsOptional();

            this.HasRequired(x => x.I_StockSale).WithMany().HasForeignKey(d => d.StockSaleId).WillCascadeOnDelete(false);
        }
    }
}
