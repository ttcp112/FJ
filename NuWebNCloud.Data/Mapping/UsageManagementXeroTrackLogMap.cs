using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class UsageManagementXeroTrackLogMap : EntityTypeConfiguration<I_UsageManagementXeroTrackLog>
    {
        public UsageManagementXeroTrackLogMap()
        {
            this.ToTable("I_UsageManagementXeroTrackLog");
            this.HasKey(ss => ss.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(ss => ss.StoreId).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.ToDate).IsRequired();
            this.Property(ss => ss.CreatedDate).IsRequired();
        }
    }
}
