using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class TaxMap : EntityTypeConfiguration<G_Tax>
    {
        public TaxMap()
        {
            this.ToTable("G_Tax");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired();
            this.Property(aa => aa.Name).HasMaxLength(350).IsRequired();
            this.Property(aa => aa.IsActive).IsRequired();
            this.Property(aa => aa.Percent).IsRequired();
            this.Property(aa => aa.TaxType).IsRequired();
            this.Property(aa => aa.DateCreated).IsRequired();
            this.Property(aa => aa.UserCreated).HasMaxLength(350).IsRequired();
        }
    }
}
