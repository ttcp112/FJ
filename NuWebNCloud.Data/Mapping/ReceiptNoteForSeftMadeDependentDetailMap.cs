using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class ReceiptNoteForSeftMadeDependentDetailMap : EntityTypeConfiguration<I_ReceiptNoteForSeftMadeDependentDetail>
    {
        public ReceiptNoteForSeftMadeDependentDetailMap()
        {
            this.ToTable("I_ReceiptNoteForSeftMadeDependentDetail");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.RNSelfMadeDetailId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.IngredientId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.StockOutQty).IsRequired();
            this.Property(aa => aa.IsActived).IsRequired();
          

        }
    }
}
