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
    public class UsageManagementItemDetailMap : EntityTypeConfiguration<I_UsageManagementItemDetail> 
    {
        public UsageManagementItemDetailMap()
        {
            this.ToTable("I_UsageManagementItemDetail");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.UsageManagementDetailId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.IndexList).IsRequired();
            this.Property(aa => aa.BusinessDay).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ItemId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ItemName).IsRequired().HasMaxLength(250);
            this.Property(aa => aa.Qty).IsRequired();
            this.Property(aa => aa.Usage).IsRequired();
        }
    }
}
