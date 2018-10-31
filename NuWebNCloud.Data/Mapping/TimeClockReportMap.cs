using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class TimeClockReportMap : EntityTypeConfiguration<R_TimeClockReport>
    {
        public TimeClockReportMap()
        {
            this.ToTable("R_TimeClockReport");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(ss => ss.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(ss => ss.DayOfWeeks).IsRequired();
            this.Property(ss => ss.UserId).HasMaxLength(50).IsRequired();
            this.Property(ss => ss.UserName).HasMaxLength(350).IsRequired();
            this.Property(ss => ss.DateTimeIn).IsRequired();
            this.Property(ss => ss.DateTimeOut).IsRequired();
            this.Property(ss => ss.Mode).IsRequired();
            this.Property(aa => aa.Early).IsRequired();
            this.Property(aa => aa.Late).IsRequired();
            this.Property(aa => aa.HoursWork).IsRequired();
            this.Property(aa => aa.BusinessId).IsOptional().HasMaxLength(100);

        }
    }
}
