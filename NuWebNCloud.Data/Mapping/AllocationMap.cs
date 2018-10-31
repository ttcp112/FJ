using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class AllocationMap : EntityTypeConfiguration<I_Allocation>
    {
        public AllocationMap()
        {
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.ToTable("I_Allocation");
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ApplyDate).IsRequired();
            this.Property(aa => aa.CreatedBy).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ModifierBy).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.ModifierDate).IsRequired();
            this.Property(aa => aa.IsActived).IsRequired();
            this.Property(aa => aa.BusinessId).IsOptional().HasMaxLength(50);
        }
    }
}
