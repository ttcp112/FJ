using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class FJDailySaleReportSettingMap : EntityTypeConfiguration<G_FJDailySaleReportSetting>
    {
        public FJDailySaleReportSettingMap()
        {
            this.ToTable("G_FJDailySaleReportSetting");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(ss => ss.StoreId).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.GLAccountCodes).HasMaxLength(250).IsRequired();
            this.Property(ss => ss.CreatedDate).IsRequired();
            this.Property(ss => ss.CreatedUser).IsRequired().HasMaxLength(255);
            this.Property(ss => ss.LastDateModified).IsOptional();
            this.Property(ss => ss.LastUserModified).IsOptional().HasMaxLength(255);
            this.Property(ss => ss.IsActived).IsRequired();
            this.Property(ss => ss.Description).IsOptional().HasMaxLength(500);
        }
    }
}
