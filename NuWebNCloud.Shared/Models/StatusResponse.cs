using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models
{
    public class StatusResponse
    {
        public string IDReturn { get; set; }
        public bool Status { get; set; }
        public string MsgError { get; set; }

        public StatusResponse()
        {
            IDReturn = "";
            Status = false;
            MsgError = "";
        }
    }
}
