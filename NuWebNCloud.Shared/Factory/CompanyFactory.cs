using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Settings;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Factory
{
    public class CompanyFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public CompanyFactory()
        {
            _baseFactory = new BaseFactory();
        }

        //public bool Insert(List<CompanyModels> lstInfo)
        //{
        //    bool result = true;
        //    var info = lstInfo.FirstOrDefault();
        //    using (NuWebContext cxt = new NuWebContext())
        //    {
        //        using (var transaction = cxt.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                List<G_Company> lstInsert = new List<G_Company>();
        //                G_Company itemInsert = null;
        //                foreach (var item in lstInfo)
        //                {
        //                    itemInsert = new G_Company();
        //                    itemInsert.Id = item.Id;
        //                    itemInsert.OrganizationID = info.OrganizationID;
        //                    itemInsert.Name = item.Name;
        //                    itemInsert.Status = item.Status;
        //                    itemInsert.CreatedDate = item.CreatedDate;
        //                    itemInsert.CreatedUser = item.CreatedUser;
        //                    itemInsert.LastUserModified = item.LastUserModified;
        //                    itemInsert.LastDateModified = item.LastDateModified;
        //                    lstInsert.Add(itemInsert);
        //                }
        //                cxt.G_Company.AddRange(lstInsert);
        //                cxt.SaveChanges();
        //                transaction.Commit();
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.Error(ex);
        //                result = false;
        //                transaction.Rollback();
        //            }
        //            finally
        //            {
        //                if (cxt != null)
        //                    cxt.Dispose();
        //            }
        //        }
        //    }
        //    var jsonContent = JsonConvert.SerializeObject(lstInfo);
        //    _baseFactory.InsertTrackingLog("G_Company", jsonContent, "CompanyId", result);
        //    return result;
        //}

        public List<SelectListItem> GetListCompany(List<string> lstOrganizationID)
        {
            List<SelectListItem> listCompany = new List<SelectListItem>();
            try
            {
                //check to session
                //if (System.Web.HttpContext.Current.Session["GetCompanyListSession"] != null)
                //{
                //    listCompany = (List<SelectListItem>)System.Web.HttpContext.Current.Session["GetCompanyListSession"];
                //}
                //else
                //{
                    CompanyApiModels paraBody = new CompanyApiModels();
                    paraBody.CreatedUser = Commons.CreateUser;
                    paraBody.ListOrganizationID = lstOrganizationID;
                    var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetCompany, null, paraBody);
                    dynamic data = result.Data;
                    var lstC = data["ListCompany"];
                    var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                    var listItems = JsonConvert.DeserializeObject<List<CompanyModels>>(lstContent);
                    if (listItems.Count != 0)
                    {
                        for (int i = 0; i < listItems.Count; i++)
                        {
                            var item = listItems[i];
                            listCompany.Add(new SelectListItem()
                            {
                                Value = item.Id.ToString(),
                                Text = item.Name
                            });
                        }
                        listCompany = listCompany.OrderBy(s => s.Text).ToList();
                    }
                    //write to session
                //    if (listCompany != null && listCompany.Any())
                //    {
                //        HttpContext.Current.Session.Add("GetCompanyListSession", listCompany);
                //    }
                    
                //}
                return listCompany;
            }
            catch (Exception e)
            {
                _logger.Error("Company_GetList: " + e);
                return listCompany;
            }
        }

        public List<SelectListItem> GetListCompanyForMerchantExtend(string urlApi ,List<string> lstOrganizationID, List<MerchantExtendModel> listMerchantExtends, ref List<CompanyModels> lstCompanyReturn)
        {
            List<SelectListItem> listCompany = new List<SelectListItem>();
            try
            {
                //check to session
                //if (System.Web.HttpContext.Current.Session["GetCompanyExtendListSession"] != null && System.Web.HttpContext.Current.Session["GetCompanyExtendListFullSession"] != null)
                //{
                //    listCompany = (List<SelectListItem>)System.Web.HttpContext.Current.Session["GetCompanyExtendListSession"];
                //    lstCompanyReturn = (List<CompanyModels>)System.Web.HttpContext.Current.Session["GetCompanyExtendListFullSession"];
                //}
                //else
                //{
                    CompanyApiModels paraBody = new CompanyApiModels();
                    paraBody.ListOrganizationID = lstOrganizationID;
                    var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetCompany, null, paraBody);
                    dynamic data = result.Data;
                    var lstC = data["ListCompany"];
                    var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                    var listItems = JsonConvert.DeserializeObject<List<CompanyModels>>(lstContent);
                    if (listItems.Count != 0)
                    {
                        foreach (var item in listItems)
                        {
                            item.ApiUrlExtend = urlApi;
                            listCompany.Add(new SelectListItem()
                            {
                                Value = item.Id.ToString(),
                                Text = item.Name
                            });
                        }
                        lstCompanyReturn.AddRange(listItems);
                    }
                    //cross with orther merchant
                    foreach (var item in listMerchantExtends)
                    {

                        //var paraBodyExtend = new GetStoreWeb2Request();
                        //paraBodyExtend.ListStoreID = item.ListStoreIds;
                        //var resultExtend = (ResponseApiModels)ApiResponse.PostWithoutHostConfig<ResponseApiModels>(item.HostApiURL + "/" + Commons.Store_Get_Web2, null, paraBody);
                        //dynamic dataExtend = resultExtend.Data;
                        //var lstExtend = dataExtend["ListStore"];
                        //var lstContentExtend = JsonConvert.SerializeObject(lstExtend);
                        //List<StoreModels> tmp = JsonConvert.DeserializeObject<List<StoreModels>>(lstContentExtend);

                        var paraBodyExtend = new GetStoreWeb2Request();
                        paraBodyExtend.ListStoreID = item.ListStoreIds;
                        var resultExtend = (ResponseApiModels)ApiResponse.PostWithoutHostConfig<ResponseApiModels>(item.HostApiURL + "/" + Commons.GetStoresExtend, null, paraBodyExtend);
                        dynamic dataExtend = resultExtend.Data;
                        var lstExtend = dataExtend["ListStore"];
                        var lstContentExtend = JsonConvert.SerializeObject(lstExtend);
                        List<SStoreModels> tmp = JsonConvert.DeserializeObject<List<SStoreModels>>(lstContentExtend);

                        if (tmp != null && tmp.Any())
                        {
                            var lstCompanyExtend = tmp.Select(ss => new CompanyModels() { Id = ss.CompanyID, Name = ss.CompanyName, ApiUrlExtend = item.HostApiURL}).ToList();
                            for (int i = 0; i < tmp.Count; i++)
                            {
                                listCompany.Add(new SelectListItem()
                                {
                                    Value = tmp[i].CompanyID,
                                    Text = tmp[i].CompanyName
                                });
                            }
                            lstCompanyReturn.AddRange(lstCompanyExtend);

                        }
                    }
                    listCompany = listCompany.OrderBy(s => s.Text).ToList();
                    //write to session
                //    if (listCompany != null && listCompany.Any())
                //    {
                //        HttpContext.Current.Session.Add("GetCompanyExtendListSession", listCompany);
                //        HttpContext.Current.Session.Add("GetCompanyExtendListFullSession", lstCompanyReturn);
                //    }
                //}
                return listCompany;
            }
            catch (Exception e)
            {
                _logger.Error("CompanyExtend_GetList: " + e);
                return listCompany;
            }
        }
    }
}
