using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class StockSaleMap : EntityTypeConfiguration<I_StockSale>
    {
        public StockSaleMap()
        {
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.ToTable("I_StockSale");
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.OrderId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.Mode).IsRequired();
            this.Property(aa => aa.Type).IsRequired();
            this.Property(aa => aa.BusinessId).IsOptional().HasMaxLength(50);

            //this.HasOptional(x => x.G_BusinessDay).WithMany().HasForeignKey(d => d.BusinessId).WillCascadeOnDelete(false);
        }
    }
}
