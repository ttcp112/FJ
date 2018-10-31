using NuWebNCloud.Shared.Factory.Ingredients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class RecipeModels
    {

    }

    public class RecipeProductModels
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public string IngredientId { get; set; }
        public string ItemId { get; set; }
        public string ItemCode { get; set; }
        public int Ingredient { get; set; }
        public string ItemName { get; set; }
        public int ItemType { get; set; }
        public string UOMId { get; set; }
        public double Usage { get; set; }
        public byte Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public double BaseUsage { get; set; }
    }

    public class RecipeProductViewModels
    {
        public int Type { get; set; }
        public string StoreID { get; set; }
        public List<RecipeProductModels> ListItem { get; set; }
        public RecipeProductViewModels()
        {
            ListItem = new List<RecipeProductModels>();
        }
    }
    /**/
    public class ProductIngredient
    {
        public bool IsSelect { get; set; }
        public string IngredientId { get; set; }
        public string IngredientName { get; set; }
        public string BaseUOM { get; set; }
        public double Usage { get; set; }
        public double BaseUsage { get; set; }
        public string BaseUOMId { get; set; }
        public List<SelectListItem> ListUOM { get; set; }

        public ProductIngredient()
        {
            ListUOM = new List<SelectListItem>();
        }

        public void GetListUOMForIng()
        {
        }
    }

    public class RecipeProductIngredientViewModels
    {
        public int Type { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string StoreId { get; set; }
        public List<ProductIngredient> ListItem { get; set; }

        public RecipeProductIngredientViewModels()
        {
            ListItem = new List<ProductIngredient>();
        }
    }

    /*Ingredient*/
    public class RecipeIngredientModels
    {
        public string Id { get; set; }
        public string IngredientId { get; set; }
        public string IngredientCode { get; set; }
        public string IngredientName { get; set; }
        public string MixtureIngredientId { get; set; }
        public string UOMId { get; set; }
        public double Usage { get; set; }
        public int Ingredient { get; set; }
        public byte Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public double BaseUsage { get; set; }
    }

    public class RecipeIngredientViewModels
    {
        public List<RecipeIngredientModels> ListItem { get; set; }
        public RecipeIngredientViewModels()
        {
            ListItem = new List<RecipeIngredientModels>();
        }
    }

    public class RecipeIngredientIngredientViewModels
    {
        public string Id { get; set; }
        public List<ProductIngredient> ListItem { get; set; }
        public RecipeIngredientIngredientViewModels()
        {
            ListItem = new List<ProductIngredient>();
        }
    }

    //For recipe - inventory
    public class RecipeIngredientUsageModels
    {
        public string Id { get; set; }
        public string MixtureIngredientId { get; set; }
        public double BaseUsage { get; set; }
        public double TotalUsage { get; set; }
        public List<RecipeIngredientUsageModels> ListChilds { get; set; }
        public RecipeIngredientUsageModels() {
            ListChilds = new List<RecipeIngredientUsageModels>();
        }
    }

}
