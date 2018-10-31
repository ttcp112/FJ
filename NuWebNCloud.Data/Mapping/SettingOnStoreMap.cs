using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class SettingOnStoreMap : EntityTypeConfiguration<G_SettingOnStore>
    {
        public SettingOnStoreMap()
        {
            this.ToTable("G_SettingOnStore");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50).HasColumnOrder(1);
            this.Property(ss => ss.StoreId).IsRequired().HasMaxLength(50).HasColumnOrder(2);
            this.Property(ss => ss.SettingId).IsRequired().HasMaxLength(50).HasColumnOrder(3);
            this.Property(ss => ss.Value).IsRequired().HasColumnOrder(4);
            this.Property(ss => ss.Status).IsRequired().HasColumnOrder(5);

            this.Property(ss => ss.CreatedDate).IsOptional().HasColumnOrder(6);
            this.Property(ss => ss.CreatedUser).IsOptional().HasMaxLength(250).HasColumnOrder(7);
            this.Property(ss => ss.LastDateModified).IsOptional().HasColumnOrder(8);
            this.Property(ss => ss.LastUserModified).IsOptional().HasMaxLength(250).HasColumnOrder(9);
            this.HasRequired(x => x.G_GeneralSetting).WithMany().HasForeignKey(d => d.SettingId).WillCascadeOnDelete(false);

        }
    }
}
