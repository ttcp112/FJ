using System.ComponentModel.DataAnnotations;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class DataEntryDetailModels
    {
        public string Id { get; set; }
        public string DataEntryId { get; set; }

        public string IngredientId { get; set; }
        public string IngredientName { get; set; }
        public string IngredientCode { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public double Damage { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public double Wast { get; set; }
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public double OrderQty { get; set; }
        public string Reasons { get; set; }
    }
}
