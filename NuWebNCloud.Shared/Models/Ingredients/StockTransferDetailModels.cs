using NuWebNCloud.Shared.Factory.Ingredients;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class StockTransferDetailModels
    {
        public string Id { get; set; }
        public string StockTransferId { get; set; }

        public string IngredientId { get; set; }
        public string IngredientName { get; set; }
        public string IngredientCode { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Please enter a value equal or larger than 0")]
        public double RequestQty { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Please enter a value equal or larger than 0")]
        public double IssueQty { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Please enter a value equal or larger than 0")]
        public double ReceiveQty { get; set; }

        public List<SelectListItem> ListUOM { get; set; }
        public string UOMId { get; set; }
        public string BaseUOM { get; set; }

        public int OffSet { get; set; }
        public int Delete { get; set; }
        public bool IsSelect { get; set; }
        public double Rate { get; set; }
        public StockTransferDetailModels()
        {
            ListUOM = new List<SelectListItem>();
        }
    }
}
