using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Utilities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NuWebNCloud.Shared.Models.Settings
{
    public class TipServiceChargeModels
    {
        public string ID { get; set; }
        [_AttributeForLanguage("Please choose Store")]
        public string StoreID { get; set; }       
        public string StoreName { get; set; }
        public bool IsActive { get; set; }
        public bool IsApplyForEatIn { get; set; }
        public bool IsApplyForTakeAway { get; set; }
        public bool IsIncludedOnBill { get; set; }
        public bool IsAllowTip { get; set; }
        public bool IsCurrency { get; set; }

        [_AttributeForLanguage("The Value field is required.")]
        [_AttributeForLanguageRange(0, double.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public double Value { get; set; }
        //=====================
        public string CreatedUser { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public int Mode { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }

        public TipServiceChargeModels()
        {

        }
    }

    public class TipServiceChargeViewModels
    {
        public string StoreID { get; set; }
        public List<TipServiceChargeModels> ListItem { get; set; }
        public TipServiceChargeViewModels()
        {
            ListItem = new List<TipServiceChargeModels>();
        }
    }
}
