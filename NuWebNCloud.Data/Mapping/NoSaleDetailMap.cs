using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class NoSaleDetailMap : EntityTypeConfiguration<R_NoSaleDetailReport>
    {
        public NoSaleDetailMap()
        {
            this.ToTable("R_NoSaleDetailReport");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.CashierId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CashierName).IsRequired().HasMaxLength(350);
            this.Property(aa => aa.DrawerId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.DrawerName).IsRequired().HasMaxLength(350);
            this.Property(aa => aa.Reason).IsRequired();
            this.Property(aa => aa.Mode).IsRequired();
            this.Property(aa => aa.BusinessId).IsOptional().HasMaxLength(60);
            this.Property(aa => aa.ShiftId).IsOptional().HasMaxLength(60);
            this.Property(aa => aa.StartedShift).IsOptional();
            this.Property(aa => aa.ClosedShift).IsOptional();
        }
    }
}
