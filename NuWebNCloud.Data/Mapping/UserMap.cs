using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class UserMap : EntityTypeConfiguration<G_User>
    {
        public UserMap()
        {
            this.ToTable("G_User");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.Email).HasMaxLength(350).IsRequired();
            this.Property(aa => aa.Password).IsRequired();
            this.Property(aa => aa.Name).HasMaxLength(350).IsRequired();
            this.Property(aa => aa.IsActive).IsRequired();
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.CreatedUser).IsRequired().HasMaxLength(255);
            this.Property(aa => aa.LastDateModified).IsRequired();
            this.Property(aa => aa.LastUserModified).IsRequired().HasMaxLength(255);
        }
    }
}
