using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class IngredientTrackLogMap : EntityTypeConfiguration<I_IngredientTrackLog>
    {
        public IngredientTrackLogMap()
        {
            this.ToTable("I_IngredientTrackLog");
            this.HasKey(ss => ss.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.Code).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.CreatedDate).IsRequired();
        }
    }
}
