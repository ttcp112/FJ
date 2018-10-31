using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class ReceiptNoteDetailMap : EntityTypeConfiguration<I_ReceiptNoteDetail>
    {
        public ReceiptNoteDetailMap()
        {
            this.ToTable("I_ReceiptNoteDetail");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.PurchaseOrderDetailId).IsOptional().HasMaxLength(50);
            this.Property(aa => aa.ReceiptNoteId).IsRequired().HasMaxLength(50);
            //this.Property(aa => aa.IngredientCode).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.UOMId).IsOptional().HasMaxLength(50);

            this.Property(aa => aa.ReceivedQty).IsRequired();
            this.Property(aa => aa.ReceivingQty).IsRequired();
            this.Property(aa => aa.RemainingQty).IsRequired();
            this.Property(aa => aa.IsActived).IsRequired();
            this.Property(x => x.Status).IsOptional();
            this.Property(x => x.BaseReceivingQty).IsOptional();
            this.Property(aa => aa.IngredientId).IsOptional().HasMaxLength(50);
            //this.Property(aa => aa.Quantity).IsRequired();
            //this.Property(aa => aa.Price).IsRequired();
        }
    }
}
