﻿using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class RecipeIngredientMap: EntityTypeConfiguration<I_Recipe_Ingredient>
    {
        public RecipeIngredientMap()
        {
            this.ToTable("I_Recipe_Ingredient");
            this.HasKey(ss => ss.Id);
            this.Property(aa => aa.Id).HasMaxLength(50);
            this.Property(ss => ss.IngredientId).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.MixtureIngredientId).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.UOMId).IsRequired().HasMaxLength(50);
            this.Property(ss => ss.Usage).IsRequired();
            this.Property(ss => ss.Status).IsRequired();
            this.Property(ss => ss.CreatedBy).IsRequired().HasMaxLength(250);
            this.Property(ss => ss.CreatedDate).IsRequired();
            this.Property(ss => ss.UpdatedBy).IsRequired().HasMaxLength(250);
            this.Property(ss => ss.UpdatedDate).IsRequired();
            this.Property(ss => ss.BaseUsage).IsOptional();
        }
    }
}
