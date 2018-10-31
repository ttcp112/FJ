using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Xero.GenerateInvoice
{
    /// <summary>
    /// GENERATE INVOICE LINEITEM
    /// </summary>
    public class GILineItemModels
    {
        public double UnitAmount { get; set; }
        public double LineAmount { get; set; }
        public List<string> Tracking { get; set; }
        public GILineItemModels()
        {
            Tracking = new List<string>();
        }
    }
}
