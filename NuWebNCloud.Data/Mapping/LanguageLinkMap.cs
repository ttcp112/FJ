using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class LanguageLinkMap : EntityTypeConfiguration<G_LanguageLink>
    {
        public LanguageLinkMap()
        {
            this.ToTable("G_LanguageLink");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.Name).IsRequired();
            this.Property(aa => aa.Status).IsRequired();
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.CreatedUser).IsRequired().HasMaxLength(250);
            this.Property(aa => aa.LastModified).IsRequired();
            this.Property(aa => aa.ModifiedUser).IsRequired().HasMaxLength(250);
        }
    }
}
