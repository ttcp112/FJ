using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class POSAPIMerchantConfigMap : EntityTypeConfiguration<G_POSAPIMerchantConfig>
    {
        public POSAPIMerchantConfigMap()
        {
            this.ToTable("G_POSAPIMerchantConfig");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(ss => ss.NuPOSInstance).IsOptional().HasMaxLength(250);
            this.Property(ss => ss.POSAPIUrl).IsRequired().HasMaxLength(350);
            this.Property(ss => ss.FTPHost).IsRequired().HasMaxLength(350);
            this.Property(ss => ss.FTPUser).IsRequired().HasMaxLength(350);
            this.Property(ss => ss.FTPPassword).IsRequired().HasMaxLength(350);
            this.Property(ss => ss.ImageBaseUrl).IsRequired().HasMaxLength(350);

            this.Property(ss => ss.BreakfastStart).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.BreakfastEnd).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.LunchStart).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.LunchEnd).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.DinnerStart).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.DinnerEnd).IsRequired().HasMaxLength(50);

            this.Property(ss => ss.CreatedDate).IsRequired();
            this.Property(ss => ss.IsActived).IsRequired();
            this.Property(ss => ss.POSInstanceVersion).IsOptional();

            this.Property(ss => ss.MorningStart).IsOptional();
            this.Property(ss => ss.MorningEnd).IsOptional();
            this.Property(ss => ss.MidDayStart).IsOptional();
            this.Property(ss => ss.MidDayEnd).IsOptional();
            this.Property(ss => ss.WebHostUrl).IsOptional();
        }
    }
}
