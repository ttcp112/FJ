using NuWebNCloud.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace NuWebNCloud.Data
{
    public class RoleOrganizationMap : EntityTypeConfiguration<G_RoleOrganization>
    {
        public RoleOrganizationMap()
        {
            this.ToTable("G_RoleOrganization");
            this.HasKey(x => x.Id);

            this.Property(x => x.Id).HasMaxLength(100);
            this.Property(x => x.Name).IsRequired().HasMaxLength(255);
            this.Property(x => x.IsActive).IsRequired();
            this.Property(x => x.OrganizationId).IsRequired().HasMaxLength(50);

            this.Property(x => x.CreatedDate).IsRequired();
            this.Property(x => x.CreatedUser).IsRequired().HasMaxLength(255);
            this.Property(x => x.ModifiedDate).IsRequired();
            this.Property(x => x.ModifiedUser).IsRequired().HasMaxLength(255);
        }
    }
}
