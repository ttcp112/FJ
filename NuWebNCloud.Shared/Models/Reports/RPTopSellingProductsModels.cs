using NuWebNCloud.Shared.Utilities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NuWebNCloud.Shared.Models.Reports
{
    public class RPTopSellingProductsModels : BaseReportModel
    {
        [_AttributeForLanguage("The Top Sell field is required.")]
        public int TopSell { get; set; }
        public RPTopSellingProductsModels()
        {
            TopSell = 10;
        }
    }
}
