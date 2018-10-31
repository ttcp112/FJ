using NLog;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class IngStoreSettingsController : BaseController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        StoreSettingFactory _factory = null;
        CompanyFactory _factoryCompany = null;
        List<string> listOrganization = new List<string>();
        List<SelectListItem> lstCompany = new List<SelectListItem>();
        List<string> listCompanyId = new List<string>();
        StoreFactory _storeFactory = null;
        List<SelectListItem> lstStore = new List<SelectListItem>();        
        public IngStoreSettingsController()
        {
            _storeFactory = new StoreFactory();
            _factory = new StoreSettingFactory();
            _factoryCompany = new CompanyFactory();
            //================            
            if (CurrentUser != null)
            {
                listOrganization = CurrentUser.ListOrganizationId;
            }
            lstCompany = _factoryCompany.GetListCompany(listOrganization);            
            listCompanyId = lstCompany.Select(x => x.Value).ToList();
            lstStore = _storeFactory.GetListStore(listOrganization);
            
        }

        // GET: IngStoreSettings
        public ActionResult Index()
        {
            try
            {
                StoreSettingViewModels model = new StoreSettingViewModels();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("StoreSettingIndex: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(StoreSettingViewModels model)
        {
            try
            {
                model.ListItem = _factory.GetData(model.ListStore, listCompanyId);
                if (model.ListStore != null && model.ListStore.Count > 0)
                {
                    model.ListItem.ForEach(x =>
                    {
                        x.StoreName = lstStore.Where(z => z.Value.Equals(x.StoreId)).FirstOrDefault().Text;
                    });
                }

            }
            catch (Exception e)
            {
                _logger.Error("StoreSettingSearch: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        [HttpPost]
        public ActionResult LoadDetail()
        {
            StoreSettingModels model = new StoreSettingModels();
            
            return PartialView("_PopUpDetail", model);
        }

        //[HttpPost]
        //public ActionResult SaveData(List<string> lstId, string quantity, string minAlert)
        //{
        //    try
        //    {
        //        string msg = "";
        //        string user = CurrentUser.UserName;

        //        List<StoreSettingModels> listData = new List<StoreSettingModels>();

        //        for (int i = 0; i < lstId.Count; i += 2)
        //        {
        //            string IngredientId = lstId[i];
        //            string StoreId = lstId[i + 1];
        //            listData.Add(new StoreSettingModels
        //            {
        //                IngredientId = IngredientId,
        //                StoreId = StoreId
        //            });
        //        }
        //        _factory.SaveData(listData, quantity, minAlert, user, ref msg);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(ex);
        //        return new HttpStatusCodeResult(400, ex.Message);
        //    }
        //    StoreSettingViewModels model = new StoreSettingViewModels();
        //    return RedirectToAction("Index", model);
        //}
        [HttpPost]
        public ActionResult SaveData(List<string> lstId, List<string> lstStore, string quantity, string minAlert)
        {
            try
            {
                string msg = "";
                string user = CurrentUser.UserName;

                List<StoreSettingModels> listData = new List<StoreSettingModels>();

                for (int i = 0; i < lstId.Count; i++)
                {
                    for (int y = 0; y < lstStore.Count; y++)
                    {
                        listData.Add(new StoreSettingModels
                        {
                            IngredientId = lstId[i],
                            StoreId = lstStore[y]
                        });
                    }                    
                }
                _factory.SaveData(listData, quantity, minAlert, user, ref msg);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
            StoreSettingViewModels model = new StoreSettingViewModels();
            return RedirectToAction("Index", model);
        }
    }
}