using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class StockSaleUsageDetailMap : EntityTypeConfiguration<I_StockSaleUsageDetail>
    {
        public StockSaleUsageDetailMap()
        {
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.ToTable("I_StockSaleUsageDetail");
            this.Property(aa => aa.StockSaleDetailId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.IngredientId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.Usage).IsRequired();

            this.HasRequired(x => x.I_StockSaleDetail).WithMany().HasForeignKey(d => d.StockSaleDetailId).WillCascadeOnDelete(false);
            this.HasRequired(x => x.I_Ingredient).WithMany().HasForeignKey(d => d.IngredientId).WillCascadeOnDelete(false);
        }
    }
}
