using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class InventoryManagementMap : EntityTypeConfiguration<I_InventoryManagement> 
    {
        public InventoryManagementMap()
        {
            this.ToTable("I_InventoryManagement");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(aa => aa.StoreId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.IngredientId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.Quantity).IsRequired();
            this.Property(aa => aa.Price).IsRequired();
            this.Property(aa => aa.POQty).IsOptional();
        }
    }
}
