using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class DailyTransactionsReportModels
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public DateTime Date { get; set; }
        public string DateDisplay { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? ClosedOn { get; set; }
        public string IngredientId { get; set; }
        public string IngredientCode { get; set; }
        public string IngredientName { get; set; }
        public string BaseUOMName { get; set; }
        public double OpenBal { get; set; }
        public double StockIn { get; set; }
        public double Received { get; set; }
        public double TransferIn { get; set; }
        public double CloseBal { get; set; }
        public double Sales { get; set; }
        public double Damage { get; set; }
        public double Wast { get; set; }
        public double Others { get; set; }
        public double Return { get; set; }
        public double TransferOut { get; set; }
        public bool IsExistAllocation { get; set; }
        public string BusinessId { get; set; }
        public double StockOut { get; set; }
        public double? AutoCloseBal { get; set; }
        public string TypeName { get; set; }
        public decimal UseForSelfMade { get; set; }
        public decimal ManualCloseBal { get; set; }
        public bool IsAutoCreated { get; set; }
        public double AdjustValue { get; set; }
    }
}
