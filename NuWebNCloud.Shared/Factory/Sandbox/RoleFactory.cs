using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Sandbox;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuWebNCloud.Shared.Factory.Sandbox
{
    public class RoleFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public RoleFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<RoleModels> GetListRole(string StoreID = null, string RoleId = null, List<string> ListOrganizationId = null)
        {
            List<RoleModels> listData = new List<RoleModels>();
            try
            {
                CustomerApiModels paraBody = new CustomerApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.StoreID = StoreID;
                paraBody.ID = RoleId;
                paraBody.ListOrgID = ListOrganizationId;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetRole, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListRole"];
                var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                listData = JsonConvert.DeserializeObject<List<RoleModels>>(lstContent);
                if (listData != null && listData.Any())
                {
                    listData = listData.OrderBy(oo => oo.Name).ToList();
                }
                return listData;
            }
            catch (Exception e)
            {
                _logger.Error("Role_GetList: " + e);
                return listData;
            }
        }

        public bool InsertOrUpdateRole(RoleModels model, ref string msg)
        {
            try
            {
                RoleApiModels paraBody = new RoleApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.Name = model.Name;
                paraBody.IsActive = model.IsActive;
                paraBody.ListRoleModule = model.ListRoleModule;
                paraBody.ListRoleDrawer = model.ListRoleDrawer;

                paraBody.Id = model.ID;
                paraBody.StoreId = model.StoreId;
                

                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.CreateOrEditRole, null, paraBody);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        _logger.Error(result.Message);
                        msg = result.Message;
                        return false;
                    }
                }
                else
                {
                    _logger.Error(result);
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.Error("Role_InsertOrUpdate: " + e);
                return false;
            }
        }

        public bool DeleteRole(string ID, ref string msg)
        {
            try
            {
                RoleApiModels paraBody = new RoleApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();
                paraBody.Id = ID;

                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.DeleteRole, null, paraBody);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        msg = result.Message;
                        _logger.Error(result.Message);
                        return false;
                    }
                }
                else
                {
                    _logger.Error(result);
                    return false;
                }
            }
            catch (Exception e)
            {
                msg = e.ToString();
                _logger.Error("Role_Delete: " + e);
                return false;
            }
        }

    }
}
