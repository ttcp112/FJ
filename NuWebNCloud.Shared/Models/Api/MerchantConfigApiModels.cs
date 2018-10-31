using System;
using System.Collections.Generic;

namespace NuWebNCloud.Shared.Models.Api
{
    public class MerchantConfigApiModels
    {
        public string Id { get; set; }
        public string NuPOSInstance { get; set; }
        public string POSAPIUrl { get; set; }
        public string FTPHost { get; set; }
        public string FTPUser { get; set; }
        public string FTPPassword { get; set; }
        public string ImageBaseUrl { get; set; }

        public string BreakfastStart { get; set; }
        public string BreakfastEnd { get; set; }

        public string LunchStart { get; set; }
        public string LunchEnd { get; set; }

        public string DinnerStart { get; set; }
        public string DinnerEnd { get; set; }

        public DateTime CreatedDate { get; set; }
        public bool IsActived { get; set; }
        //08/08/2017
        public int? POSInstanceVersion { get; set; }
        public TimeSpan MorningStart { get; set; }
        public TimeSpan MorningEnd { get; set; }
        public TimeSpan MidDayStart { get; set; }
        public TimeSpan MidDayEnd { get; set; }
        public string EmpConfigId { get; set; }

        public List<EmployeeConfigApiModels> ListEmployees { get; set; }
        public string WebHostUrl { get; set; }
        public MerchantConfigApiModels()
        {
            IsActived = true;
            ListEmployees = new List<EmployeeConfigApiModels>();
        }
    }

    public class EmployeeConfigApiModels
    {
        public string Id { get; set; }
        public string POSAPIMerchantConfigId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActived { get; set; }
    }

    public class ResponeMerchantConfig : ResultModels
    {
        public List<MerchantConfigApiModels> Data { get; set; }
        public string POSIntanceID { get; set; }
        public ResponeMerchantConfig()
        {
            Data = new List<MerchantConfigApiModels>();
        }
    }
}
