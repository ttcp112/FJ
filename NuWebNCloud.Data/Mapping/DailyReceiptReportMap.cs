using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class DailyReceiptReportMap:EntityTypeConfiguration<R_DailyReceiptReport>
    {
        public DailyReceiptReportMap()
        {
            this.ToTable("R_DailyReceiptReport");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired();
            this.Property(aa => aa.BusinessDayId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.ReceiptId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.ReceiptNo).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.NoOfPerson).IsRequired();
            this.Property(aa => aa.ReceiptTotal).IsRequired();
            this.Property(aa => aa.Discount).IsRequired();
            this.Property(aa => aa.ServiceCharge).IsRequired();
            this.Property(aa => aa.GST).IsRequired();
            this.Property(aa => aa.Tips).IsRequired();
            this.Property(aa => aa.Rounding).IsRequired();
            this.Property(aa => aa.NetSales).IsRequired();
            this.Property(aa => aa.PromotionValue).IsRequired();
            this.Property(aa => aa.Mode).IsRequired();
            this.Property(aa => aa.CreditNoteNo).IsOptional().HasMaxLength(50);
        }
    }
}
