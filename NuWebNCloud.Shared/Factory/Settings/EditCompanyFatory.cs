using Newtonsoft.Json;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Settings.Company;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory.Settings
{
   public class EditCompanyFatory       
    {
        private static EditCompanyFatory _Company;
        public static EditCompanyFatory Company
        {
            get
            {
                return _Company = (_Company != null) ? _Company : new EditCompanyFatory();
            }
        }
        public List<CompanyModels> GetDetailCompany(string Id, List<string> ListOrganizationId = null)
        {
            List<CompanyModels> Data = new List<CompanyModels>();
            try
            {
                GetCompanyWebRequest paraBody = new GetCompanyWebRequest();

                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;             
                paraBody.ID = Id;
                paraBody.ListOrganizationID = ListOrganizationId;
                NSLog.Logger.Info("GetDetailCompany", paraBody);
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetDetailCompany, null, paraBody);
                NSLog.Logger.Info("GetDetailCompany", result);
                dynamic data = result.Data;
                var lstC = data["ListCompany"];
                var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                Data = JsonConvert.DeserializeObject<List<CompanyModels>>(lstContent);
                return Data;
            }
            catch (Exception e)
            {
                NSLog.Logger.Info("Get Detail Company", e);
                return Data;
            }
        }
        public bool UpdateCompany(CompanyModels model, ref string msg, List<string> ListOrganizationId = null)
        {
            try
            {
                CreateOrUpdateCompanyRequest paraBody = new CreateOrUpdateCompanyRequest();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;                
                paraBody.RegisterToken = new RegisterTokenModels();
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.ModifiedUser = Commons.CreateUser;
                paraBody.LastModified = DateTime.Now;
                paraBody.CreatedDate = DateTime.Now;
                paraBody.ID = model.ID;
                paraBody.OrganizationID = ListOrganizationId.FirstOrDefault();
                paraBody.CompanyName = model.Name;
                paraBody.Address = model.Address;
                paraBody.Zipcode = model.Zipcode; 
                NSLog.Logger.Info("UpdateCompany request", paraBody);
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.UpdateCompany, null, paraBody);
                NSLog.Logger.Info("UpdateCompany response", result);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        msg = result.Message;
                        NSLog.Logger.Info("UpdateCompany error", msg);
                        return false;
                    }
                }
                else
                {                    
                    NSLog.Logger.Info("UpdateCompany error", result);
                    return false;
                }
            }
            catch (Exception e)
            {
                NSLog.Logger.Info("UpdateCompany error", e);               
                return false;
            }
        }
    }
}
