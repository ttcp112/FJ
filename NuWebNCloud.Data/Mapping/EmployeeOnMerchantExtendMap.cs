using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class EmployeeOnMerchantExtendMap : EntityTypeConfiguration<G_EmployeeOnMerchantExtend>
    {
        public EmployeeOnMerchantExtendMap()
        {
            this.ToTable("G_EmployeeOnMerchantExtend");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(ss => ss.POSEmployeeConfigId).IsRequired().HasMaxLength(60);
            this.Property(ss => ss.POSAPIMerchantConfigId).IsRequired().HasMaxLength(60);
            this.Property(ss => ss.CreatedDate).IsRequired();
            this.Property(ss => ss.IsActived).IsRequired();

            this.HasRequired(x => x.G_POSAPIMerchantConfig).WithMany().HasForeignKey(d => d.POSAPIMerchantConfigId).WillCascadeOnDelete(false);
            this.HasRequired(x => x.G_POSEmployeeConfig).WithMany().HasForeignKey(d => d.POSEmployeeConfigId).WillCascadeOnDelete(false);

        }
    }
}
