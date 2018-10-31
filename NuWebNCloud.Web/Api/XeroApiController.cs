using NuWebNCloud.Shared.Factory.Xero;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Xero.GenerateInvoice;
using System.Web.Http;

namespace NuWebNCloud.Web.Api
{
    public class XeroApiController : ApiController
    {
        [HttpPost]
        public ResultModels GeneratePurchaseOrder(GeneratePurchaseOrderApiModels request)
        {
            NSLog.Logger.Info("XeroGeneratePurchaseOrderApi.......................", request);
            var result = new ResultModels();
            if (request != null)
            {
                XeroFactory _facXero = new XeroFactory();
                result = _facXero.GeneratePurchaseOrderAPI(request);
            }
            return result;
        }

        [HttpPost]
        public ResultModels GenerateReceiptNote(GenerateReceiptNoteApiModels request)
        {
            NSLog.Logger.Info("XeroGenerateReceiptNoteApi.......................", request);
            var result = new ResultModels();
            if (request != null)
            {
                XeroFactory _facXero = new XeroFactory();
                result = _facXero.GenerateReceiptNoteAPI(request);
            }
            return result;
        }
    }
}
