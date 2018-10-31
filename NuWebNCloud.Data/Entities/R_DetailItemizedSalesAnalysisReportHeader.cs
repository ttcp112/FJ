using NuWebNCloud.Data.Entities;

namespace NuWebNCloud.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class R_DetailItemizedSalesAnalysisReportHeader : BaseEntity
    {
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public int ItemTypeId { get; set; }
        public double Qty { get; set; }
        public double Price { get; set; }
        public DateTime CreatedDate { get; set; }

        public int Mode { get; set; }
    }
}
