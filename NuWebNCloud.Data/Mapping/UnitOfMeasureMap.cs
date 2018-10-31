using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class UnitOfMeasureMap : EntityTypeConfiguration<I_UnitOfMeasure>
    {
        public UnitOfMeasureMap()
        {
            this.ToTable("I_UnitOfMeasure");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);

            this.Property(aa => aa.OrganizationId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.Code).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.Name).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.Description).IsOptional().HasMaxLength(500);

            this.Property(aa => aa.CreatedBy).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.UpdatedBy).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.UpdatedDate).IsOptional();
            this.Property(aa => aa.IsActive).IsRequired();
            this.Property(aa => aa.Status).IsOptional();
        }
    }
}
