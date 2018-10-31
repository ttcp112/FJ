using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Api.Language
{
    public class LanguageLinkDetailModels
    {
        public string Id { get; set; }

        public string LanguageId { get; set; }
        public string LanguageName { get; set; }

        public string LanguageLinkId { get; set; }
        public string LanguageLinkName { get; set; }

        public string Text { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime LastModified { get; set; }
    }
    public class LanguageDetailDTO
    {
        public string KeyID { get; set; }
        public string KeyName { get; set; }
        public string Text { get; set; }
    }
}
