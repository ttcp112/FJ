using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class StockCountDetailMap : EntityTypeConfiguration<I_StockCountDetail>
    {
        public StockCountDetailMap()
        {
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.ToTable("I_StockCountDetail");
            this.Property(aa => aa.StockCountId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.IngredientId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CloseBal).IsRequired();
            this.Property(aa => aa.OpenBal).IsOptional();
            this.Property(aa => aa.Damage).IsOptional();
            this.Property(aa => aa.Wastage).IsOptional();
            this.Property(aa => aa.OtherQty).IsOptional();
            this.Property(aa => aa.Reasons).IsOptional();
            this.Property(aa => aa.AutoCloseBal).IsOptional();
        }
    }
}
