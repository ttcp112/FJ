using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Models
{
    public class DailySalesReportInsertDataModels
    {
        public string StoreId { get; set; }
        public string OrderId { get; set; }

        public DateTime CreatedDate { get; set; }

        public int NoOfPerson { get; set; }

        public double ReceiptTotal { get; set; }

        public double Discount { get; set; }

        public double ServiceCharge { get; set; }

        public double GST { get; set; }

        public double Rounding { get; set; }

        public double Refund { get; set; }

        public double NetSales { get; set; }
        public double Tip { get; set; }
        public double PromotionValue { get; set; }

        public int Mode { get; set; }
        public string BusinessId { get; set; }
        public string CreditNoteNo { get; set; }
    }
}
