using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class R_DiscountAndMiscReportMap:EntityTypeConfiguration<R_DiscountAndMiscReport>
    {
        public R_DiscountAndMiscReportMap()
        {
            this.ToTable("R_DiscountAndMiscReport");
            this.HasKey(ss => ss.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(ss => ss.StoreId).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.MiscValue).IsRequired();
            this.Property(ss => ss.DiscountValue).IsRequired();
            this.Property(ss => ss.CreatedDate).IsRequired();
            this.Property(ss => ss.Mode).IsRequired();
        }
    }
}
