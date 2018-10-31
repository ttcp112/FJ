using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class POSEmployeeConfigMap : EntityTypeConfiguration<G_POSEmployeeConfig>
    {
        public POSEmployeeConfigMap()
        {
            this.ToTable("G_POSEmployeeConfig");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(ss => ss.POSAPIMerchantConfigId).IsRequired().HasMaxLength(60);
            this.Property(ss => ss.UserName).IsRequired().HasMaxLength(350);
            this.Property(ss => ss.Password).IsRequired().HasMaxLength(350);
            this.Property(ss => ss.CreatedDate).IsRequired();
            this.Property(ss => ss.IsActived).IsRequired();

        }
    }
}
