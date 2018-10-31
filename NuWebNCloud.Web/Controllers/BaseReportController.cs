using Newtonsoft.Json;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    //[NuAuth]
    public class BaseReportController : Controller
    {
        // GET: BaseReport
        public ActionResult LoadEmployee(List<string> listData, int type)
        {
            BaseReportModel model = new BaseReportModel();
            try
            {
                //UserFactory factory = new UserFactory();
                //List<string> StoreId = listData;
                //if (type == Commons.TypeCompanySelected)
                //{
                //    StoreId = GetSelectedStoreIDCompany(listData);
                //}
                //List<UserModels> lstData = factory.GetAllEmployee(StoreId);
                //if (lstData != null)
                //{
                //    for (int i = 0; i < lstData.Count; i++)
                //    {
                //        model.ListEmployees.Add(new RPEmployeeItemModels()
                //        {
                //            ID = lstData[i].UserId,
                //            Name = lstData[i].Name,
                //            StoreId = lstData[i].StoreId,
                //            StoreName = lstData[i].StoreName
                //        });
                //    }
                //}

                model.ListEmployees = GetListEmployee(listData, type, false);
                /*Editor by Trongntn 10-07-2017*/
                model.ListStoreEmpl = model.ListEmployees
                                        .GroupBy(x => new { StoreId = x.StoreId, StoreName = x.StoreName })
                                        .Select(x => new StoreEmpl
                                        {
                                            StoreID = x.Key.StoreId,
                                            StoreName = x.Key.StoreName,
                                            ListEmployeesSel = new List<RPEmployeeItemModels>()
                                        }).ToList();
                int OffSet = 0;
                model.ListStoreEmpl.ForEach(x =>
                {
                    x.OffSet = OffSet++;
                    x.ListEmployeesSel = model.ListEmployees.Where(z => z.StoreId.Equals(x.StoreID)).ToList();
                });
            }
            catch (Exception)
            {
            }
            return PartialView("_FilterEmployee", model);
        }

        public List<string> GetSelectedStoreIDCompany(List<string> ListCompanys)
        {
            List<string> lstStore = new List<string>();
            if (ListCompanys.Count > 0)
            {
                for (int j = 0; j < ListCompanys.Count; j++)
                {
                    string CompanyID = ListCompanys[j];
                    StoreApiModels paraBody = new StoreApiModels();
                    //paraBody.AppKey = Commons.AppKey;
                    //paraBody.AppSecret = Commons.AppSecret;
                    //paraBody.CreatedUser = Commons.CreateUser;
                    paraBody.CompanyID = CompanyID;
                    var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetStore, null, paraBody);
                    dynamic data = result.Data;
                    var lstC = data["ListStore"];

                    var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                    var listItems = JsonConvert.DeserializeObject<List<StoreModels>>(lstContent);
                    if (listItems.Count != 0)
                    {
                        for (int i = 0; i < listItems.Count; i++)
                        {
                            var item = listItems[i];
                            lstStore.Add(item.Id.ToString());
                        }
                    }
                }
            }
            var user = (UserSession)System.Web.HttpContext.Current.Session["User"];
            if (user != null && user.ListStoreID != null)
            {
                lstStore = lstStore.Where(ww => user.ListStoreID.Contains(ww)).ToList();
            }
            return lstStore;
        }

        public List<RPEmployeeItemModels> GetListEmployee(List<string> listData, int type, bool isChecked)
        {
            List<RPEmployeeItemModels> result = new List<RPEmployeeItemModels>();
            try
            {
                UserFactory factory = new UserFactory();
                List<string> StoreId = listData;
                if (type == Commons.TypeCompanySelected)
                {
                    StoreId = GetSelectedStoreIDCompany(listData);
                }
                List<UserModels> lstData = factory.GetAllEmployee(StoreId);
                if (lstData != null)
                {
                    for (int i = 0; i < lstData.Count; i++)
                    {
                        result.Add(new RPEmployeeItemModels()
                        {
                            ID = lstData[i].UserId,
                            Name = lstData[i].Name,
                            StoreId = lstData[i].StoreId,
                            StoreName = lstData[i].StoreName,
                            Checked = isChecked
                        });
                    }
                    result = result.OrderBy(oo => oo.StoreName).ThenBy(n => n.Name).ToList();
                }
            }
            catch (Exception)
            {

            }
            return result;
        }
    }
}