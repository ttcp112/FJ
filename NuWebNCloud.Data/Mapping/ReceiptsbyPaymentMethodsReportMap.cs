using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class ReceiptsbyPaymentMethodsReportMap:EntityTypeConfiguration<R_ReceiptsbyPaymentMethodsReport>
    {
        public ReceiptsbyPaymentMethodsReportMap()
        {
            this.ToTable("R_ReceiptsbyPaymentMethodsReport");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(ss => ss.StoreId).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.PaymentId).HasMaxLength(50).IsRequired();
            this.Property(ss => ss.PaymentName).HasMaxLength(350).IsRequired();
            this.Property(ss => ss.CreatedDate).IsRequired();
            this.Property(ss => ss.ReceiptNo).HasMaxLength(50).IsRequired();
            this.Property(ss => ss.ReceiptRefund).IsRequired();
            this.Property(ss => ss.ReceiptTotal).IsRequired();
            this.Property(ss => ss.Total).IsRequired();
            this.Property(ss => ss.Mode).IsRequired();

        }
    }
}
