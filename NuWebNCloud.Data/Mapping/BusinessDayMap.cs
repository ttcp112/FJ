using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class BusinessDayMap : EntityTypeConfiguration<G_BusinessDay>
    {
        public BusinessDayMap()
        {
            this.ToTable("G_BusinessDay");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(ss => ss.StoreId).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.StartedOn).IsRequired();
            this.Property(ss => ss.ClosedOn).IsOptional();
            this.Property(ss => ss.CreatedDate).IsRequired();
            this.Property(ss => ss.CreatedUser).IsRequired().HasMaxLength(255);
            this.Property(ss => ss.LastDateModified).IsOptional();
            this.Property(ss => ss.LastUserModified).IsOptional().HasMaxLength(255);
            this.Property(ss => ss.Mode).IsRequired();
        }
    }
}
