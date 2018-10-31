using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Models
{
    public class CategoryTmpChartResponseDataModels
    {
        public string GLAccountCode { get; set; }
        public string CategoryName { get; set; }
        public double Amount { get; set; }
        public DateTime CreateDate { get; set; }
        public string CategoryId { get; set; }
        public string ReceiptId { get; set; }
    }
}
