
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Models.Settings.TransactionReasonSetting;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class STransactionReasonSettingsController : BaseController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private TransactionReasonSettingFactory _factory = null;
        StoreFactory _storeFactory;
        List<StoreOnCompany> ListStoreOnComp;
        public STransactionReasonSettingsController()
        {
            
            _factory = new TransactionReasonSettingFactory();
            _storeFactory = new StoreFactory();
            //ViewBag.ListStore = GetListStore();
            ListStoreOnComp = new List<StoreOnCompany>();
            ////////var lstStore = ViewBag.StoresIncludeCompany;
            var lstStore = listStoresInfoSession;
            if (lstStore != null)
            {
                //foreach (var item in lstStore)
                //{
                //    StoreOnCompany itstore = new StoreOnCompany();

                //    itstore.Id = item.CompanyId;
                //    itstore.Name = item.CompanyName;
                //    itstore.IsCompany = true;
                //    itstore.Selected = false;
                //    itstore.CompId = item.CompanyId;
                //    var lst = ListStoreOnComp.Where(x => x.Id == itstore.Id).ToList();
                //    if (lst.Count == 0)
                //    {
                //        ListStoreOnComp.Add(itstore);
                //    }

                //}

                // List comp
                ListStoreOnComp = lstStore.GroupBy(gg => gg.CompanyId).Select(ss => new StoreOnCompany()
                {
                    Id = ss.Key,
                    Name = ss.Select(s => s.CompanyName).FirstOrDefault(),
                    IsCompany = true,
                    Selected = false,
                    CompId = ss.Key,
                    CompName = ss.Select(s => s.CompanyName).FirstOrDefault()
                }).ToList();

                foreach (var item in lstStore)
                {
                    StoreOnCompany itstore = new StoreOnCompany();

                    itstore.Id = item.Id;
                    itstore.Name = item.Name;
                    itstore.IsCompany = false;
                    itstore.Selected = false;
                    itstore.CompId = item.CompanyId;
                    itstore.CompName = item.CompanyName;
                    ListStoreOnComp.Add(itstore);
                }
                ListStoreOnComp = ListStoreOnComp.OrderBy(x => x.CompName).ThenBy(oo => oo.Name).ToList();
            }
            
        }
        // GET: STransactionReasonSettings
        public ActionResult Index()
        {
            try
            {
                ReasonViewModels model = new ReasonViewModels();
                model.StoreID = CurrentUser.StoreId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("Reason_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(ReasonViewModels model)
        {
            try
            {
                var data = _factory.GetListReason();
                data = data.Where(x => CurrentUser.ListStoreID.Contains(x.StoreID)).ToList();
                if (model.LstStoreID != null && model.LstStoreID.Count > 0)
                {
                    data = data.Where(o => model.LstStoreID.Contains(o.StoreID)).ToList();
                }

                model.LstReason = data;
            }
            catch (Exception ex)
            {
                _logger.Error("Reason_Search: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }

            return PartialView("_ListData", model);
        }

        public ReasonModels GetDetail(string id)
        {
            try
            {
                var data = _factory.GetDetailReason(id);
                int OffSet = 0;
                ReasonModels model = new ReasonModels();
                model = data;
                model.ListStoreOnComp = ListStoreOnComp;
                if (model.ListStore != null && model.ListStore.Count > 0)
                {
                    foreach (var item in model.ListStore)
                    {
                        item.IsDelete = true;
                        model.ID = item.ReasonID;
                        item.OffSet = OffSet++;
                        model.ListStoreOnComp.ForEach(
                            x =>
                            {
                                if (!x.Selected)
                                {
                                    x.Selected = x.Id.Equals(item.StoreID) ? true : false;
                                }
                                if (x.Selected == true)
                                {
                                    x.Disabled = true;
                                }
                            }
                            );
                    }
                    model.ListStore = model.ListStore.OrderBy(x => x.StoreName).ToList();
                }


                //var lstStore = (List<SelectListItem>)ViewBag.ListStore;
                //model.ListStoreView = lstStore;
                //model.ListStoreView = model.ListStoreView.OrderBy(o => o.Text).ToList();
                //if (model.ListStore != null && model.ListStore.Count > 0)
                //{
                //    foreach (var item in model.ListStore)
                //    {
                //        item.IsDelete = true;
                //        model.ID = item.ReasonID;
                //        item.OffSet = OffSet++;
                //        model.ListStoreView.ForEach(
                //            x =>
                //            {
                //                if (!x.Selected)
                //                {
                //                    x.Selected = x.Value.Equals(item.StoreID) ? true : false;

                //                }
                //            }
                //            );
                //    }
                //}

                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("Reason_Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public new PartialViewResult View(string id)
        {

            ReasonModels model = GetDetail(id);
            return PartialView("_View", model);
        }

        // add Store for Reason
        public ActionResult AddStore(int currentOffset, string StoreID, string StoreName)
        {

            StoreReasonDTO model = new StoreReasonDTO();
            model.OffSet = currentOffset;
            model.StoreID = StoreID;
            model.StoreName = StoreName;
            return PartialView("_ReasonOnStore", model);

        }
        public ActionResult Create()
        {
            ReasonModels model = new ReasonModels();
            //var lstStore = ViewBag.StoresIncludeCompany;
            //foreach (var item in lstStore)
            //{
            //    StoreOnCompany itstore = new StoreOnCompany();

            //    itstore.Id = item.CompanyId;
            //    itstore.Name = item.CompanyName;
            //    itstore.IsCompany = true;
            //    itstore.Selected = false;
            //    itstore.CompId = item.CompanyId;
            //    var lst = model.ListStoreOnComp.Where(x => x.Id == itstore.Id).ToList();
            //    if (lst.Count == 0)
            //    {
            //        model.ListStoreOnComp.Add(itstore);
            //    }

            //}
            //foreach (var item in lstStore)
            //{
            //    StoreOnCompany itstore = new StoreOnCompany();

            //    itstore.Id = item.Id;
            //    itstore.Name = item.Name;
            //    itstore.IsCompany = false;
            //    itstore.Selected = false;
            //    itstore.CompId = item.CompanyId;
            //    model.ListStoreOnComp.Add(itstore);
            //}
            //model.ListStoreOnComp = model.ListStoreOnComp.OrderBy(x => x.CompId).ToList();
            //model.ListStoreView = lstStore;
            model.ListStoreOnComp = ListStoreOnComp;
            return View(model);
        }
        [HttpPost]
        public ActionResult Create(ReasonModels model)
        {
            try
            {
                //if (string.IsNullOrEmpty(model.Name))
                //{
                //    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Reason field is required"));
                //    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                //    if (ListStoreOnComp != null && ListStoreOnComp.Count > 0)
                //    {
                //        model.ListStoreOnComp = ListStoreOnComp;
                //        var lstStore = model.ListStore.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                //        if (lstStore != null && lstStore.Count > 0)
                //        {
                //            foreach (var item in model.ListStoreOnComp)
                //            {
                //                var exits = lstStore.Where(x => x.StoreID.Equals(item.Id)).FirstOrDefault();
                //                if (exits != null)
                //                {
                //                    item.Selected = true;
                //                    item.Disabled = false;
                //                }
                //            }
                //            model.ListStore = model.ListStore.OrderBy(x => x.StoreName).ToList();
                //        }
                //    }

                //    return View(model);
                //}
                //if (model.Code != (byte)Commons.EReasonCode.Deposit && model.Code != (byte)Commons.EReasonCode.Payout)
                //{
                //    model.GLAccountCode = "";
                //}
                //string msg = "";
                //model.ListStore = model.ListStore.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();

                model.ListStore = model.ListStore.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();

                if (model.ListStore == null || !model.ListStore.Any())
                {
                    ModelState.AddModelError("ListStoreOnComp", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please Choose Store"));

                }
                if (string.IsNullOrEmpty(model.Name))
                {
                    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Reason field is required"));

                }
                if (model.Code != (byte)Commons.EReasonCode.Deposit && model.Code != (byte)Commons.EReasonCode.Payout)
                {
                    model.GLAccountCode = "";
                }
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    
                    model.ListStoreOnComp = ListStoreOnComp;
                    if (model.ListStoreOnComp == null)
                    {
                        model.ListStoreOnComp = new List<StoreOnCompany>();
                    } else
                    {
                        foreach (var item in model.ListStoreOnComp)
                        {
                            var exits = model.ListStore.Where(x => x.StoreID == item.Id).FirstOrDefault();
                            if (exits != null)
                            {
                                item.Selected = true;
                                item.Disabled = false;
                            }
                        }
                    }
                    int offSet = 0;
                    foreach (var store in model.ListStore)
                    {
                        store.OffSet = offSet;
                        offSet++;
                    }
                    return View(model);
                }
                string msg = "";
                //=====================
                bool result = _factory.InsertOrUpdateReason(model, ref msg);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg));
                    model.GLAccountCode = null;
                    //if (ListStoreOnComp != null && ListStoreOnComp.Count > 0)
                    //{
                    //    model.ListStoreOnComp = ListStoreOnComp;
                    //    var lstStore = model.ListStore.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                    //    if (lstStore != null && lstStore.Count > 0)
                    //    {
                    //        foreach (var item in model.ListStoreOnComp)
                    //        {
                    //            var exits = lstStore.Where(x => x.StoreID.Equals(item.Id)).FirstOrDefault();
                    //            if (exits != null)
                    //            {
                    //                item.Selected = true;
                    //                item.Disabled = false;
                    //            }
                    //        }
                    //        model.ListStore = model.ListStore.OrderBy(x => x.StoreName).ToList();
                    //    }
                    //}

                    model.ListStoreOnComp = ListStoreOnComp;
                    if (model.ListStoreOnComp == null)
                    {
                        model.ListStoreOnComp = new List<StoreOnCompany>();
                    }
                    else
                    {
                        foreach (var item in model.ListStoreOnComp)
                        {
                            var exits = model.ListStore.Where(x => x.StoreID.Equals(item.Id)).FirstOrDefault();
                            if (exits != null)
                            {
                                item.Selected = true;
                                item.Disabled = false;
                            }
                        }
                    }
                    int offSet = 0;
                    foreach (var store in model.ListStore)
                    {
                        store.OffSet = offSet;
                        offSet++;
                    }
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Reason_Create : " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
        [HttpGet]
        public PartialViewResult Delete(string id)
        {
            ReasonModels model = GetDetail(id);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(ReasonModels model)
        {
            try
            {
                string msg = "";
                var result = _factory.DeleteReason(model.ID, ref msg);
                if (!result)
                {
                    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg));
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Delete", model);
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.Error("Reason_Delete: " + ex);
                ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Have an error when you delete a Reason"));
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        public PartialViewResult Edit(string id)
        {
            ReasonModels model = GetDetail(id);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(ReasonModels model)
        {
            try
            {
                model.ListStore = model.ListStore.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();

                if (model.ListStore == null || !model.ListStore.Any())
                {
                    ModelState.AddModelError("ListStoreOnComp", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please Choose Store"));

                }
                if (string.IsNullOrEmpty(model.Name))
                {
                    ModelState.AddModelError("Name", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Reason field is required"));

                }
                if (model.Code != (byte)Commons.EReasonCode.Deposit && model.Code != (byte)Commons.EReasonCode.Payout)
                {
                    model.GLAccountCode = "";
                }
                if (!ModelState.IsValid)
                {
                    var lstchoose = model.ListStore;

                    Response.StatusCode = (int)HttpStatusCode.BadRequest;                    
                    model = GetDetail(model.ID);
                    //////var offset = model.ListStore.Count();
                    if(lstchoose!= null && lstchoose.Any())
                    {
                        foreach(var item in model.ListStoreOnComp)
                        {
                            var exits = lstchoose.Where(x => x.StoreID == item.Id).FirstOrDefault();
                            if(exits != null && !item.Disabled)
                            {
                                item.Selected = true;
                                item.Disabled = false;
                                
                                StoreReasonDTO store = new StoreReasonDTO();
                                //////store.OffSet = offset++;
                                store.StoreID = item.Id;
                                store.StoreName = item.Name;
                                model.ListStore.Add(store);
                            }
                        }
                        
                    }
                    int offSet = 0;
                    foreach (var store in model.ListStore)
                    {
                        store.OffSet = offSet;
                        offSet++;
                    }
                    //////model.ListStore = model.ListStore.OrderBy(x => x.StoreName).ToList();
                    return PartialView("_Edit", model);
                }

                //==================== 
                string msg = "";
                var result = _factory.InsertOrUpdateReason(model, ref msg);

                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    var lstchoose = model.ListStore;

                    ModelState.AddModelError("Height", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg));
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    model = GetDetail(model.ID);
                    //////var offset = model.ListStore.Count();
                    if (lstchoose != null && lstchoose.Any())
                    {
                        foreach (var item in model.ListStoreOnComp)
                        {
                            var exits = lstchoose.Where(x => x.StoreID == item.Id).FirstOrDefault();
                            if (exits != null && !item.Disabled)
                            {
                                item.Selected = true;
                                item.Disabled = false;

                                StoreReasonDTO store = new StoreReasonDTO();
                                //////store.OffSet = offset++;
                                store.StoreID = item.Id;
                                store.StoreName = item.Name;
                                model.ListStore.Add(store);
                            }
                        }

                    }
                    int offSet = 0;
                    foreach (var store in model.ListStore)
                    {
                        store.OffSet = offSet;
                        offSet++;
                    }
                    //////model.ListStore = model.ListStore.OrderBy(x => x.StoreName).ToList();
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Reason_Edit: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
    }
}