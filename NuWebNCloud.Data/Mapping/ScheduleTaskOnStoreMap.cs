using System.Data.Entity.ModelConfiguration;
using NuWebNCloud.Data.Entities;

namespace NuWebNCloud.Data
{
    public partial class ScheduleTaskOnStoreMap : EntityTypeConfiguration<G_ScheduleTaskOnStore>
    {
        public ScheduleTaskOnStoreMap()
        {
            this.ToTable("G_ScheduleTaskOnStore");
            this.HasKey(t => t.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(t => t.ScheduleTaskId).IsRequired().HasMaxLength(60);
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(60);
            this.Property(aa => aa.Description).IsOptional().HasMaxLength(450);

            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.CreatedUser).IsRequired().HasMaxLength(250);
            this.Property(aa => aa.LastUserModified).IsRequired().HasMaxLength(250);
            this.Property(aa => aa.LastDateModified).IsRequired();

            this.HasRequired(x => x.G_ScheduleTask).WithMany().HasForeignKey(d => d.ScheduleTaskId).WillCascadeOnDelete(false);
        }
    }
}