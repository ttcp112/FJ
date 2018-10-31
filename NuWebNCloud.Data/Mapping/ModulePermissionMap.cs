using NuWebNCloud.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace NuWebNCloud.Data
{
    public class ModulePermissionMap : EntityTypeConfiguration<G_ModulePermission>
    {
        public ModulePermissionMap()
        {
            this.ToTable("G_ModulePermission");
            this.HasKey(x => x.Id);
            this.Property(x => x.Id).HasMaxLength(100);

            this.Property(x => x.RoleID).IsRequired().HasMaxLength(100);
            this.Property(x => x.ModuleID).IsRequired().HasMaxLength(100);
            this.Property(x => x.ModuleParentID).IsRequired().HasMaxLength(100);

            this.Property(x => x.IsView).IsOptional();
            this.Property(x => x.IsAction).IsOptional();
            this.Property(x => x.IsActive).IsOptional();

            this.Property(x => x.CreatedDate).IsRequired();
            this.Property(x => x.CreatedUser).IsRequired().HasMaxLength(255);
            this.Property(x => x.ModifiedDate).IsRequired();
            this.Property(x => x.ModifiedUser).IsRequired().HasMaxLength(255);
        }
    }
}
