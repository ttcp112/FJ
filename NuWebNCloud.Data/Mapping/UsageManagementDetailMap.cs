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
    public class UsageManagementDetailMap : EntityTypeConfiguration<I_UsageManagementDetail> 
    {
        public UsageManagementDetailMap()
        {
            this.ToTable("I_UsageManagementDetail");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.UsageManagementId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.IngredientId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.Usage).IsRequired();
        }
    }
}
