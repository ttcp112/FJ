using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Data.Mapping
{
    public partial class NewsteadEntityTypeConfiguration<T> : EntityTypeConfiguration<T> where T : class
    {
        public NewsteadEntityTypeConfiguration() { }
    }
}
