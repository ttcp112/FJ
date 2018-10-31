using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class ShiftLogMap : EntityTypeConfiguration<R_ShiftLog>
    {
        public ShiftLogMap()
        {
            this.ToTable("R_ShiftLog");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.BusinessId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.StartedOn).IsRequired();
            this.Property(aa => aa.ClosedOn).IsRequired();
            this.Property(aa => aa.StartedStaff).IsRequired().HasMaxLength(450);
            this.Property(aa => aa.ClosedStaff).IsRequired().HasMaxLength(450);

        }
    }
}
