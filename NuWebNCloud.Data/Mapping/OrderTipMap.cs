using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class OrderTipMap: EntityTypeConfiguration<G_OrderTip>
    {
        public OrderTipMap()
        {
            this.ToTable("G_OrderTip");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(ss => ss.StoreId).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.OrderId).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.PaymentId).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.PaymentName).IsRequired().HasMaxLength(150);
            this.Property(ss => ss.Amount).IsRequired();
            this.Property(ss => ss.CreatedDate).IsRequired();
            this.Property(ss => ss.Mode).IsRequired();
        }
    }
}
