using NuWebNCloud.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace NuWebNCloud.Data
{
    public class CountryMap : EntityTypeConfiguration<I_Country>
    {
        public CountryMap()
        {
            this.ToTable("I_Country");
            this.HasKey(x => x.Id);
            this.Property(x => x.Id).HasMaxLength(50);

            this.Property(x => x.ShortName).IsRequired().HasMaxLength(10);
            this.Property(x => x.FullName).IsRequired().HasMaxLength(255);
            this.Property(x => x.ZipCode).IsRequired().HasMaxLength(50);
            this.Property(x => x.IsActived).IsRequired();

        }
    }
}
