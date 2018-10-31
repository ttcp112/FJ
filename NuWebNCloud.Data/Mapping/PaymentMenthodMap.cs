using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class PaymentMenthodMap : EntityTypeConfiguration<G_PaymentMenthod>
    {
        public PaymentMenthodMap()
        {
            this.ToTable("G_PaymentMenthod");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.OrderId).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.PaymentId).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.PaymentCode).IsOptional();
            this.Property(aa => aa.PaymentName).HasMaxLength(250).IsRequired();
            this.Property(aa => aa.Amount).IsRequired();
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.CreatedUser).IsRequired().HasMaxLength(255);
            this.Property(aa => aa.LastDateModified).IsRequired();
            this.Property(aa => aa.LastUserModified).IsRequired().HasMaxLength(255);
            this.Property(ss => ss.Mode).IsRequired();
            this.Property(ss => ss.IsInclude).IsOptional();
        }
    }
}
