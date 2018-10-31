using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class StoreSettingModels
    {
        public string Id { get; set; }

        public string IngredientId { get; set; }
        public string IngredientCode { get; set; }
        public string IngredientName { get; set; }
        public string ReceivingUOM { get; set; }
        public string CompanyId { get; set; }
        public string StoreId { get; set; }
        public string StoreName { get; set; }

        [Required(ErrorMessage = "Reordering Quantity is required")]
        public double ReorderingQty { get; set; }

        [Required(ErrorMessage = "Min Alert is required")]
        public double MinAlert { get; set; }
        //for set up
        public List<string> ListStore { get; set; }
        public StoreSettingModels()
        {
            ListStore = new List<string>();
        }
    }

    public class StoreSettingViewModels
    {
        public List<StoreSettingModels> ListItem { get; set; }
        public List<string> ListStore { get; set; }
        public StoreSettingViewModels()
        {
            ListItem = new List<StoreSettingModels>();
            ListStore = new List<string>();
        }
    }
}
