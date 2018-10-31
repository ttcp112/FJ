using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class EmployeeOnStoreExtendMap : EntityTypeConfiguration<G_EmployeeOnStoreExtend>
    {
        public EmployeeOnStoreExtendMap()
        {
            this.ToTable("G_EmployeeOnStoreExtend");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(ss => ss.EmpOnMerchantExtendId).IsRequired().HasMaxLength(60);
            this.Property(ss => ss.StoreExtendId).IsRequired().HasMaxLength(60);
            this.Property(ss => ss.CreatedDate).IsRequired();
            this.Property(ss => ss.IsActived).IsRequired();

            this.HasRequired(x => x.G_EmployeeOnMerchantExtend).WithMany().HasForeignKey(d => d.EmpOnMerchantExtendId).WillCascadeOnDelete(false);
        }
    }
}
