using System;

namespace NuWebNCloud.Data.Entities
{
    public partial class G_Tax : BaseEntity
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public double Percent { get; set; }
        public int TaxType { get; set; }
        public DateTime DateCreated { get; set; }
        public string UserCreated { get; set; }
    }
}
