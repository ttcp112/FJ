using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class GeneralSettingMap : EntityTypeConfiguration<G_GeneralSetting>
    {
        public GeneralSettingMap()
        {
            this.ToTable("G_GeneralSetting");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50).HasColumnOrder(1);
            this.Property(ss => ss.Code).IsRequired().HasColumnOrder(2);
            this.Property(ss => ss.Name).IsRequired().HasMaxLength(250).HasColumnOrder(3);
            this.Property(ss => ss.DisplayName).IsRequired().HasMaxLength(350).HasColumnOrder(4);
            this.Property(ss => ss.Value).IsRequired().HasColumnOrder(5);
            this.Property(ss => ss.Status).IsRequired().HasColumnOrder(6);
            this.Property(ss => ss.CreatedDate).IsOptional().HasColumnOrder(7);
            this.Property(ss => ss.CreatedUser).IsOptional().HasMaxLength(250).HasColumnOrder(8);
            this.Property(ss => ss.LastDateModified).IsOptional().HasColumnOrder(9);
            this.Property(ss => ss.LastUserModified).IsOptional().HasMaxLength(250).HasColumnOrder(10);
        }
    }
}
