using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class AuditTrailReportMap: EntityTypeConfiguration<R_AuditTrailReport>
    {
        public AuditTrailReportMap()
        {
            this.ToTable("R_AuditTrailReport");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired();
            this.Property(aa => aa.ReceiptDate).IsRequired();
            this.Property(aa => aa.ReceiptID).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ReceiptNo).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ReceiptStatus).IsRequired();
            this.Property(aa => aa.OrderNo).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CashierId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CashierName).IsRequired().HasMaxLength(350);
            this.Property(aa => aa.Discount).IsRequired();
            this.Property(aa => aa.ReceiptTotal).IsRequired();
            this.Property(aa => aa.CancelAmount).IsRequired();
            this.Property(aa => aa.BusinessDayId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.Mode).IsRequired();
            this.Property(aa => aa.PromotionAmount).IsOptional();
            this.Property(aa => aa.CreditNoteNo).IsOptional().HasMaxLength(50);
        }
    }
}
