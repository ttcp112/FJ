using NuWebNCloud.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace NuWebNCloud.Data
{
    public class ReturnNoteMap : EntityTypeConfiguration<I_Return_Note>
    {
        public ReturnNoteMap()
        {
            this.ToTable("I_Return_Note");
            this.HasKey(x => x.Id);
            this.Property(x => x.Id).HasMaxLength(50);

            this.Property(x => x.ReturnNoteNo).IsRequired().HasMaxLength(20);
            this.Property(x => x.ReceiptNoteId).IsRequired().HasMaxLength(50);

            this.Property(x => x.CreatedBy).IsRequired().HasMaxLength(50);
            this.Property(x => x.CreatedDate).IsRequired();
            this.Property(x => x.ModifierBy).IsRequired().HasMaxLength(50);
            this.Property(x => x.ModifierDate).IsRequired();
            this.Property(x => x.IsActived).IsRequired();
            this.Property(x => x.BusinessId).IsOptional().HasMaxLength(50);

        }
    }
}
