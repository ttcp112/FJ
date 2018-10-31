using System.Collections.Generic;
using System.Web.Mvc;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models.Reports;
using System;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Models;
using NLog;
using System.Linq;

namespace NuWebNCloud.Web.Controllers
{
    public class BaseController : Controller
    {
        //private static Logger _logger = LogManager.GetCurrentClassLogger();
        private StoreFactory _storeFactory;
        //private CompanyFactory _companyFactory;
        protected List<StoreModels> listStoresInfoSession = new List<StoreModels>();

        public UserSession CurrentUser
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["User"] != null)
                    return (UserSession)System.Web.HttpContext.Current.Session["User"];
                else
                    return new UserSession();
            }
        }

        public BaseController()
        {
            // Updated 04022018, for set css menu toggle
            ViewBag.IsFromNuPos = CurrentUser.IsFromNuPos;

            /////////var user = System.Web.HttpContext.Current.Session["User"] as UserSession;

            //string areas = ControllerContext.RouteData.DataTokens["area"].ToString();
            //if (!user.IndustryId.Equals(areas))
            //{
            //    RedirectToAction("Index", "Home", new { area = "" });
            //}
            //============
            _storeFactory = new StoreFactory();
            /////////////////////_companyFactory = new CompanyFactory();

            // List Stores info from session, updated 07172018
            listStoresInfoSession = _storeFactory.GetListStoresInfo();

            //List<string> listOrganizationId = new List<string>();
            //var lstCompany = new List<SelectListItem>();
            //var lstStore = new List<SelectListItem>();
            //if (user != null)
            //{
            //    listOrganizationId = user.ListOrganizationId;
            //    if (user.IsMerchantExtend)
            //    {
            //        //get all list company & store extend
            //        var lstStoreReturn = new List<StoreModels>();
            //        var lstCompanyModelsReturn = new List<CompanyModels>();

            //        ViewBag.Companys = _companyFactory.GetListCompanyForMerchantExtend(user.HostApi, listOrganizationId, user.ListMerchantExtends, ref lstCompanyModelsReturn);
            //        ViewBag.Stores = _storeFactory.GetListStoreForMerchantExtend(user.HostApi,listOrganizationId, user.ListMerchantExtends, ref lstStoreReturn);
            //        ViewBag.StoresExtend = lstStoreReturn;
            //        ViewBag.CompanyExtend = lstCompanyModelsReturn;
            //        //For View, list stores group by company
            //        ViewBag.Stores_Group = new List<StoresByCompany>();
            //        if (lstStoreReturn != null && lstStoreReturn.Count > 0)
            //        {
            //            List<StoresByCompany> listStores_Group = new List<StoresByCompany>();
            //            StoresByCompany groupCompany = new StoresByCompany();
            //            var lstDataGroupCompany = lstStoreReturn.GroupBy(gg => new { CompanyId = gg.CompanyId, CompanyName = gg.CompanyName }).ToList();
            //            lstDataGroupCompany.ForEach(company => {
            //                groupCompany = new StoresByCompany();
            //                groupCompany.text = company.Key.CompanyName;
            //                var lstStores = company.ToList();
            //                lstStores.ForEach(store => {
            //                    groupCompany.children.Add(new StoresChildren()
            //                    {
            //                        id = store.Id,
            //                        text = store.Name
            //                    });
            //                });
            //                groupCompany.children = groupCompany.children.OrderBy(o => o.text).ToList();
            //                listStores_Group.Add(groupCompany);
            //            });
            //            listStores_Group = listStores_Group.OrderBy(o => o.text).ToList();
            //            ViewBag.Stores_Group = listStores_Group;
            //        }
            //    }
            //    else
            //    {
            //        ////////////ViewBag.Companys = _companyFactory.GetListCompany(listOrganizationId);
            //        ////////////lstCompany = ViewBag.Companys;
            //        ////////////ViewBag.Stores = _storeFactory.GetListStore(lstCompany);
            //        ////////////ViewBag.StoresIncludeCompany = _storeFactory.GetListStoreIncludeCompany(lstCompany);
            //        // For View, list stores group by company
            //        //ViewBag.Stores_Group = _storeFactory.GetListStore_View(lstCompany);
            //    }
            //}

            // Updated 07172018
            ViewBag.Companys = listStoresInfoSession.GroupBy(gg => gg.CompanyId).Select(ss => new SelectListItem()
            {
                Value = ss.Key,
                Text = ss.Select(s => s.CompanyName).FirstOrDefault()
            }).OrderBy(oo => oo.Text).ToList();
            ViewBag.StoresInfoSession = new SelectList(listStoresInfoSession, "Id", "Name", "CompanyName", 1);

            //var lstCompany = _companyFactory.GetListCompany(listOrganizationId);
            //ViewBag.Companys = lstCompany;
            //ViewBag.Stores = _storeFactory.GetListStore(lstCompany);

            List<ReportTimeItemModels> lstTimer = new List<ReportTimeItemModels>();
            for (int i = 0; i < 24; i++)
            {
                TimeSpan t = new TimeSpan(i, 0, 0);
                string sTime = t.ToString(@"hh\:mm");
                ReportTimeItemModels time = new ReportTimeItemModels();
                time.Value = sTime;
                time.Text = sTime;
                lstTimer.Add(time);
            }
            ViewBag.TIMER = lstTimer;
        }
    }
}