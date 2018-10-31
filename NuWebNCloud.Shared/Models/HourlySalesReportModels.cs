using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class HourlySalesReportModels
    {
        public string StoreId { get; set; }

        public int Time { get; set; }
        public DateTime Date { get; set; }

        public double ReceiptTotal { get; set; }    //SUM(RC.Total)

        public int NoOfReceipt { get; set; }  //COUNT(RC.ID)

        public int NoOfPerson { get; set; }         //SUM(ISNULL(T.NoOfPerson, 0))

        public double PerNoOfReceipt { get; set; }  //SUM(RC.Total) / (CASE COUNT(RC.ID) WHEN 0 THEN 1 ELSE COUNT(RC.ID) END)

        public double PerNoOfPerson { get; set; }   //CASE SUM(ISNULL(T.NoOfPerson, 0)) WHEN 0 THEN 0 ELSE (SUM(RC.Total) / SUM(ISNULL(T.NoOfPerson, 0)))

        public double NetSales { get; set; }
        public double RoundingValue { get; set; }

        public string ReceiptId { get; set; }

        public DateTime CreatedDate { get; set; }
        public int Mode { get; set; }
        public string CreditNoteNo { get; set; }
        //for credit note
        public string BusinessDayId { get; set; }
        public string ReceiptNo { get; set; }
        public double Discount { get; set; }
        public double ServiceCharge { get; set; }
        public double GST { get; set; }
        public double Rounding { get; set; }
        public double PromotionValue { get; set; }
        //for calculate
        public double CreditNoteTotal { get; set; }
    }
}
