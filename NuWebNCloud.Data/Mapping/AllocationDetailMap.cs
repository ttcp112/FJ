using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class AllocationDetailMap : EntityTypeConfiguration<I_AllocationDetail>
    {
        public AllocationDetailMap()
        {
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.ToTable("I_AllocationDetail");
            this.Property(aa => aa.AllocationId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.IngredientId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.OpenBal).IsRequired();
            this.Property(aa => aa.CloseBal).IsRequired();
            this.Property(aa => aa.Sales).IsRequired();
            this.Property(aa => aa.ActualSold).IsRequired();
            this.Property(aa => aa.Damage).IsRequired();
            this.Property(aa => aa.Wast).IsRequired();
            this.Property(aa => aa.Others).IsRequired();
            this.Property(aa => aa.Reasons).IsRequired().HasMaxLength(3000);

        }
    }
}
