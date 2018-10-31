using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class HourlySalesReportMap:EntityTypeConfiguration<R_HourlySalesReport>
    {
        public HourlySalesReportMap()
        {
            this.ToTable("R_HourlySalesReport");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ReceiptTotal).IsRequired();
            this.Property(aa => aa.NetSales).IsRequired();
            this.Property(aa => aa.ReceiptId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.NoOfPerson).IsRequired();
            this.Property(aa => aa.Mode).IsRequired();
            this.Property(aa => aa.CreditNoteNo).IsOptional().HasMaxLength(50);
        }
    }
}
