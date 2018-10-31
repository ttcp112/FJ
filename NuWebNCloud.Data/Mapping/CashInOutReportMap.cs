using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class CashInOutReportMap:EntityTypeConfiguration<R_CashInOutReport>
    {
        public CashInOutReportMap()
        {
            this.ToTable("R_CashInOutReport");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.DrawerId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.DrawerName).HasMaxLength(350).IsRequired();
            this.Property(aa => aa.CashValue).IsRequired();
            this.Property(aa => aa.StartOn).IsRequired();
            this.Property(aa => aa.EndOn).IsRequired();
            this.Property(aa => aa.ShiftEndOn).IsRequired();
            this.Property(aa => aa.ShiftStartOn).IsRequired();
            this.Property(aa => aa.UserName).HasMaxLength(350).IsRequired();
            this.Property(aa => aa.CashType).IsRequired();
            this.Property(aa => aa.Mode).IsRequired();
        }
    }
}
