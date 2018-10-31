using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class IndustryMap : EntityTypeConfiguration<G_Industry>
    {
        public IndustryMap()
        {
            this.ToTable("G_Industry");
            this.HasKey(aa => aa.ID);
            this.Property(aa => aa.ID).HasMaxLength(50);
            this.Property(aa => aa.Name).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.IsPublic).IsRequired();
            this.Property(aa => aa.Status).IsRequired();
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.CreatedUser).IsRequired().HasMaxLength(255); ;
            this.Property(aa => aa.ModifiedUser).IsRequired().HasMaxLength(255);
            this.Property(aa => aa.LastModified).IsRequired();
            this.Property(aa => aa.AreaName).IsRequired().HasMaxLength(50);
        }
    }
}
