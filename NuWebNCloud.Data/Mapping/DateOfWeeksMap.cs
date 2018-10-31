using NuWebNCloud.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace NuWebNCloud.Data
{
    public class DateOfWeeksMap : EntityTypeConfiguration<G_DateOfWeeks>
    {
        public DateOfWeeksMap()
        {
            this.ToTable("G_DateOfWeeks");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.DayName).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.DayNumber).IsRequired();
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.CreatedUser).IsRequired().HasMaxLength(255);
            this.Property(aa => aa.LastDateModified).IsRequired();
            this.Property(aa => aa.LastUserModified).IsRequired().HasMaxLength(255);
        }
    }
}
