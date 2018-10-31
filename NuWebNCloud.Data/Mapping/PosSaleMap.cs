using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class PosSaleMap : EntityTypeConfiguration<R_PosSale>
    {
        public PosSaleMap()
        {
            this.ToTable("R_PosSale");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(60);
            this.Property(aa => aa.BusinessId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.OrderId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.OrderNo).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ReceiptNo).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.ReceiptCreatedDate).IsOptional();
            this.Property(aa => aa.OrderStatus).IsRequired();
            this.Property(aa => aa.TableNo).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.NoOfPerson).IsRequired();
            this.Property(aa => aa.CancelAmount).IsRequired();
            this.Property(aa => aa.ReceiptTotal).IsRequired();
            this.Property(aa => aa.Discount).IsRequired();
            this.Property(aa => aa.Tip).IsRequired();
            this.Property(aa => aa.PromotionValue).IsRequired();
            this.Property(aa => aa.ServiceCharge).IsRequired();
            this.Property(aa => aa.GST).IsRequired();
            this.Property(aa => aa.Rounding).IsRequired();
            this.Property(aa => aa.Refund).IsRequired();
            //this.Property(aa => aa.NetSales).IsRequired();
            this.Property(aa => aa.CashierId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CashierName).IsRequired().HasMaxLength(350);
           
            this.Property(aa => aa.Mode).IsRequired();
            this.Property(aa => aa.CreditNoteNo).IsOptional().HasMaxLength(50);
        }
    }
}
