using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class ReceiptNoteMap : EntityTypeConfiguration<I_ReceiptNote>
    {
        public ReceiptNoteMap()
        {
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.ToTable("I_ReceiptNote");
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.SupplierId).IsOptional().HasMaxLength(50);
            this.Property(aa => aa.ReceiptNo).IsOptional().HasMaxLength(50);
            this.Property(aa => aa.ReceiptBy).IsOptional().HasMaxLength(50);
            this.Property(aa => aa.ReceiptDate).IsOptional();

            this.Property(aa => aa.Status).IsRequired();
            this.Property(aa => aa.CreatedBy).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.UpdatedBy).IsOptional().HasMaxLength(50);
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.UpdatedBy).IsOptional();
            this.Property(aa => aa.BusinessId).IsOptional().HasMaxLength(50);
        }
    }
}
