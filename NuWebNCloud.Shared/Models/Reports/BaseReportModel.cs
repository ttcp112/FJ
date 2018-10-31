using ClosedXML.Excel;
using Newtonsoft.Json;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using NuWebNCloud.Shared.Factory;

namespace NuWebNCloud.Shared.Models.Reports
{
    public class StoreEmpl
    {
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public int OffSet { get; set; }
        public List<RPEmployeeItemModels> ListEmployeesSel { get; set; }

    }

    public class StorePay
    {
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public int OffSet { get; set; }
        public List<RFilterChooseExtBaseModel> ListPaymentMethodSel { get; set; }
    }

    public class StoreSetMenu
    {
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public int OffSet { get; set; }
        public List<RFilterCategoryModel> ListSetMenuSel { get; set; }
    }
    public class StoreModifier
    {
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public int OffSet { get; set; }
        public List<RFilterCategoryModel> ListModifierSel { get; set; }
    }
    public class StoreCate
    {
        public string StoreID { get; set; }
        public string StoreName { get; set; }
        public int OffSet { get; set; }
        public List<RFilterCategoryModel> ListCategoriesSel { get; set; }
    }

    public class BaseReportModel
    {
        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime FromDate { get; set; } = DateTime.Now;

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime ToDate { get; set; } = DateTime.Now;

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/yyyy}")]
        public DateTime FromMonth { get; set; } = DateTime.Now;

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/yyyy}")]
        public DateTime ToMonth { get; set; } = DateTime.Now;

        //[Required(ErrorMessage = "Please choose at Store")]
        public List<string> ListStores { get; set; }

        //[Required(ErrorMessage = "Please choose at Company")]
        public List<string> ListCompanys { get; set; }

        public string FormatExport { get; set; } = "Excel";/*HTML*/

        public List<RPEmployeeItemModels> ListEmployees { get; set; }
        [Required(ErrorMessage = "Please choose employee.")]

        public List<StoreEmpl> ListStoreEmpl { get; set; }

        //[Required(ErrorMessage = "Please choose category.")]
        public List<RFilterCategoryModel> ListCategories { get; set; }
        public List<StoreCate> ListStoreCate { get; set; }

        //[Required(ErrorMessage = "Please choose category.")]
        public List<RFilterCategoryModel> ListSetMenu { get; set; }
        public List<RFilterCategoryModel> ListModifier { get; set; }
        public List<StorePay> ListStorePay { get; set; }
        /**/
        public List<RFilterChooseExtBaseModel> ListPaymentMethod { get; set; }
        public List<StoreSetMenu> ListStoreSetMenu { get; set; }
        public List<StoreModifier> ListStoreModifier { get; set; }
        public List<SelectListItem> ListType { get; set; }
        public int Type { get; set; }

        //2018/04/27
        public DateTime FromDateFilter { get; set; }
        public DateTime ToDateFilter { get; set; }

        // 05232018, Filter time : None = 0, OnDay = 1, PassDay = 2
        public int FilterType { get; set; }

        public BaseReportModel()
        {
            Type = Commons.TypeCompanySelected;
            GetTypeSelected();
            ListEmployees = new List<Reports.RPEmployeeItemModels>();
            ListSetMenu = new List<Reports.RFilterCategoryModel>();
            Mode = (int)Commons.EStatus.Actived;

            ListStoreEmpl = new List<StoreEmpl>();

            ListPaymentMethod = new List<RFilterChooseExtBaseModel>();
            ListStorePay = new List<StorePay>();

            ListStoreSetMenu = new List<StoreSetMenu>();
            ListStoreCate = new List<StoreCate>();

            ListCategories = new List<RFilterCategoryModel>();
        }

        private void GetTypeSelected()
        {
            ListType = new List<SelectListItem>()
            {
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.Company), Value = "1"},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.Store), Value = "2"}
            };
        }

        public List<StoreModels> GetSelectedStore(List<SelectListItem> vbStore)
        {
            List<SelectListItem> w1 = vbStore;
            var lstStore = (from m in w1
                            where ListStores.Any(w2 => m.Value == w2)
                            select new StoreModels
                            {
                                Id = m.Value,
                                Name = m.Text
                            }).ToList();

            if (lstStore != null && lstStore.Any())
                lstStore = lstStore.OrderBy(oo => oo.Name).ToList();
            //var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
            //if (currentUser != null)
            //    lstStore = lstStore.Where(x => currentUser.ListStoreID.Contains(x.Id)).ToList();
            return lstStore;
        }

        public List<StoreModels> GetSelectedStoreForMerchantExtend(List<StoreModels> vbStore)
        {
            List<StoreModels> w1 = vbStore;
            var lstStore = (from m in w1
                            where ListStores.Any(w2 => m.Id == w2)
                            select new StoreModels
                            {
                                Id = m.Id,
                                Name = m.Name,
                                CompanyId = m.CompanyId,
                                CompanyName = m.CompanyName,
                                IsIncludeTax = m.IsIncludeTax,
                                HostUrlExtend = m.HostUrlExtend,
                                NameExtend = m.NameExtend
                            }).ToList();

            if (lstStore != null && lstStore.Any())
                lstStore = lstStore.OrderBy(oo => oo.Name).ToList();
            //var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
            //if (currentUser != null)
            //    lstStore = lstStore.Where(x => currentUser.ListStoreID.Contains(x.Id)).ToList();
            return lstStore;
        }

        public List<StoreModels> GetSelectedStoreCompany()
        {
            List<StoreModels> lstStore = new List<StoreModels>();
            ListStores = new List<string>();
            if (ListCompanys.Count > 0)
            {
                TaxFactory taxFactory = new TaxFactory();
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
                            lstStore.Add(new StoreModels()
                            {
                                Id = item.Id.ToString(),
                                Name = item.Name,
                                CompanyId = item.CompanyId,
                                CompanyName = item.CompanyName,
                                IsIncludeTax = (taxFactory.GetDetailTaxForStore(item.Id) == (int)Commons.ETax.AddOn) ? false : true
                            });
                            //========
                            ListStores.Add(item.Id.ToString());

                            // Add value common for FJ Daily Sales report
                            if (item.StoreCode == Commons.Stall14StoreCode)
                            {
                                Commons.Stall14StoreId = item.Id.ToString();
                                Commons.Stall14StoreName = item.Name;
                            }
                        }
                    }
                }
            }
            if (lstStore != null && lstStore.Any())
                lstStore = lstStore.OrderBy(oo => oo.Name).ToList();
            var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
            if (currentUser != null)
                lstStore = lstStore.Where(x => currentUser.ListStoreID.Contains(x.Id)).ToList();
            return lstStore;
        }

        public List<StoreModels> GetSelectedStoreCompanyForExtend(List<StoreModels> lstStoreExtend)
        {
            List<StoreModels> lstStore = new List<StoreModels>();
            ListStores = new List<string>();
            if (ListCompanys != null && ListCompanys.Any())
            {
                lstStore = lstStoreExtend.Where(ww => ListCompanys.Contains(ww.CompanyId)).Select(ss => new StoreModels()
                {
                    Id = ss.Id,
                    Name = ss.Name,
                    CompanyId = ss.CompanyId,
                    CompanyName = ss.CompanyName,
                    IsIncludeTax = ss.IsIncludeTax,
                    HostUrlExtend = ss.HostUrlExtend,
                    NameExtend = ss.NameExtend
                }).ToList();
                
            }
            if (lstStore != null && lstStore.Any())
                lstStore = lstStore.OrderBy(oo => oo.Name).ToList();
      
            return lstStore;
        }

        public List<string> GetSelectedStoreIDCompany()
        {
            List<string> lstStore = new List<string>();
            ListStores = new List<string>();
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
                            //========
                            ListStores.Add(item.Id.ToString());
                        }
                    }
                }
            }
            return lstStore;
        }

        public int Mode { get; set; }
    }

    // For list stores view group by company
    public class StoresByCompany
    {
        public string id { get; set; }
        public string text { get; set; }
        public List<StoresChildren> children { get; set; }
        public StoresByCompany()
        {
            children = new List<StoresChildren>();
        }
    }
    public class StoresChildren
    {
        public string id { get; set; }
        public string text { get; set; }
    }
    #region Auto report
    public class BaseAutoReportRequestModel
    {
        public List<string> ListStoreIds { get; set; }
        public List<string> ListReportIds { get; set; }
        public int Mode { get; set; }
        public Boolean IsDaily { get; set; }
        public Boolean IsWeekly { get; set; }
        public Boolean IsMonthly { get; set; }
        public BaseAutoReportRequestModel()
        {
            ListStoreIds = new List<string>();
            ListReportIds = new List<string>();
        }
    }
    public class BaseAutoReportResponseModel : ResultModels
    {
        public string URLPath { get; set; }
    }
    #endregion End Auto report
    }

