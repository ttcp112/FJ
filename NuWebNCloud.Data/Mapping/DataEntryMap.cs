using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class DataEntryMap : EntityTypeConfiguration<I_DataEntry>
    {
        public DataEntryMap()
        {
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.ToTable("I_DataEntry");
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.EntryCode).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.EntryDate).IsRequired();
            this.Property(aa => aa.BusinessId).IsOptional().HasMaxLength(50);
            this.Property(aa => aa.CreatedBy).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ModifierBy).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.ModifierDate).IsRequired();
            this.Property(aa => aa.IsActived).IsRequired();
            this.Property(aa => aa.StartedOn).IsOptional();
            this.Property(aa => aa.ClosedOn).IsOptional();
        }
    }
}
