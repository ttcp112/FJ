using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class RefundDetailMap: EntityTypeConfiguration<R_RefundDetail>
    {
        public RefundDetailMap()
        {
            this.ToTable("R_RefundDetail");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.RefundId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ItemId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ItemName).IsRequired().HasMaxLength(250);
            this.Property(aa => aa.ItemType).IsRequired();
            this.Property(aa => aa.Qty).IsRequired();
            this.Property(aa => aa.ServiceCharged).IsRequired();
            this.Property(aa => aa.Tax).IsRequired();
            this.Property(aa => aa.PriceValue).IsRequired();
            this.Property(aa => aa.PromotionAmount).IsRequired();
            this.Property(aa => aa.DiscountAmount).IsRequired();
        }
    }
}
