using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
     public class BusinessDayModels
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime ClosedOn { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string LastUserModified { get; set; }
        public DateTime LastDateModified { get; set; }
        public int Mode { get; set; }
    }
    public class GetBusinessDayHistoryModelRequest : BaseApiRequestModel
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class GetShiftHistoryResponse : ResponseApiModels
    {
        public List<ShiftHistoryDTO> ListShiftHistory { get; set; }

        public GetShiftHistoryResponse()
        {
            ListShiftHistory = new List<ShiftHistoryDTO>();
        }
    }

    public class ShiftHistoryDTO
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? ClosedOn { get; set; }
        public bool IsCurrent { get; set; }
    }
}
