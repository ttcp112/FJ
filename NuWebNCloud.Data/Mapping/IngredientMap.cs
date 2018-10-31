using NuWebNCloud.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace NuWebNCloud.Data
{
    public class IngredientMap: EntityTypeConfiguration<I_Ingredient>
    {
        public IngredientMap()
        {
            this.ToTable("I_Ingredient");
            this.HasKey(ss => ss.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);

            this.Property(aa => aa.CompanyId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.Code).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.Name).IsRequired().HasMaxLength(255);
            this.Property(aa => aa.Description).IsOptional().HasMaxLength(3000);

            this.Property(aa => aa.BaseUOMName).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.BaseUOMId).IsRequired().HasMaxLength(50);

            this.Property(aa => aa.IsActive).IsRequired();
            this.Property(aa => aa.IsPurchase).IsRequired();
            this.Property(aa => aa.IsCheckStock).IsRequired();
            this.Property(aa => aa.IsSelfMode).IsRequired();

            this.Property(aa => aa.ReceivingUOMId).IsRequired().HasMaxLength(50);
            this.Property(aa => aa.ReceivingQty).IsRequired();
            this.Property(aa => aa.QtyTolerance).IsRequired();


            this.Property(aa => aa.Status).IsOptional();
            this.Property(aa => aa.PurchasePrice).IsRequired();
            this.Property(aa => aa.SalePrice).IsRequired();

            this.Property(aa => aa.CreatedBy).IsRequired().HasMaxLength(250);
            this.Property(aa => aa.CreatedDate).IsRequired();
            this.Property(aa => aa.UpdatedBy).IsOptional().HasMaxLength(250);
            this.Property(aa => aa.UpdatedDate).IsOptional();

            this.Property(aa => aa.XeroId).IsOptional().HasMaxLength(100);
            this.Property(aa => aa.ReOrderQty).IsOptional();
            this.Property(aa => aa.MinAlertQty).IsOptional();
            this.Property(aa => aa.StockAble).IsOptional();
        }
    }
}
