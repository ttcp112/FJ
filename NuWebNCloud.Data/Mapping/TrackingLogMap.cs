using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuWebNCloud.Data.Entities;

namespace NuWebNCloud.Data
{
    public class TrackingLogMap:EntityTypeConfiguration<G_TrackingLog>
    {
        public TrackingLogMap()
        {
            this.ToTable("G_TrackingLog");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.TableName).HasMaxLength(200).IsRequired();
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.JsonContent).IsRequired();
            this.Property(aa => aa.IsDone).IsRequired();
        }
    }
}
