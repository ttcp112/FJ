using NuWebNCloud.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace NuWebNCloud.Data
{
    public class SupplierMap : EntityTypeConfiguration<I_Supplier>
    {
        public SupplierMap()
        {
            this.ToTable("I_Supplier");
            this.HasKey(x => x.Id);
            this.Property(x => x.Id).HasMaxLength(50);

            this.Property(x => x.CompanyId).IsRequired().HasMaxLength(50);
            this.Property(x => x.Name).IsRequired().HasMaxLength(355);
            this.Property(x => x.Address).IsOptional().HasMaxLength(3000);
            this.Property(x => x.City).IsOptional().HasMaxLength(40);
            this.Property(x => x.Country).IsOptional().HasMaxLength(250);
            this.Property(x => x.ZipCode).IsOptional().HasMaxLength(50);
            this.Property(x => x.Phone1).IsOptional().HasMaxLength(50);
            this.Property(x => x.Phone2).IsOptional().HasMaxLength(50);
            this.Property(x => x.Fax).IsOptional().HasMaxLength(3000);
            this.Property(x => x.Email).IsOptional().HasMaxLength(255);
            this.Property(x => x.ContactInfo).IsOptional().HasMaxLength(3000);

            this.Property(x => x.CreatedBy).IsRequired().HasMaxLength(50);
            this.Property(x => x.CreatedDate).IsRequired();
            this.Property(x => x.ModifierBy).IsRequired().HasMaxLength(50);
            this.Property(x => x.ModifierDate).IsRequired();
            this.Property(x => x.IsActived).IsRequired();
            this.Property(aa => aa.Status).IsOptional();
            this.Property(aa => aa.XeroId).IsOptional().HasMaxLength(60);
        }
    }
}
