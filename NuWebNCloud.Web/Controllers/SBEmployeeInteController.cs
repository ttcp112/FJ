using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Integration.Factory.Sandbox;
using NuWebNCloud.Shared.Integration.Models.Sandbox;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class SBEmployeeInteController : HQController
    {
        // GET: SBEmployeeInte
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private InteEmployeeFactory _factory = null;
        List<string> listPropertyReject = null;

        public SBEmployeeInteController()
        {
            _factory = new InteEmployeeFactory();
            //ViewBag.ListStore = GetListStoreIntegration();
            listPropertyReject = new List<string>();
            listPropertyReject.Add("Marital");
        }

        public ActionResult Index()
        {
            try
            {
                InteEmployeeViewModels model = new InteEmployeeViewModels();
                model.StoreID = CurrentUser.StoreId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("Employee_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(InteEmployeeViewModels model)
        {
            try
            {
                var data = _factory.GetListEmployee(model.StoreID, null, CurrentUser.ListOrganizationId);
                var data02 = new List<InteEmployeeModels>();
                foreach (var item in data)
                {
                    item.ImageURL = string.IsNullOrEmpty(item.ImageURL) ? Commons.Image100_100 : item.ImageURL;
                    if (item.ListEmpStore != null && item.ListEmpStore.Count > 0)
                    {
                        item.StoreName = item.ListEmpStore[0].StoreName;
                        item.RoleName = item.ListEmpStore[0].RoleName;
                        item.StoreID = item.ListEmpStore[0].StoreID;
                        item.RoleID = item.ListEmpStore[0].RoleID;
                    }
                    //======
                    if (item.ListEmpStore != null && item.ListEmpStore.Count > 1)
                    {
                        InteEmployeeModels emp = null;
                        for (int i = item.ListEmpStore.Count - 1; i >= 1; i--)
                        {
                            InteEmployeeOnStoreModels empOnStore = item.ListEmpStore[i];

                            List<InteEmployeeOnStoreModels> ListEmpStore = new List<InteEmployeeOnStoreModels>();
                            ListEmpStore.Add(new InteEmployeeOnStoreModels
                            {
                                EmployeeID = item.ID,
                                RoleID = empOnStore.RoleID,
                                RoleName = empOnStore.RoleName,
                                StoreID = empOnStore.StoreID,
                                StoreName = empOnStore.StoreName
                            });

                            emp = new InteEmployeeModels()
                            {
                                BirthDate = item.BirthDate,
                                City = item.City,
                                Country = item.Country,
                                Email = item.Email,
                                Gender = item.Gender,
                                HiredDate = item.HiredDate,
                                ID = item.ID,
                                ImageURL = item.ImageURL,
                                IsActive = item.IsActive,
                                ListMarital = item.ListMarital,
                                ListRole = item.ListRole,
                                ListWorkingTime = item.ListWorkingTime,
                                Marital = item.Marital,
                                Mode = item.Mode,
                                Name = item.Name,
                                Phone = item.Phone,
                                Pincode = item.Pincode,

                                RoleName = empOnStore.RoleName,
                                StoreName = empOnStore.StoreName,
                                StoreID = empOnStore.StoreID,
                                RoleID = empOnStore.RoleID,
                                ListEmpStore = ListEmpStore,

                                Street = item.Street,
                                TimeItems = item.TimeItems,
                                Zipcode = item.Zipcode
                            };

                            data02.Add(emp);
                            //item.ListEmpStore.Remove(empOnStore);
                        }
                    }
                }
                data.AddRange(data02);
                if (model.ListStores != null && model.ListStores.Count > 0)
                {
                    if(data != null && data.Count > 0)
                        data = data.Where(x => model.ListStores.Contains(x.StoreID)).ToList();
                }
                model.ListItem = data;


            }
            catch (Exception e)
            {
                _logger.Error("Employee_Search: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult Create()
        {
            InteEmployeeModels model = new InteEmployeeModels();
            model.GetValue();
            //===========
            //var lstStore = (List<SelectListItem>)ViewBag.ListStore;
            var lstStore = (SelectList)ViewBag.StoreID;
            model.ListStore = lstStore.ToList();
            return View(model);
        }

        public void PropertyReject()
        {
            foreach (var item in listPropertyReject)
            {
                if (ModelState.ContainsKey(item))
                    ModelState[item].Errors.Clear();
            }
        }

        [HttpPost]
        public ActionResult Create(InteEmployeeModels model)
        {
            try
            {
                byte[] photoByte = null;

                PropertyReject();
                if (string.IsNullOrEmpty(model.Name))
                {
                    ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Name field is required"));
                }
                model.ListStore = model.ListStore.Where(x => x.Selected).ToList();
                if (model.ListStore.Count == 0)
                {
                    ModelState.AddModelError("ListStore", CurrentUser.GetLanguageTextFromKey("Please choose store"));
                }
                //===============
                if (model.PictureUpload != null && model.PictureUpload.ContentLength > 0)
                {
                    Byte[] imgByte = new Byte[model.PictureUpload.ContentLength];
                    model.PictureUpload.InputStream.Read(imgByte, 0, model.PictureUpload.ContentLength);
                    model.PictureByte = imgByte;
                    model.ImageURL = Guid.NewGuid() + Path.GetExtension(model.PictureUpload.FileName);
                    model.PictureUpload = null;
                    photoByte = imgByte;
                }
                ////Working Time
                //for (int i = 0; i < model.ListWorkingTime.Count; i++)
                //{
                //    var item = model.ListWorkingTime[i];
                //    item.IsOffline = (item.From.ToLower().Equals("off") || item.To.ToLower().Equals("off")) ? true : false;
                //    if (!item.From.ToLower().Equals("off") && !item.To.ToLower().Equals("off"))
                //    {
                //        item.FromTime = Convert.ToDateTime(item.From);
                //        item.ToTime = Convert.ToDateTime(item.To);
                //        if (item.FromTime.TimeOfDay > item.ToTime.TimeOfDay)
                //        {
                //            ModelState.AddModelError("WorkingTime", "From time must be less than To time");
                //            break;
                //        }
                //    }
                //}
                //model.ListEmpStore = new List<EmployeeOnStoreModels>();
                //model.ListEmpStore.Add(new EmployeeOnStoreModels
                //{
                //    RoleID = model.RoleID,
                //    StoreID = model.StoreID
                //});

                model.ListStoreID = model.ListRoleWorkingTime.Select(x => x.StoreID).ToList();
                model.ListRoleWorkingTime = model.ListRoleWorkingTime.Where(x => x.Status != (int)Commons.EStatus.Deleted).ToList();
                if (model.ListRoleWorkingTime != null && model.ListRoleWorkingTime.Count > 0)
                {
                    InteEmployeeOnStoreModels EmpOnStore = null;
                    foreach (var item in model.ListRoleWorkingTime)
                    {
                        if (item.RoleID == null)
                        {
                            ModelState.AddModelError("RoleWorkingTimeAlert_" + item.StoreID, CurrentUser.GetLanguageTextFromKey("Please choose role for store")+ "[" + item.StoreName + "]");
                            break;
                        }
                        ////Role - Store
                        EmpOnStore = new InteEmployeeOnStoreModels();
                        EmpOnStore.RoleID = item.RoleID;
                        EmpOnStore.StoreID = item.StoreID;
                        EmpOnStore.IsSyncPoins = item.IsSyncPoins;
                        model.ListEmpStore.Add(EmpOnStore);

                        ////Working Time
                        for (int i = 0; i < item.ListWorkingTime.Count; i++)
                        {
                            var itemWorkingTime = item.ListWorkingTime[i];
                            itemWorkingTime.IsOffline = (itemWorkingTime.From.Equals(CurrentUser.GetLanguageTextFromKey("OFF")) || itemWorkingTime.To.Equals(CurrentUser.GetLanguageTextFromKey("OFF"))) ? true : false;
                            if (!itemWorkingTime.From.Equals(CurrentUser.GetLanguageTextFromKey("OFF")) && !itemWorkingTime.To.Equals(CurrentUser.GetLanguageTextFromKey("OFF")))
                            {
                                itemWorkingTime.FromTime = Convert.ToDateTime(itemWorkingTime.From);
                                itemWorkingTime.ToTime = Convert.ToDateTime(itemWorkingTime.To);
                                if (itemWorkingTime.FromTime.TimeOfDay > itemWorkingTime.ToTime.TimeOfDay)
                                {
                                    ModelState.AddModelError("RoleWorkingTimeAlert_" + item.StoreID, CurrentUser.GetLanguageTextFromKey("From time must be less than To time for store")+ "[" + item.StoreName + "]");
                                    break;
                                }
                            }
                            //WorkingTime
                            model.ListWorkingTime.Add(itemWorkingTime);
                        }
                    }
                }

                if (!ModelState.IsValid)
                {
                    if ((ModelState["PictureUpload"]) != null && (ModelState["PictureUpload"]).Errors.Count > 0)
                        model.ImageURL = "";
                    model.GetValue();

                    //===========
                    //var lstStore = (List<SelectListItem>)ViewBag.ListStore;
                    var lstStore = (SelectList)ViewBag.StoreID;
                    model.ListStore = lstStore.ToList();
                    /*===*/
                    if (model.ListRoleWorkingTime != null)
                    {
                        model.ListRoleWorkingTime.ForEach(x =>
                        {
                            x.GetListRole(x.StoreID, CurrentUser.ListOrganizationId);
                        });
                    }

                    return View(model);
                }
                //====================
                string msg = "";
                bool result = _factory.InsertOrUpdateEmployee(model, ref msg);
                if (result)
                {
                    //Save Image on Server
                    if (!string.IsNullOrEmpty(model.ImageURL) && model.PictureByte != null)
                    {
                        var originalDirectory = new DirectoryInfo(string.Format("{0}Uploads\\", Server.MapPath(@"\")));
                        var path = string.Format("{0}{1}", originalDirectory, model.ImageURL);
                        MemoryStream ms = new MemoryStream(photoByte, 0, photoByte.Length);
                        ms.Write(photoByte, 0, photoByte.Length);
                        System.Drawing.Image imageTmp = System.Drawing.Image.FromStream(ms, true);

                        ImageHelper.Me.SaveCroppedImage(imageTmp, path, model.ImageURL, ref photoByte);
                        model.PictureByte = photoByte;

                        FTP.Upload(model.ImageURL, model.PictureByte);

                        ImageHelper.Me.TryDeleteImageUpdated(path);
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    model.GetValue();
                    //===========
                    //var lstStore = (List<SelectListItem>)ViewBag.ListStore;
                    var lstStore = (SelectList)ViewBag.StoreID;
                    model.ListStore = lstStore.ToList();
                    //return RedirectToAction("Create");
                    /*===*/
                    if (model.ListRoleWorkingTime != null)
                    {
                        model.ListRoleWorkingTime.ForEach(x =>
                        {
                            x.GetListRole(x.StoreID, CurrentUser.ListOrganizationId);
                        });
                    }
                    ModelState.AddModelError("name", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return View("Create", model);
                }
            }
            catch (FormatException fEx)
            {
                _logger.Error("Employee_Edit: " + fEx.Message);
                ModelState.AddModelError("name", fEx.Message);
                //===========
                //var lstStore = (List<SelectListItem>)ViewBag.ListStore;
                var lstStore = (SelectList)ViewBag.StoreID;
                model.ListStore = lstStore.ToList();
                /*===*/
                if (model.ListRoleWorkingTime != null)
                {
                    model.ListRoleWorkingTime.ForEach(x =>
                    {
                        x.GetListRole(x.StoreID, CurrentUser.ListOrganizationId);
                    });
                }
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return View("Create", model);
            }
            catch (Exception ex)
            {
                _logger.Error("Employee_Create: " + ex);
                //return new HttpStatusCodeResult(400, ex.Message);
                ModelState.AddModelError("name", CurrentUser.GetLanguageTextFromKey("Have an error"));

                //===========
                //var lstStore = (List<SelectListItem>)ViewBag.ListStore;
                var lstStore = (SelectList)ViewBag.StoreID;
                model.ListStore = lstStore.ToList();
                /*===*/
                if (model.ListRoleWorkingTime != null)
                {
                    model.ListRoleWorkingTime.ForEach(x =>
                    {
                        x.GetListRole(x.StoreID, CurrentUser.ListOrganizationId);
                    });
                }
                //return View(model);
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return View("Create", model);
            }
        }

        public InteEmployeeModels GetDetail(string id, string StoreId)
        {
            try
            {
                InteEmployeeModels model = new InteEmployeeModels();
                var listEmp = _factory.GetListEmployee(null, id);
                if (listEmp != null && listEmp.Any())
                {
                    model = listEmp.FirstOrDefault();
                }
                model.ImageURL = string.IsNullOrEmpty(model.ImageURL) ? Commons.Image100_100 : model.ImageURL;
                model.HiredDate = model.HiredDate.ToLocalTime();
                model.BirthDate = model.BirthDate.ToLocalTime();

                //==========
                InteRoleWorkingTime rWorkingTime = null;
                int OffSet = 0;
                if (model.ListEmpStore != null)
                {
                    model.ListEmpStore = model.ListEmpStore.OrderBy(oo => oo.StoreName).ToList();
                    foreach (var emp in model.ListEmpStore)
                    {
                        rWorkingTime = new InteRoleWorkingTime();
                        rWorkingTime.ListWorkingTime.Clear();
                        //if (model.ListEmpStore != null)
                        //{
                        var empOnStore = model.ListEmpStore.Where(x => x.StoreID.Equals(emp.StoreID)).FirstOrDefault();
                        if (empOnStore != null)
                        {
                            model.StoreID = empOnStore.StoreID;
                            model.StoreName = empOnStore.StoreName;

                            model.RoleID = empOnStore.RoleID;
                            model.RoleName = empOnStore.RoleName;

                            //===========
                            rWorkingTime.OffSet = OffSet;
                            rWorkingTime.Status = (int)Commons.EStatus.Actived;

                            rWorkingTime.RoleID = empOnStore.RoleID;
                            rWorkingTime.RoleName = empOnStore.RoleName;

                            rWorkingTime.StoreID = empOnStore.StoreID;
                            rWorkingTime.StoreName = empOnStore.StoreName;
                            rWorkingTime.IsWithPoins = CurrentUser.listStore.Where(x => x.ID == rWorkingTime.StoreID).FirstOrDefault().IsWithPoins;
                            rWorkingTime.IsSyncPoins = empOnStore.IsSyncPoins;
                            rWorkingTime.GetListRole(rWorkingTime.StoreID, CurrentUser.ListOrganizationId);
                            //===========
                        }
                        //============
                        //var lstStore = (List<SelectListItem>)ViewBag.ListStore;
                        var lstStore = (SelectList)ViewBag.StoreID;
                        model.ListStore = lstStore.ToList();
                        foreach (var item in model.ListStore)
                        {
                            item.Disabled = model.ListEmpStore.Where(x => x.StoreID.Equals(item.Value)).FirstOrDefault() == null ? false : true;
                            item.Selected = model.ListEmpStore.Where(x => x.StoreID.Equals(item.Value)).FirstOrDefault() == null ? false : true;  //item.Value.Equals(StoreId);
                        }
                        //}
                        if (model.ListWorkingTime != null)
                        {

                            foreach (var itemStore in model.ListWorkingTime)
                            {
                                if (string.IsNullOrEmpty(itemStore.StoreID))
                                {
                                    itemStore.StoreID = emp.StoreID;
                                }
                            }
                            //=========
                            //model.ListWorkingTime = model.ListWorkingTime.Where(x => x.StoreID.Equals(emp.StoreID)).OrderBy(x => x.Day).ToList();
                            var lstWorkingTime = model.ListWorkingTime.Where(x => x.StoreID.Equals(emp.StoreID)).OrderBy(x => x.Day).ToList();
                            for (int i = 0; i < lstWorkingTime.Count; i++)
                            {
                                var item = lstWorkingTime[i];
                                if (item.IsOffline)
                                {
                                    item.From = CurrentUser.GetLanguageTextFromKey("OFF");
                                    item.To = CurrentUser.GetLanguageTextFromKey("OFF");
                                }
                                else
                                {
                                    item.From = item.FromTime.ToLocalTime().ToString("HH:mm");
                                    item.To = item.ToTime.ToLocalTime().ToString("HH:mm");
                                }
                                switch (item.Day)
                                {
                                    case 2:
                                        item.StrDate = CurrentUser.GetLanguageTextFromKey("Mon");
                                        break;
                                    case 3:
                                        item.StrDate = CurrentUser.GetLanguageTextFromKey("Tue");
                                        break;
                                    case 4:
                                        item.StrDate = CurrentUser.GetLanguageTextFromKey("Wed");
                                        break;
                                    case 5:
                                        item.StrDate = CurrentUser.GetLanguageTextFromKey("Thu");
                                        break;
                                    case 6:
                                        item.StrDate = CurrentUser.GetLanguageTextFromKey("Fri");
                                        break;
                                    case 7:
                                        item.StrDate = CurrentUser.GetLanguageTextFromKey("Sat");
                                        break;
                                    case 8:
                                        item.StrDate = CurrentUser.GetLanguageTextFromKey("Sun");
                                        break;
                                }
                                rWorkingTime.ListWorkingTime.Add(item);
                            }
                        }
                        model.ListRoleWorkingTime.Add(rWorkingTime);
                        OffSet++;
                    }
                }
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("Employee_Detail: " + ex);
                return null;
            }
        }


        public InteEmployeeModels GetDetailView(string id, string StoreId)
        {
            try
            {
                InteEmployeeModels model = new InteEmployeeModels();
                var listEmp = _factory.GetListEmployee(null, id);
                if (listEmp != null && listEmp.Any())
                {
                    model = listEmp.FirstOrDefault();
                }
                model.ImageURL = string.IsNullOrEmpty(model.ImageURL) ? Commons.Image100_100 : model.ImageURL;
                model.HiredDate = model.HiredDate.ToLocalTime();
                model.BirthDate = model.BirthDate.ToLocalTime();

                //==========
                InteRoleWorkingTime rWorkingTime = null;
                if (model.ListEmpStore != null && model.ListEmpStore.Any())
                {
                    // If case View, only get store info with id = StoreId
                    var emp = model.ListEmpStore.Where(w => w.StoreID == StoreId).FirstOrDefault();

                    if (emp != null)
                    {
                        model.StoreID = emp.StoreID;
                        model.StoreName = emp.StoreName;
                        model.IsWithPoins = CurrentUser.listStore.Where(x => x.ID == model.StoreID).FirstOrDefault().IsWithPoins;
                        model.IsSyncPoins = emp.IsSyncPoins;
                        model.RoleID = emp.RoleID;
                        model.RoleName = emp.RoleName;

                        //===========
                        rWorkingTime = new InteRoleWorkingTime();
                        rWorkingTime.ListWorkingTime.Clear();

                        rWorkingTime.OffSet = 0;
                        rWorkingTime.Status = (int)Commons.EStatus.Actived;

                        rWorkingTime.RoleID = emp.RoleID;
                        rWorkingTime.RoleName = emp.RoleName;

                        rWorkingTime.StoreID = emp.StoreID;
                        rWorkingTime.StoreName = emp.StoreName;
                        
                        rWorkingTime.GetListRole(rWorkingTime.StoreID, CurrentUser.ListOrganizationId);

                        //============
                        var lstStore = (SelectList)ViewBag.StoreID;
                        model.ListStore = lstStore.ToList();
                        foreach (var item in model.ListStore)
                        {
                            item.Disabled = model.ListEmpStore.Where(x => x.StoreID.Equals(item.Value)).FirstOrDefault() == null ? false : true;
                            item.Selected = model.ListEmpStore.Where(x => x.StoreID.Equals(item.Value)).FirstOrDefault() == null ? false : true;  
                        }

                        if (model.ListWorkingTime != null)
                        {

                            foreach (var itemStore in model.ListWorkingTime)
                            {
                                if (string.IsNullOrEmpty(itemStore.StoreID))
                                {
                                    itemStore.StoreID = emp.StoreID;
                                }
                            }
                            //=========
                            var lstWorkingTime = model.ListWorkingTime.Where(x => x.StoreID.Equals(emp.StoreID)).OrderBy(x => x.Day).ToList();
                            for (int i = 0; i < lstWorkingTime.Count; i++)
                            {
                                var item = lstWorkingTime[i];
                                if (item.IsOffline)
                                {
                                    item.From = CurrentUser.GetLanguageTextFromKey("OFF");
                                    item.To = CurrentUser.GetLanguageTextFromKey("OFF");
                                }
                                else
                                {
                                    item.From = item.FromTime.ToLocalTime().ToString("HH:mm");
                                    item.To = item.ToTime.ToLocalTime().ToString("HH:mm");
                                }
                                switch (item.Day)
                                {
                                    case 2:
                                        item.StrDate = CurrentUser.GetLanguageTextFromKey("Mon");
                                        break;
                                    case 3:
                                        item.StrDate = CurrentUser.GetLanguageTextFromKey("Tue");
                                        break;
                                    case 4:
                                        item.StrDate = CurrentUser.GetLanguageTextFromKey("Wed");
                                        break;
                                    case 5:
                                        item.StrDate = CurrentUser.GetLanguageTextFromKey("Thu");
                                        break;
                                    case 6:
                                        item.StrDate = CurrentUser.GetLanguageTextFromKey("Fri");
                                        break;
                                    case 7:
                                        item.StrDate = CurrentUser.GetLanguageTextFromKey("Sat");
                                        break;
                                    case 8:
                                        item.StrDate = CurrentUser.GetLanguageTextFromKey("Sun");
                                        break;
                                }
                                rWorkingTime.ListWorkingTime.Add(item);
                            }
                        }
                        model.ListRoleWorkingTime.Add(rWorkingTime);
                    }
                }
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("Employee_Detail_View: " + ex);
                return null;
            }
        }
        [HttpGet]
        public PartialViewResult View(string id, string StoreId)
        {
            InteEmployeeModels model = GetDetailView(id, StoreId);
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id, string StoreId)
        {
            InteEmployeeModels model = GetDetail(id, StoreId);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(InteEmployeeModels model)
        {
            try
            {
                byte[] photoByte = null;
                PropertyReject();

                if (model.ListStore.Count(x => x.Selected) == 0)
                    ModelState.AddModelError("ListStore", CurrentUser.GetLanguageTextFromKey("Please choose store"));

                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("Name is required"));

                if (!string.IsNullOrEmpty(model.ImageURL))
                {
                    model.ImageURL = model.ImageURL.Replace(Commons._PublicImages, "").Replace(Commons.Image100_100, "");
                }
                if (model.PictureUpload != null && model.PictureUpload.ContentLength > 0)
                {
                    Byte[] imgByte = new Byte[model.PictureUpload.ContentLength];
                    model.PictureUpload.InputStream.Read(imgByte, 0, model.PictureUpload.ContentLength);
                    model.PictureByte = imgByte;
                    model.ImageURL = Guid.NewGuid() + Path.GetExtension(model.PictureUpload.FileName);
                    model.PictureUpload = null;
                    photoByte = imgByte;
                }
                //else
                //    model.ImageURL = model.ImageURL.Replace(Commons._PublicImages, "").Replace(Commons.Image100_100, "");
                ////Working Time
                //for (int i = 0; i < model.ListWorkingTime.Count; i++)
                //{
                //    var item = model.ListWorkingTime[i];
                //    item.IsOffline = (item.From.ToLower().Equals("off") || item.To.ToLower().Equals("off")) ? true : false;
                //    if (!item.From.ToLower().Equals("off") && !item.To.ToLower().Equals("off"))
                //    {
                //        item.FromTime = Convert.ToDateTime(item.From);
                //        item.ToTime = Convert.ToDateTime(item.To); ;
                //        if (item.FromTime.TimeOfDay > item.ToTime.TimeOfDay)
                //        {
                //            ModelState.AddModelError("WorkingTime", "From time must be less than To time");
                //            break;
                //        }
                //    }
                //}
                //model.ListEmpStore = new List<EmployeeOnStoreModels>();
                //model.ListEmpStore.Add(new EmployeeOnStoreModels
                //{
                //    RoleID = model.RoleID,
                //    StoreID = model.StoreID
                //});

                //model.ListStoreID = model.ListRoleWorkingTime.Select(x => x.StoreID).ToList();
                model.ListRoleWorkingTime = model.ListRoleWorkingTime.Where(x => x.Status != (int)Commons.EStatus.Deleted).ToList();
                if (model.ListRoleWorkingTime != null && model.ListRoleWorkingTime.Count > 0)
                {
                    InteEmployeeOnStoreModels EmpOnStore = null;
                    foreach (var item in model.ListRoleWorkingTime)
                    {
                        //============
                        if (item.RoleID == null)
                        {
                            ModelState.AddModelError("RoleWorkingTimeAlert_" + item.StoreID, CurrentUser.GetLanguageTextFromKey("Please choose role for store")+ "[" + item.StoreName + "]");
                            break;
                        }
                        ////Role - Store
                        EmpOnStore = new InteEmployeeOnStoreModels();
                        EmpOnStore.RoleID = item.RoleID;
                        EmpOnStore.StoreID = item.StoreID;
                        EmpOnStore.IsSyncPoins = item.IsSyncPoins;
                        model.ListEmpStore.Add(EmpOnStore);

                        ////Working Time
                        for (int i = 0; i < item.ListWorkingTime.Count; i++)
                        {
                            var itemWorkingTime = item.ListWorkingTime[i];
                            itemWorkingTime.IsOffline = (itemWorkingTime.From.Equals(CurrentUser.GetLanguageTextFromKey("OFF")) 
                                || itemWorkingTime.To.Equals(CurrentUser.GetLanguageTextFromKey("OFF"))) ? true : false;
                            if (!itemWorkingTime.From.Equals(CurrentUser.GetLanguageTextFromKey("OFF")) 
                                && !itemWorkingTime.To.Equals(CurrentUser.GetLanguageTextFromKey("OFF")))
                            {
                                itemWorkingTime.FromTime = Convert.ToDateTime(itemWorkingTime.From);
                                itemWorkingTime.ToTime = Convert.ToDateTime(itemWorkingTime.To);
                                if (itemWorkingTime.FromTime.TimeOfDay > itemWorkingTime.ToTime.TimeOfDay)
                                {
                                    ModelState.AddModelError("RoleWorkingTimeAlert_" + item.StoreID, CurrentUser.GetLanguageTextFromKey("From time must be less than To time for store")+ "[" + item.StoreName + "]");
                                    break;
                                }
                            }
                            //WorkingTime
                            model.ListWorkingTime.Add(itemWorkingTime);
                        }
                    }
                }

                if (!ModelState.IsValid)
                {
                    if ((ModelState["PictureUpload"]) != null && (ModelState["PictureUpload"]).Errors.Count > 0)
                        model.ImageURL = "";
                    //model = GetDetail(model.ID, model.StoreID);

                    var lstStore = (SelectList)ViewBag.StoreID;
                    model.ListStore = lstStore.ToList();

                    if (model.ListRoleWorkingTime != null)
                    {
                        model.ListRoleWorkingTime.ForEach(x =>
                        {
                            x.GetListRole(x.StoreID, CurrentUser.ListOrganizationId);
                        });
                    }
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
                //====================
                string msg = "";
                var result = _factory.InsertOrUpdateEmployee(model, ref msg);
                if (result)
                {
                    //Save Image on Server
                    if (!string.IsNullOrEmpty(model.ImageURL) && model.PictureByte != null)
                    {
                        var originalDirectory = new DirectoryInfo(string.Format("{0}Uploads\\", Server.MapPath(@"\")));
                        var path = string.Format("{0}{1}", originalDirectory, model.ImageURL);
                        MemoryStream ms = new MemoryStream(photoByte, 0, photoByte.Length);
                        ms.Write(photoByte, 0, photoByte.Length);
                        System.Drawing.Image imageTmp = System.Drawing.Image.FromStream(ms, true);

                        ImageHelper.Me.SaveCroppedImage(imageTmp, path, model.ImageURL, ref photoByte);
                        model.PictureByte = photoByte;

                        FTP.Upload(model.ImageURL, model.PictureByte);

                        ImageHelper.Me.TryDeleteImageUpdated(path);
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    //return PartialView("_Edit", model);
                    ModelState.AddModelError("name", msg);
                    //model = GetDetail(model.ID, model.StoreID);

                    var lstStore = (SelectList)ViewBag.StoreID;
                    model.ListStore = lstStore.ToList();

                    /*===*/
                    if (model.ListRoleWorkingTime != null)
                    {
                        model.ListRoleWorkingTime.ForEach(x =>
                        {
                            x.GetListRole(x.StoreID, CurrentUser.ListOrganizationId);
                        });
                    }
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
            }
            catch (FormatException fEx)
            {
                _logger.Error("Employee_Edit: " + fEx.Message);
                ModelState.AddModelError("name", CurrentUser.GetLanguageTextFromKey(fEx.Message));
                //model = GetDetail(model.ID, model.StoreID);

                var lstStore = (SelectList)ViewBag.StoreID;
                model.ListStore = lstStore.ToList();

                /*===*/
                if (model.ListRoleWorkingTime != null)
                {
                    model.ListRoleWorkingTime.ForEach(x =>
                    {
                        x.GetListRole(x.StoreID, CurrentUser.ListOrganizationId);
                    });
                }
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Edit", model);
            }
            catch (Exception ex)
            {
                _logger.Error("Employee_Edit: " + ex);
                //return new HttpStatusCodeResult(400, ex.Message);

                ModelState.AddModelError("name", CurrentUser.GetLanguageTextFromKey("Have an error"));
                //model = GetDetail(model.ID, model.StoreID);

                var lstStore = (SelectList)ViewBag.StoreID;
                model.ListStore = lstStore.ToList();

                /*===*/
                if (model.ListRoleWorkingTime != null)
                {
                    model.ListRoleWorkingTime.ForEach(x =>
                    {
                        x.GetListRole(x.StoreID, CurrentUser.ListOrganizationId);
                    });
                }
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Edit", model);
            }
        }

        [HttpGet]
        public PartialViewResult Delete(string id, string StoreId)
        {
            InteEmployeeModels model = GetDetail(id, StoreId);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(InteEmployeeModels model)
        {
            try
            {
                string msg = "";
                var result = _factory.DeleteEmployee(model.ID, model.StoreID, ref msg);
                if (!result)
                {
                    ModelState.AddModelError("Name", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Delete", model);
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.Error("Employee_Delete: " + ex);
                ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey(ex.Message));
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        public ActionResult AddRoleWorkingTime(int currentOffset, string StoreID, string StoreName)
        {
            InteRoleWorkingTime model = new InteRoleWorkingTime();
            model.GetListRole(StoreID, CurrentUser.ListOrganizationId);
            model.OffSet = currentOffset;
            model.StoreName = StoreName;
            model.StoreID = StoreID;
            model.IsWithPoins = CurrentUser.listStore.Where(x => x.ID == StoreID).FirstOrDefault().IsWithPoins;
            model.ListWorkingTime.ForEach(x => x.StoreID = StoreID);
            return PartialView("_RoleWorkingTime", model);
        }

        public ActionResult Import()
        {
            SandboxImportModel model = new SandboxImportModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Import(SandboxImportModel model)
        {
            try
            {
                if (model.ListStores == null)
                {
                    ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Please choose store"));
                    return View(model);
                }
                //if (model.ImageZipUpload == null || model.ImageZipUpload.ContentLength <= 0)
                //{
                //    ModelState.AddModelError("ImageZipUpload", "Image Folder (.zip) cannot be null");
                //    return View(model);
                //}

                if (model.ExcelUpload == null || model.ExcelUpload.ContentLength <= 0)
                {
                    ModelState.AddModelError("ImageZipUpload", CurrentUser.GetLanguageTextFromKey("Excel filename cannot be null"));
                    return View(model);
                }
                if (model.ImageZipUpload != null)
                {
                    if (!Path.GetExtension(model.ImageZipUpload.FileName).ToLower().Equals(".zip"))
                    {
                        ModelState.AddModelError("ImageZipUpload", "");
                        return View(model);
                    }
                }
                FileInfo[] listFiles = new FileInfo[] { };
                string serverZipExtractPath = System.Web.HttpContext.Current.Server.MapPath("~/Uploads") + "/ExtractFolder";
                if (model.ImageZipUpload != null && model.ImageZipUpload.ContentLength > 0)
                {
                    bool isFolderEmpty;
                    string fileName = Path.GetFileName(model.ImageZipUpload.FileName);
                    string filePath = string.Format("{0}/{1}", System.Web.HttpContext.Current.Server.MapPath("~/Uploads"), fileName);

                    //upload file to server
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                    model.ImageZipUpload.SaveAs(filePath);

                    //extract file
                    CommonHelper.ExtractZipFile(filePath, serverZipExtractPath);

                    //delete zip file after extract
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                    isFolderEmpty = CommonHelper.IsDirectoryEmpty(serverZipExtractPath);

                    if (!isFolderEmpty)
                    {
                        string[] extensions = new[] { ".jpg", ".png", ".jpeg" };
                        DirectoryInfo dInfo = new DirectoryInfo(serverZipExtractPath);
                        //Getting Text files
                        listFiles = dInfo.EnumerateFiles()
                                 .Where(f => extensions.Contains(f.Extension.ToLower()))
                                 .ToArray();
                    }
                }

                ImportModel importModel = new ImportModel();
                string msg = "";
                // read excel file
                if (model.ExcelUpload != null && model.ExcelUpload.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(model.ExcelUpload.FileName);
                    string filePath = string.Format("{0}/{1}", System.Web.HttpContext.Current.Server.MapPath("~/Uploads"), fileName);

                    //upload file to server
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                    model.ExcelUpload.SaveAs(filePath);

                    importModel = _factory.Import(filePath, listFiles, model.ListStores, ref msg);
                    //delete folder extract after get file.
                    if (System.IO.Directory.Exists(serverZipExtractPath))
                        System.IO.Directory.Delete(serverZipExtractPath, true);
                    //delete file excel after insert to database
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                if (msg.Equals(""))
                {
                    return View("ImportDetail", importModel);
                }
                else
                {
                    _logger.Error("Employee_Import: " + msg);
                    ModelState.AddModelError("ImageZipUpload", msg);
                    return View(model);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Employee_Import: " + e);
                ModelState.AddModelError("ImageZipUpload", CurrentUser.GetLanguageTextFromKey("Import file have error."));
                return View(model);
            }
        }

        public ActionResult Export()
        {
            InteEmployeeModels model = new InteEmployeeModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Export(InteEmployeeModels model)
        {
            try
            {
                if (model.ListStores == null)
                {
                    ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Please choose store"));
                    return View(model);
                }

                XLWorkbook wb = new XLWorkbook();
                var wsSetMenu = wb.Worksheets.Add("Sheet1");
                StatusResponse response = _factory.Export(ref wsSetMenu, model.ListStores);

                if (!response.Status)
                {
                    ModelState.AddModelError("", response.MsgError);
                    return View(model);
                }

                ViewBag.wb = wb;
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Charset = UTF8Encoding.UTF8.WebName;
                Response.ContentEncoding = UTF8Encoding.UTF8;
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", CommonHelper.GetExportFileName("Employee").Replace(" ", "_")));

                using (var memoryStream = new System.IO.MemoryStream())
                {
                    wb.SaveAs(memoryStream);
                    memoryStream.WriteTo(HttpContext.Response.OutputStream);
                    memoryStream.Close();
                }
                HttpContext.Response.End();
                return RedirectToAction("Export");
            }
            catch (Exception e)
            {
                _logger.Error("Employee_Export: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
        }
    }
}