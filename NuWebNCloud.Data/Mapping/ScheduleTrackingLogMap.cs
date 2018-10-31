using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class ScheduleTrackingLogMap : EntityTypeConfiguration<G_ScheduleTrackingLog>
    {
        public ScheduleTrackingLogMap()
        {
            this.ToTable("G_ScheduleTrackingLog");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreIds).IsRequired().HasMaxLength(1000);
            this.Property(pp => pp.ReportId).IsRequired().HasMaxLength(60);
            this.Property(pp => pp.Description).IsOptional().HasMaxLength(1000);
            this.Property(pp => pp.DateSend).IsOptional();
            this.Property(pp => pp.IsSend).IsRequired();
        }
    }
}
