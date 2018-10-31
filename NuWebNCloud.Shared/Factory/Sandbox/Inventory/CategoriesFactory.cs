using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Shared.Models.Sandbox.Inventory;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuWebNCloud.Shared.Factory.Sandbox.Inventory
{
    public class CategoriesFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public CategoriesFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<CategoriesModels> GetListCategory(string StoreID = null, string CategoryId = null, string ProductTypeID = null, List<string> ListOrganizationId = null)
        {
            List<CategoriesModels> listData = new List<CategoriesModels>();
            try
            {
                CategoriesApiModels paraBody = new CategoriesApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.ProductTypeID = ProductTypeID;
                paraBody.StoreID = StoreID;
                paraBody.ID = CategoryId;

                paraBody.isGetChild = true;
                paraBody.ListOrgID = ListOrganizationId;
                NSLog.Logger.Info("GetListCategory Request", paraBody);

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetCategories, null, paraBody);
                NSLog.Logger.Info("GetListCategory Result", paraBody);
                dynamic data = result.Data;
                var lstC = data["ListCategory"];
                var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                listData = JsonConvert.DeserializeObject<List<CategoriesModels>>(lstContent);
                var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                if (currentUser != null)
                    listData = listData.Where(ww => currentUser.ListStoreID.Contains(ww.StoreID)).ToList();
                return listData;
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("GetListCategory Error", e);
                return listData;
            }
        }

        // Updated 08282017
        public List<SBInventoryBaseCateGroupViewModel> GetListCategorySortParent(string StoreID = null, string CategoryId = null, string ProductTypeID = null, List<string> ListOrganizationId = null, string cateID = "0")
        {
            List<CategoriesModels> listData = new List<CategoriesModels>();
            List<SBInventoryBaseCateGroupViewModel> lstCateGroup = new List<SBInventoryBaseCateGroupViewModel>();
            try
            {
                CategoriesApiModels paraBody = new CategoriesApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.ProductTypeID = ProductTypeID;
                paraBody.StoreID = StoreID;
                paraBody.ID = CategoryId;

                paraBody.isGetChild = true;
                paraBody.ListOrgID = ListOrganizationId;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetCategories, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListCategory"];
                var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                listData = JsonConvert.DeserializeObject<List<CategoriesModels>>(lstContent);
                var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                if (currentUser != null)
                    listData = listData.Where(ww => currentUser.ListStoreID.Contains(ww.StoreID)).ToList();

                // Sort by Sequence, Name
                List<CategoriesModels> parentItems = listData.Where(w => string.IsNullOrEmpty(w.ParentID)).OrderBy(o => o.Sequence).ThenBy(o => o.Name).ToList();
                int levelCate = 0;
                foreach (var parentCat in parentItems)
                {
                    lstCateGroup.Add(new SBInventoryBaseCateGroupViewModel
                    {
                        Id = parentCat.ID,
                        Name = parentCat.Name,
                        Level = "level" + levelCate,
                        Selected = parentCat.ID.Equals(cateID) ? true : false
                    });
                    List<CategoriesModels> childItems = listData.Where(w => !string.IsNullOrEmpty(w.ParentID) && w.ParentID == parentCat.ID).OrderBy(o => o.Sequence).ThenBy(o => o.Name).ToList();
                    if (childItems.Count > 0)
                        AddChildCate(ref lstCateGroup, cateID, listData, childItems, levelCate);

                }

                return lstCateGroup;
            }
            catch (Exception e)
            {
                _logger.Error("Categories_GetList: " + e);
                return lstCateGroup;
            }
        }
        // Updated 08282017
        private void AddChildCate(ref List<SBInventoryBaseCateGroupViewModel> lstCateGroup, string cateID, List<CategoriesModels> lstData, List<CategoriesModels> childItems, int currentLevelCate)
        {
            int LevelCate = currentLevelCate + 1;
            foreach (CategoriesModels cItem in childItems)
            {

                lstCateGroup.Add(new SBInventoryBaseCateGroupViewModel
                {
                    Id = cItem.ID,
                    Name = cItem.Name,
                    Level = "level" + LevelCate,
                    Selected = cItem.ID.Equals(cateID) ? true : false
                });
                List<CategoriesModels> subChilds = lstData.Where(w => !string.IsNullOrEmpty(w.ParentID) && w.ParentID == cItem.ID).OrderBy(o => o.Sequence).ThenBy(o => o.Name).ToList();
                if (subChilds.Count > 0)
                {
                    AddChildCate(ref lstCateGroup, cateID, lstData, subChilds, LevelCate);
                }
            }
        }

        // Updated 09082017
        public List<SBInventoryBaseCateGroupViewModel> GetListCategorySortParentForEditCate(string StoreID = null, string CategoryId = null, string ProductTypeID = null, List<string> ListOrganizationId = null, string cateID = "0", string parentID = "0")
        {
            List<CategoriesModels> listData = new List<CategoriesModels>();
            List<SBInventoryBaseCateGroupViewModel> lstCateGroup = new List<SBInventoryBaseCateGroupViewModel>();
            try
            {
                CategoriesApiModels paraBody = new CategoriesApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.ProductTypeID = ProductTypeID;
                paraBody.StoreID = StoreID;
                paraBody.ID = CategoryId;

                paraBody.isGetChild = true;
                paraBody.ListOrgID = ListOrganizationId;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetCategories, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListCategory"];
                var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                listData = JsonConvert.DeserializeObject<List<CategoriesModels>>(lstContent);
                var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                if (currentUser != null)
                    listData = listData.Where(ww => currentUser.ListStoreID.Contains(ww.StoreID)).ToList();

                // Sort by Sequence, Name
                List<CategoriesModels> parentItems = listData.Where(w => string.IsNullOrEmpty(w.ParentID) && !w.ID.Equals(cateID)).OrderBy(o => o.Sequence).ThenBy(o => o.Name).ToList();
                int levelCate = 0;
                foreach (var parentCat in parentItems)
                {
                    lstCateGroup.Add(new SBInventoryBaseCateGroupViewModel
                    {
                        Id = parentCat.ID,
                        Name = parentCat.Name,
                        Level = "level" + levelCate,
                        Selected = parentCat.ID.Equals(parentID) ? true : false
                    });

                    // No include cate child of current cate
                    List<CategoriesModels> childItems = listData.Where(w => !string.IsNullOrEmpty(w.ParentID) && w.ParentID == parentCat.ID && !w.ID.Equals(cateID)).OrderBy(o => o.Sequence).ThenBy(o => o.Name).ToList();
                    if (childItems.Count > 0)
                        AddChildCateForEditCate(ref lstCateGroup, cateID, listData, childItems, levelCate, parentID);

                }

                return lstCateGroup;
            }
            catch (Exception e)
            {
                _logger.Error("Categories_GetList: " + e);
                return lstCateGroup;
            }
        }

        private void AddChildCateForEditCate(ref List<SBInventoryBaseCateGroupViewModel> lstCateGroup, string cateID, List<CategoriesModels> lstData, List<CategoriesModels> childItems, int currentLevelCate, string parentID)
        {
            int LevelCate = currentLevelCate + 1;
            foreach (CategoriesModels cItem in childItems)
            {

                lstCateGroup.Add(new SBInventoryBaseCateGroupViewModel
                {
                    Id = cItem.ID,
                    Name = cItem.Name,
                    Level = "level" + LevelCate,
                    Selected = cItem.ID.Equals(parentID) ? true : false
                });
                List<CategoriesModels> subChilds = lstData.Where(w => !string.IsNullOrEmpty(w.ParentID) && w.ParentID == cItem.ID && !w.ID.Equals(cateID)).OrderBy(o => o.Sequence).ThenBy(o => o.Name).ToList();
                if (subChilds.Count > 0)
                {
                    AddChildCateForEditCate(ref lstCateGroup, cateID, lstData, subChilds, LevelCate, parentID);
                }
            }
        }

        public List<CategoriesModels> GetListCategoryInte(string StoreID = null, string CategoryId = null, string ProductTypeID = null, List<string> ListOrganizationId = null)
        {
            List<CategoriesModels> listData = new List<CategoriesModels>();
            try
            {
                CategoriesApiModels paraBody = new CategoriesApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.ProductTypeID = ProductTypeID;
                paraBody.StoreID = StoreID;
                paraBody.ID = CategoryId;

                paraBody.isGetChild = true;
                paraBody.ListOrgID = ListOrganizationId;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetCategories, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListCategory"];
                var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                listData = JsonConvert.DeserializeObject<List<CategoriesModels>>(lstContent);
                return listData;
            }
            catch (Exception e)
            {
                _logger.Error("Categories_GetList: " + e);
                return listData;
            }
        }

        public bool InsertOrUpdateCategories(CategoriesModels model, ref string msg)
        {
            try
            {
                CategoriesApiModels paraBody = new CategoriesApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.ID = model.ID;
                paraBody.StoreID = model.StoreID;

                paraBody.Name = model.Name;
                paraBody.Sequence = model.Sequence;
                paraBody.Description = model.Description;
                paraBody.TotalProducts = model.TotalProducts;
                paraBody.ImageURL = model.ImageURL;
                paraBody.ParentID = model.ParentID;
                paraBody.ProductTypeID = model.ProductTypeID;
                paraBody.Type = model.Type;
                paraBody.IsShowInReservation = model.IsShowInReservation;
                paraBody.IsShowInKiosk = model.IsShowInKiosk;
                paraBody.IsGiftCard = model.IsGiftCard;
                paraBody.IsIncludeNetSale = model.IsIncludeNetSale;
                paraBody.GLAccountCode = model.GLAccountCode;

                //====================
                NSLog.Logger.Info("InsertOrUpdateCategories Request", paraBody);
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.CreateOrEditCategories, null, paraBody);
                NSLog.Logger.Info("InsertOrUpdateCategories Result", result);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        msg = result.Message;
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("InsertOrUpdateCategories Error", e);
                return false;
            }
        }

        public bool DeleteCategories(string ID, ref string msg)
        {
            try
            {
                CategoriesApiModels paraBody = new CategoriesApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.ID = ID;

                NSLog.Logger.Info("DeleteCategories Request", paraBody);
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.DeleteCategories, null, paraBody);
                NSLog.Logger.Info("DeleteCategories Result", result);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        msg = result.Message;
                      
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                msg = e.ToString();
                NSLog.Logger.Error("DeleteCategories Error", e);
                return false;
            }
        }

        public List<RFilterCategoryModel> GetAllCategoriesForReport(CategoryApiRequestModel request)
        {
            var lstData = new List<RFilterCategoryModel>();
            var lstResult = new List<RFilterCategoryModel>();
            try
            {

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetCategoryReport, null, request);
                if (result.Success)
                {
                    dynamic data = result.Data;
                    var ListCate = data["ListCategories"];
                    RFilterCategoryModel obj = null;
                    foreach (var item in ListCate)
                    {
                        obj = new RFilterCategoryModel();
                        obj.Id = item["Id"];
                        obj.Name = item["Name"];
                        obj.StoreId = item["StoreId"];
                        obj.StoreName = item["StoreName"];
                        obj.ParentId = item["ParentId"];
                        if (CommonHelper.IsPropertyExist(data, "Seq"))
                            obj.Seq = item["Seq"];

                        lstData.Add(obj);
                        //lstData.Add(new RFilterCategoryModel
                        //{
                        //    Id = item["Id"],
                        //    Name = item["Name"],
                        //    StoreId = item["StoreId"],
                        //    StoreName = item["StoreName"],
                        //    ParentId = item["ParentId"],
                        //    Seq = item["Seq"],
                        //});
                    }

                    var lstParentOrNotChild = lstData.Where(ww => string.IsNullOrEmpty(ww.ParentId)).OrderBy(ww => ww.Seq).ThenBy(aa => aa.Name).ToList();
                    var lstChilds = lstData.Where(ww => !string.IsNullOrEmpty(ww.ParentId)).ToList();
                    foreach (var item in lstParentOrNotChild)
                    {
                        lstResult.Add(GetCategoryModel(item, lstChilds));
                    }

                }
                lstResult = lstResult.OrderBy(oo => oo.StoreName).ToList();
                return lstResult;
            }
            catch (Exception e)
            {
                _logger.Error("GetCate Report: " + e);
                return lstData;
            }
        }
        private RFilterCategoryModel GetCategoryModel(RFilterCategoryModel item, List<RFilterCategoryModel> listFull, int count = 0)
        {
            count++;
            var lstGrandChilds = listFull.Where(ww => ww.ParentId == item.Id && ww.StoreId == item.StoreId).OrderBy(ww => ww.Seq).ToList();
            //if (lstGrandChilds.Count > 0 && count <= 3)
            if (lstGrandChilds.Count > 0 )
            {
                foreach (var child in lstGrandChilds)
                    item.ListChilds.Add(GetCategoryModel(child, listFull, count));
            }
            return item;
        }

        public void GetCategoryCheck(ref List<RFilterCategoryModel> lstReturns, List<RFilterCategoryModel> listChild)
        {
            var lstGrandChilds = listChild.Where(ww => ww.Checked).ToList();
            if (lstGrandChilds.Count > 0)
            {
                lstReturns.AddRange(lstGrandChilds);
                foreach (var child in lstGrandChilds)
                    GetCategoryCheck(ref lstReturns, child.ListChilds);
            }
        }

        // Get all parent & child categories selected, updated 03212018
        public void GetCategoryCheck_V1(ref List<RFilterCategoryModel> lstReturns, List<RFilterCategoryModel> lstCategories, Boolean isCheckAll = false)
        {
            foreach (var parent in lstCategories)
            {
                if (isCheckAll || parent.Checked)
                {
                    lstReturns.Add(parent);
                }
                if (parent.ListChilds != null && parent.ListChilds.Any())
                {
                    GetCategoryCheck_V1(ref lstReturns, parent.ListChilds, isCheckAll);
                }
            }
        }

        // Updated 09202017
        public List<RFilterCategoryV1Model> GetAllCategoriesForReport_V1(CategoryApiRequestModel request)
        {
            List<RFilterCategoryV1Model> lstData = new List<RFilterCategoryV1Model>();
            List<RFilterCategoryV1Model> lstCateGroup = new List<RFilterCategoryV1Model>();
            try
            {
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetCategoryReport, null, request);
                if (result.Success)
                {
                    dynamic data = result.Data;
                    var ListCate = data["ListCategories"];

                    var lstContent = JsonConvert.SerializeObject(ListCate);
                    lstData = JsonConvert.DeserializeObject<List<RFilterCategoryV1Model>>(lstContent);

                    // Sort by Sequence, Name
                    List<RFilterCategoryV1Model> parentItems = lstData.Where(w => string.IsNullOrEmpty(w.ParentId)).OrderBy(o => o.Seq).ThenBy(o => o.Name).ToList();
                    int levelCate = 0;
                    foreach (var parentCat in parentItems)
                    {
                        lstCateGroup.Add(new RFilterCategoryV1Model
                        {
                            Id = parentCat.Id,
                            Name = parentCat.Name,
                            Level = "level" + levelCate,
                            Seq = parentCat.Seq,
                            StoreId = parentCat.StoreId,
                            StoreName = parentCat.StoreName,
                            ParentId = parentCat.ParentId

                        });
                        List<RFilterCategoryV1Model> childItems = lstData.Where(w => !string.IsNullOrEmpty(w.ParentId) && w.ParentId == parentCat.Id && w.StoreId == parentCat.StoreId).OrderBy(o => o.Seq).ThenBy(o => o.Name).ToList();
                        if (childItems.Count > 0)
                            AddChildCateForReport_V1(ref lstCateGroup, lstData, childItems, levelCate);
                    }

                }
                lstCateGroup = lstCateGroup.OrderBy(oo => oo.StoreName).ToList();
                return lstCateGroup;
            }
            catch (Exception e)
            {
                _logger.Error("GetCate Report: " + e);
                return lstCateGroup;
            }
        }
        private void AddChildCateForReport_V1(ref List<RFilterCategoryV1Model> lstCateGroup, List<RFilterCategoryV1Model> lstData, List<RFilterCategoryV1Model> childItems, int currentLevelCate)
        {
            int levelCate = currentLevelCate + 1;
            foreach (RFilterCategoryV1Model cItem in childItems)
            {
                lstCateGroup.Add(new RFilterCategoryV1Model
                {
                    Id = cItem.Id,
                    Name = cItem.Name,
                    Level = "level" + levelCate,
                    Seq = cItem.Seq,
                    StoreId = cItem.StoreId,
                    StoreName = cItem.StoreName,
                    ParentId = cItem.ParentId

                });
                List<RFilterCategoryV1Model> subChilds = lstData.Where(w => !string.IsNullOrEmpty(w.ParentId) && w.ParentId == cItem.Id && w.StoreId == cItem.StoreId).OrderBy(o => o.Seq).ThenBy(o => o.Name).ToList();
                if (subChilds.Count > 0)
                {
                    AddChildCateForReport_V1(ref lstCateGroup, lstData, subChilds, levelCate);
                }
            }
        }


        #region For Extend merchant
        public List<RFilterCategoryV1Model> GetAllCategoriesForReport_V1ForExtendMerchant(List<CategoryApiRequestModel> lstRequest)
        {
            List<RFilterCategoryV1Model> lstData = new List<RFilterCategoryV1Model>();
            List<RFilterCategoryV1Model> lstCateGroup = new List<RFilterCategoryV1Model>();
            try
            {
                foreach (var request in lstRequest)
                {
                    var result = (ResponseApiModels)ApiResponse.PostWithoutHostConfig<ResponseApiModels>(request.HostUrl + "/" + Commons.GetCategoryReport, null, request);
                    if (result.Success)
                    {
                        dynamic data = result.Data;
                        var ListCate = data["ListCategories"];

                        var lstContent = JsonConvert.SerializeObject(ListCate);
                        lstData = JsonConvert.DeserializeObject<List<RFilterCategoryV1Model>>(lstContent);

                        // Sort by Sequence, Name
                        List<RFilterCategoryV1Model> parentItems = lstData.Where(w => string.IsNullOrEmpty(w.ParentId)).OrderBy(o => o.Seq).ThenBy(o => o.Name).ToList();
                        int levelCate = 0;
                        foreach (var parentCat in parentItems)
                        {
                            lstCateGroup.Add(new RFilterCategoryV1Model
                            {
                                Id = parentCat.Id,
                                Name = parentCat.Name,
                                Level = "level" + levelCate,
                                Seq = parentCat.Seq,
                                StoreId = parentCat.StoreId,
                                StoreName = parentCat.StoreName,
                                ParentId = parentCat.ParentId

                            });
                            List<RFilterCategoryV1Model> childItems = lstData.Where(w => !string.IsNullOrEmpty(w.ParentId) && w.ParentId == parentCat.Id).OrderBy(o => o.Seq).ThenBy(o => o.Name).ToList();
                            if (childItems.Count > 0)
                                AddChildCateForReport_V1(ref lstCateGroup, lstData, childItems, levelCate);
                        }

                    }
                }
                lstCateGroup = lstCateGroup.OrderBy(oo => oo.StoreName).ToList();
                return lstCateGroup;
            }
            catch (Exception e)
            {
                _logger.Error("GetCate Report: " + e);
                return lstCateGroup;
            }
        }
        #endregion End for extend merchant

        #region For daily sale report
        public List<RFilterCategoryV1Model> GetAllCategoriesForDailySale(CategoryApiRequestModel request)
        {
            List<RFilterCategoryV1Model> lstCateHaveGlCode = new List<RFilterCategoryV1Model>();
            List<RFilterCategoryV1Model> lstData = new List<RFilterCategoryV1Model>();
            try
            {
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetCategoryReport, null, request);
                if (result.Success)
                {
                    dynamic data = result.Data;
                    var ListCate = data["ListCategories"];

                    var lstContent = JsonConvert.SerializeObject(ListCate);
                    lstCateHaveGlCode = JsonConvert.DeserializeObject<List<RFilterCategoryV1Model>>(lstContent);
                    lstCateHaveGlCode = lstCateHaveGlCode.Where(w => !string.IsNullOrEmpty(w.GLCode)).ToList();
                    if (lstCateHaveGlCode != null && lstCateHaveGlCode.Any())
                    {
                        lstCateHaveGlCode = lstCateHaveGlCode.OrderBy(o => o.Seq).ThenBy(o => o.Name).ToList();
                    }
                }

            }
            catch (Exception e)
            {
                //_logger.Error("GetCate Report: " + e);
                NSLog.Logger.Error("GetCates for daily Report:", e);

            }
            return lstCateHaveGlCode;
        }
        #endregion End for daily sale report
    }
}
