using NuWebNCloud.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace NuWebNCloud.Data
{
    public class ReturnNoteDetailMap : EntityTypeConfiguration<I_Return_Note_Detail>
    {
        public ReturnNoteDetailMap()
        {
            this.ToTable("I_Return_Note_Detail");
            this.HasKey(x => x.Id);
            this.Property(x => x.Id).HasMaxLength(50);
            this.Property(x => x.ReturnNoteId).IsRequired().HasMaxLength(50);
            this.Property(x => x.ReceiptNoteDetailId).IsRequired().HasMaxLength(50);
            this.Property(x => x.ReceivedQty).IsRequired();
            this.Property(x => x.ReturnQty).IsRequired();
            this.Property(x => x.IsActived).IsRequired();
            this.Property(ss => ss.ReturnBaseQty).IsOptional();
        }
    }
}
