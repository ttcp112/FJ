using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Mapping
{
    public class RefundMap : EntityTypeConfiguration<R_Refund>
    {
        public RefundMap()
        {
            this.ToTable("R_Refund");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.BusinessDayId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.OrderId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ReceiptDate).IsRequired();
            this.Property(aa => aa.TotalRefund).IsRequired();
            this.Property(aa => aa.ServiceCharged).IsRequired();
            this.Property(aa => aa.Tax).IsRequired();
            this.Property(aa => aa.Discount).IsRequired();
            this.Property(aa => aa.Promotion).IsRequired();
            this.Property(aa => aa.CreatedUser).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.Description).IsOptional().HasMaxLength(4000);
            this.Property(aa => aa.IsGiftCard).IsOptional();
        }
    }
}
