using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class ReceiptNoteForSeftMadeDetailMap : EntityTypeConfiguration<I_ReceiptNoteForSeftMadeDetail>
    {
        public ReceiptNoteForSeftMadeDetailMap()
        {
            this.ToTable("I_ReceiptNoteForSeftMadeDetail");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.ReceiptNoteId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ReceivingQty).IsRequired();
            this.Property(aa => aa.IsActived).IsRequired();
            this.Property(x => x.Status).IsOptional();
            this.Property(x => x.BaseReceivingQty).IsOptional();
            this.Property(aa => aa.IngredientId).IsOptional();
            this.Property(aa => aa.WOId).IsOptional().HasMaxLength(50);

        }
    }
}
