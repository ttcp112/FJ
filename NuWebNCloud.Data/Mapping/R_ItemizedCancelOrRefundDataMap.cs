using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class R_ItemizedCancelOrRefundDataMap : EntityTypeConfiguration<R_ItemizedCancelOrRefundData>
    {
        public R_ItemizedCancelOrRefundDataMap()
        {
            this.ToTable("R_ItemizedCancelOrRefundData");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.BusinessId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.OrderId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ItemId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ItemCode).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ItemTypeId).IsRequired();
            this.Property(aa => aa.ItemName).IsRequired().HasMaxLength(450);
            this.Property(aa => aa.Price).IsRequired();
            this.Property(aa => aa.Quantity).IsRequired();
            this.Property(aa => aa.Amount).IsRequired();
            this.Property(aa => aa.CategoryId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CategoryName).IsRequired().HasMaxLength(350);
            this.Property(aa => aa.CancelUser).IsRequired().HasMaxLength(350);
            this.Property(aa => aa.RefundUser).IsRequired().HasMaxLength(350);

            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.Mode).IsRequired();
            this.Property(aa => aa.IsRefund).IsOptional();
        }
    }
}
