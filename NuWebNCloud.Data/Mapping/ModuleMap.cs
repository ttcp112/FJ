using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class ModuleMap : EntityTypeConfiguration<G_Module>
    {
        public ModuleMap()
        {
            this.ToTable("G_Module");
            this.HasKey(x => x.Id);
            this.Property(x => x.Id).HasMaxLength(100);

            this.Property(x => x.Name).IsRequired().HasMaxLength(255);
            this.Property(x => x.ParentID).HasMaxLength(100);
            this.Property(x => x.Controller).IsRequired().HasMaxLength(100);

            this.Property(x => x.CreatedDate).IsRequired();
            this.Property(x => x.CreatedUser).IsRequired().HasMaxLength(255);
            this.Property(x => x.ModifiedDate).IsRequired();
            this.Property(x => x.ModifiedUser).IsRequired().HasMaxLength(255);
            this.Property(x => x.IndexNum).IsOptional();
        }
    }
}
