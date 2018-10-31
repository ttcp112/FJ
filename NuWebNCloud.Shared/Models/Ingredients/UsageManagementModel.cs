using NuWebNCloud.Shared.Models.Ingredients;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Ingredients
{

    #region Request
    public class UsageManagementRequest
    {
        public string StoreId { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DateFrom { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DateTo { get; set; }
        public int Mode { get; set; }
        public UsageManagementRequest()
        {
            Mode = (int)Commons.EStatus.Actived;
        }
    }
    #endregion

    public class UsageManagementModel
    {
        [Required(ErrorMessage = "Please choose Store")]
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public int Index { get; set; }
        public string Id { get; set; }
        public string ListUsageManagementDetailId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string UOMName { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N4}")]
        public double Usage { get; set; }
        public List<UsageManagementDetailModel> ListDetail { get; set; }
        public DateTime DataEntry { get; set; }
        public  UsageManagementModel()
        {
            
        ListDetail = new List<UsageManagementDetailModel>();
        }
    }

    
    public class UsageManagementDetailModel
    {
        public int Index { get; set; }
        public string BusinessDay { get; set; }
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public double Qty { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N4}")]
        public double Usage { get; set; }
        public DateTime ApplyDate { get; set; }
        public string BusinessId { get; set; }
        public bool StockAble { get; set; }
        public bool IsSeftMade { get; set; }
    }

    public class RecipeItemModel
    {
        public string ItemId { get; set; }
        public string IngredientId { get; set; }
        public string IngredientCode { get; set; }
        public string IngredientName { get; set; }
        public string UOMName { get; set; }
        public double Usage { get; set; }
        public double BaseUsage { get; set; }
    }
    public class ItemSaleForIngredientModel
    {
        public string ItemId { get; set; }
        public string IngredientId { get; set; }
        public string ItemName { get; set; }
        public double Qty { get; set; }
        public double BaseUsage { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
