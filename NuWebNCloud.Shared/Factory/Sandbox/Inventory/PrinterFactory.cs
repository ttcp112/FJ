using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Sandbox.Inventory;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory.Sandbox.Inventory
{
    public class PrinterFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public PrinterFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<PrinterModels> GetListPrinter(string StoreId = null, string PrinterId = null, List<string> ListOrganizationId = null)
        {
            List<PrinterModels> listData = new List<PrinterModels>();
            try
            {
                PrinterModels paraBody = new PrinterModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.StoreId = StoreId;
                paraBody.Id = PrinterId;

                paraBody.ListOrgID = ListOrganizationId;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetPrinter, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListPrinter"];
                var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                listData = JsonConvert.DeserializeObject<List<PrinterModels>>(lstContent);
                return listData;
            }
            catch (Exception e)
            {
                _logger.Error("Printer_GetList: " + e);
                return listData;
            }
        }
    }
}
