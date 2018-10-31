using NuWebNCloud.Shared.Models.Validation;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace NuWebNCloud.Shared.Models.Sandbox.Import
{
    public class ImportNoImageModel
    {
        public bool ImportStatus { get; set; }

        [Required]
        [DataType(DataType.Upload)]
        [FileSize(3072000)]
        [FileTypes("xlsx")]
        public HttpPostedFileBase ExcelUpload { get; set; }

        public ImportNoImageModel()
        {
        }
    }
}
