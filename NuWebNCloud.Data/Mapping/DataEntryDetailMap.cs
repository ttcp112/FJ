using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class DataEntryDetailMap : EntityTypeConfiguration<I_DataEntryDetail>
    {
        public DataEntryDetailMap()
        {
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.ToTable("I_DataEntryDetail");
            this.Property(aa => aa.DataEntryId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.IngredientId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.Damage).IsOptional();
            this.Property(aa => aa.Wastage).IsOptional();
            this.Property(aa => aa.OrderQty).IsOptional();
            this.Property(aa => aa.Reasons).IsOptional();
        }
    }
}
