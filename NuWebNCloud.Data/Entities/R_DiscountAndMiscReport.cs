using NuWebNCloud.Data.Entities;

namespace NuWebNCloud.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class R_DiscountAndMiscReport : BaseEntity
    {
        public double MiscValue { get; set; }
        public double DiscountValue { get; set; }

        public DateTime CreatedDate { get; set; }

        public int Mode { get; set; }
    }
}
