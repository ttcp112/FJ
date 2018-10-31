using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class ItemizedSalesAnalysisReportModels
    {
        public string BusinessId { get; set; }
        public string StoreId { get; set; }
        public string ItemCode { get; set; }
        public string ItemId { get; set; }

        public int ItemTypeId { get; set; }

        public string ItemName { get; set; }

        public double Price { get; set; }

        public double ExtraPrice { get; set; }

        public double TotalPrice { get; set; }

        public double Quantity { get; set; }

        public double Discount { get; set; }

        public double Cost { get; set; }


        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string GLAccountCode { get; set; }

        public double ServiceCharge { get; set; }

        public double Tax { get; set; }

        public DateTime CreatedDate { get; set; }

        /*Properties View | Default = 0*/
        public double TotalCost { get; set; }
        public double CP { get; set; }
        public double Percent { get; set; }
        public double ItemTotal { get; set; }

        /*properties view | Default = ""*/
        public string StoreName { get; set; }
        public int Mode { get; set; }
        public double PromotionAmount { get; set; }
        public bool? IsIncludeSale { get; set; }
        public double? TotalDiscount { get; set; }
        public double? TotalAmount { get; set; }
        public double? ExtraAmount { get; set; }
        public string ReceiptId { get; set; }
        public string PoinsOrderId { get; set; }
        public string GiftCardId { get; set; }
        public int Hour { get; set; }
        public int? TaxType { get; set; }
        // Updated 05142018, for report new DB
        public bool IsDiscountTotal { get; set; }
        public string CreditNoteNo { get; set; }
        public string ParentId { get; set; }
        public string OrderDetailId { get; set; }
    }
}
