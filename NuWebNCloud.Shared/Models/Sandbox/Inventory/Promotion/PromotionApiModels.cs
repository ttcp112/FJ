using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Sandbox.Inventory.Promotion
{
    public class PromotionApiModels
    {
        public string ID { get; set; }
        public string Name { get; set; }

        public string StoreID { get; set; }
        public string StoreName { get; set; }

        public string Description { get; set; }
        public string ImageURL { get; set; }
        public string ShortName { get; set; }
        public string PromoteCode { get; set; }
        public byte PromotionType { get; set; }
        public int Priority { get; set; }
        public bool isActive { get; set; }
        public bool IsAllowedCombined { get; set; }
        public int MaximumUsedQty { get; set; }
        public double? MaximumEarnAmount { get; set; }
        public int MaximumQtyPerUser { get; set; }
        public bool IsRepeated { get; set; }
        public bool IsLimited { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public string DateOfWeek { get; set; }
        public string DateOfMonth { get; set; }
        public bool isSpendOperatorAnd { get; set; }
        public bool isEarnOperatorAnd { get; set; }

        public List<SpendingRuleDTO> ListSpendingRule { get; set; }
        public List<EarningRuleDTO> ListEarningRule { get; set; }

        public List<string> ListStoreID { get; set; }
        public List<PromotionModels> ListPromotion { get; set; }
        //===========
        public string CreatedUser { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        public byte Mode { get; set; }

        public List<string> ListOrgID { get; set; }
    }
}
