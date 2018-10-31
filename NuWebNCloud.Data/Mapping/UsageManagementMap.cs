using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class UsageManagementMap : EntityTypeConfiguration<I_UsageManagement> 
    {
        public UsageManagementMap()
        {
            this.ToTable("I_UsageManagement");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired();
            this.Property(aa => aa.BusinessId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.DateFrom).IsRequired();
            this.Property(aa => aa.DateTo).IsRequired();
            this.Property(aa => aa.IsStockInventory).IsRequired();
        }
    }
}
