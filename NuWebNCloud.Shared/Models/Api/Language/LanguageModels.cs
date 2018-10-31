using System;
using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Api.Language
{
    public class LanguageModels
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public int Status { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime LastModified { get; set; }
        public List<LanguageDetailDTO> ListText { get; set; }
        public LanguageModels()
        {
            ListText = new List<LanguageDetailDTO>();
        }
    }
    public class RequestGetLanguage
    {
        public string LanguageID { get; set; }
        public int Type { get; set; }
        public List<string> ListKey { get; set; }
    }
    public class ResponseGetLanguage
    {
        public bool Success { get; set; }
        public List<LanguageModels> ListData { get; set; }

        public ResponseGetLanguage()
        {
            Success = false;
            ListData = new List<LanguageModels>();
        }
    }
}
