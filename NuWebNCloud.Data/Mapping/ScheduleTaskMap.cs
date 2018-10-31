using System.Data.Entity.ModelConfiguration;
using NuWebNCloud.Data.Entities;

namespace NuWebNCloud.Data
{
    public partial class ScheduleTaskMap : EntityTypeConfiguration<G_ScheduleTask>
    {
        public ScheduleTaskMap()
        {
            this.ToTable("G_ScheduleTask");
            this.HasKey(t => t.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(t => t.ReportId).IsRequired().HasMaxLength(100);
            this.Property(aa => aa.ReportName).IsOptional().HasMaxLength(350);
            this.Property(aa => aa.EmailSubject).IsRequired().HasMaxLength(450);
            this.Property(aa => aa.Email).IsRequired().HasMaxLength(250);
            this.Property(aa => aa.Cc).HasMaxLength(1000).IsOptional();
            this.Property(aa => aa.Bcc).HasMaxLength(1000).IsOptional();
            //this.Property(aa => aa.DayOfWeeks).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.Hour).IsRequired();
            this.Property(aa => aa.Minute).IsRequired();
            this.Property(aa => aa.Enabled).IsRequired();
            this.Property(aa => aa.IsDaily).IsRequired();
            this.Property(aa => aa.IsWeekly).IsRequired();
            this.Property(aa => aa.IsMonthly).IsRequired();

            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.CreatedUser).IsRequired().HasMaxLength(250);
            this.Property(aa => aa.LastUserModified).IsRequired().HasMaxLength(250);
            this.Property(aa => aa.LastDateModified).IsRequired();
        }
    }
}