using NuWebNCloud.Shared.Models.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace NuWebNCloud.Shared.Models.Sandbox.Import
{
    public class SandboxImportModel
    {
        [DataType(DataType.Upload)]
        [FileSize(3072000)]
        [FileTypes("xls,xlsx")]
        public HttpPostedFileBase ExcelUpload { get; set; }

        [DataType(DataType.Upload)]
        [FileSize(10240000)]
        [FileTypes("zip")]
        public HttpPostedFileBase ImageZipUpload { get; set; }

        public List<string> ListStores { get; set; }

        public SandboxImportModel()
        {
        }
    }
}
