using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class MultiLocationConfigMap : EntityTypeConfiguration<G_MultiLocationConfig>
    {
        public MultiLocationConfigMap()
        {
            this.ToTable("G_MultiLocationConfig");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(ss => ss.POSEmployeeConfigId).IsRequired().HasMaxLength(60);
            this.Property(ss => ss.CountryCode).IsRequired().HasMaxLength(60);
            this.Property(ss => ss.UrlWebHost).IsRequired().HasMaxLength(3000);
            this.Property(ss => ss.IsActived).IsRequired();

            this.HasRequired(x => x.G_POSEmployeeConfig).WithMany().HasForeignKey(d => d.POSEmployeeConfigId).WillCascadeOnDelete(false);

        }
    }
}
