﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Models
{
    public class DailyReceiptReportDataModels
    {
        public string StoreId { get; set; }
        public string BusinessDayId { get; set; }
        public DateTime CreatedDate { get; set; }

        public string ReceiptId { get; set; }

        public string ReceiptNo { get; set; }

        public int NoOfReceipt { get; set; }

        public int NoOfPerson { get; set; }     //PAX

        public double ReceiptTotal { get; set; }

        public double Discount { get; set; }

        public double ServiceCharge { get; set; }

        public double GST { get; set; }         //Tax

        public double Tips { get; set; }

        public double Rounding { get; set; }

        public double NetSales { get; set; }
        public double PromotionValue { get; set; }
        public int Mode { get; set; }
        public string CreditNoteNo { get; set; }
    }
}
