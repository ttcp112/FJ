using NuWebNCloud.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace NuWebNCloud.Data
{
    public class RoleOnStoreMap : EntityTypeConfiguration<G_RoleOnStore>
    {
        public RoleOnStoreMap()
        {
            this.ToTable("G_RoleOnStore");
            this.HasKey(x => x.Id);

            this.Property(x => x.Id).HasMaxLength(100);
            this.Property(x => x.RoleId).IsRequired().HasMaxLength(100);
            this.Property(x => x.StoreId).IsRequired().HasMaxLength(100);
            this.Property(x => x.IsActive).IsRequired();
           
            this.Property(x => x.CreatedDate).IsRequired();
            this.Property(x => x.CreatedUser).IsRequired().HasMaxLength(255);
            this.Property(x => x.ModifiedDate).IsRequired();
            this.Property(x => x.ModifiedUser).IsRequired().HasMaxLength(255);
        }
    }
}
