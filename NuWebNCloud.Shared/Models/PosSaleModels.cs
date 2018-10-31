using NuWebNCloud.Shared.Models.Xero.GenerateInvoice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class PosSaleModels
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public string BusinessId { get; set; }
        public string OrderId { get; set; }
        public string OrderNo { get; set; }
        public string ReceiptNo { get; set; }
        public DateTime? ReceiptCreatedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrderStatus { get; set; }
        public string TableNo { get; set; }
        public int NoOfPerson { get; set; }
        public double CancelAmount { get; set; }
        public double ReceiptTotal { get; set; }
        public double Discount { get; set; }
        public double Tip { get; set; }
        public double PromotionValue { get; set; }
        public double ServiceCharge { get; set; }
        public double GST { get; set; }
        public double Rounding { get; set; }
        public double Refund { get; set; }
        public double NetSales { get; set; }
        public string CashierId { get; set; }
        public string CashierName { get; set; }
        public int Mode { get; set; }
        public string CreditNoteNo { get; set; }
    }

    public class PosSaleReportReturnModels
    {
        public List<PosSaleModels> PosSaleReportDTOs { get; set; }
        public List<PosSaleDetailModels> PosSaleDetailReportDTOs { get; set; }
        public List<PaymentReportXeroDTO> PaymentReportXeroDTOs { get; set; }
        public BusinessInfoXeroDTO BusinessInfo { get; set; }
        public PosSaleReportReturnModels()
        {
            PosSaleReportDTOs = new List<PosSaleModels>();
            PosSaleDetailReportDTOs = new List<PosSaleDetailModels>();
            PaymentReportXeroDTOs = new List<PaymentReportXeroDTO>();
            BusinessInfo = new BusinessInfoXeroDTO();
        }
    }
  
}
