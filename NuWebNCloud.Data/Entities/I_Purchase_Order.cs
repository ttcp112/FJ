using NuWebNCloud.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data
{
    public class I_Purchase_Order : BaseEntity
    {
        public string Code { get; set; }
        public string SupplierId { get; set; }
        public DateTime PODate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int TaxType { get; set; }
        public double TaxPercen { get; set; }
        public double SubTotal { get; set; }
        public double TaxAmount { get; set; }
        public double Additional { get; set; }
        public string AdditionalReason { get; set; }
        public double Total { get; set; }
        public int Status { get; set; }
        public string Note { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifierBy { get; set; }
        public DateTime ModifierDate { get; set; }
        public bool IsActived { get; set; }
    }
}
