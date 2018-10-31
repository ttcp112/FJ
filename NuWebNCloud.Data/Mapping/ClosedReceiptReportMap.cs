using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class ClosedReceiptReportMap : EntityTypeConfiguration<R_ClosedReceiptReport> 
    {
        public ClosedReceiptReportMap()
        {
            this.ToTable("R_ClosedReceiptReport");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.CashierId).HasMaxLength(50).IsRequired();
            this.Property(aa => aa.CashierName).HasMaxLength(350).IsRequired();
            this.Property(aa => aa.ReceiptNo).HasMaxLength(100).IsRequired();
            this.Property(aa => aa.TableNo).HasMaxLength(100).IsRequired();
            this.Property(aa => aa.NoOfPersion).IsRequired();
            this.Property(aa => aa.Total).IsRequired();
            this.Property(aa => aa.Mode).IsRequired();
            this.Property(aa => aa.OrderId).IsOptional().HasMaxLength(50);
            this.Property(aa => aa.CreditNoteNo).IsOptional().HasMaxLength(50);
        }
    }
}
