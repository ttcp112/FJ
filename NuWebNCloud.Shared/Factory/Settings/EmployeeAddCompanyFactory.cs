using NLog;
using NuWebNCloud.Shared.Integration.Models.Sandbox;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Sandbox;
using NuWebNCloud.Shared.Models.Settings.EmployeeAddCompany;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory.Settings
{
    public class EmployeeAddCompanyFactory : BaseFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public EmployeeAddCompanyFactory()
        {

        }

        public bool EmployeeAddCompany(InteEmployeeModels model, ref string msg)
        {
            try
            {
                CompanyModels obj;
                List<CompanyModels> ListComp = new List<CompanyModels>();
                if (model.LstCompany != null && model.LstCompany.Count > 0)
                {
                    foreach (var item in model.LstCompany)
                    {
                        obj = new CompanyModels();
                        obj.ID = item.Value;
                        ListComp.Add(obj);
                    }
                }
                EmployeeAddCompanyRequest paraBody = new EmployeeAddCompanyRequest();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.EmployeeID = model.ID;
                paraBody.ListCompany = ListComp;

                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.EmployeeAddCompany, null, paraBody);
                if (result != null)
                {
                    if (result.Success)
                    {                                     
                        return true;
                    }
                    else
                    {
                        msg = result.Message;
                        _logger.Error(result.Message);
                        return false;
                    }
                }
                else
                {
                    //msg = result.ToString();
                    _logger.Error(result);
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.Error("Employee Add Company: " + e);
                //msg = e.ToString();
                return false;
            }
        }
    }
}
