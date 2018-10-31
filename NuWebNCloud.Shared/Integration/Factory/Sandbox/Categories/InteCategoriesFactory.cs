using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Integration.Models.Sandbox.Inventory;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Shared.Models.Sandbox.Import;
// Updated 08282017
using NuWebNCloud.Shared.Models.Sandbox.Inventory;
using NuWebNCloud.Shared.Utilities;
//using ServiceStack;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace NuWebNCloud.Shared.Integration.Factory.Sandbox.Categories
{
    public class InteCategoriesFactory : BaseFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public InteCategoriesFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<InteCategoriesModels> GetListCategorOfCate(List<string> ListStoreID = null, string CategoryId = null, string ProductTypeID = null, List<string> ListOrganizationId = null, List<string> LstStoreIDOfUser = null)
        {
            List<InteCategoriesModels> listData = new List<InteCategoriesModels>();
            try
            {
                InteCategoriesApiModels paraBody = new InteCategoriesApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.ProductType = ProductTypeID;
                paraBody.ListStoreID = ListStoreID;
                paraBody.ID = CategoryId;

                paraBody.isGetChild = true;
                paraBody.ListOrgID = ListOrganizationId;
                NSLog.Logger.Info("GetListCategorOfCate Request", paraBody);
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.InteGetCategories, null, paraBody);
                NSLog.Logger.Info("GetListCategorOfCate Result", result);
                dynamic data = result.Data;
                var lstC = data["ListCategory"];
                var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                listData = JsonConvert.DeserializeObject<List<InteCategoriesModels>>(lstContent);
                //List<InteCategoriesModels> tmp = new List<InteCategoriesModels>();
                //foreach (var item in listData)
                //{
                //    var checkExist = item.ListCategoryOnStore.Where(o => LstStoreIDOfUser.Contains(o.StoreID)).ToList();
                //    if (checkExist.Count > 0)
                //        tmp.Add(item);
                //}
                //return tmp;
                if (listData != null && listData.Any())
                    listData = listData.Where(o => o.ListCategoryOnStore.Select(s => s.StoreID).ToList().Any(sID => LstStoreIDOfUser.Contains(sID))).ToList();
                return listData;

            }
            catch (Exception e)
            {
                NSLog.Logger.Error("GetListCategorOfCate Error", e);

                return listData;
            }
        }

        public List<InteCategoriesModels> GetListCategory(List<string> LstStoreID = null, string CategoryId = null, string ProductTypeID = null, List<string> ListOrganizationId = null)
        {
            List<InteCategoriesModels> listData = new List<InteCategoriesModels>();
            try
            {
                InteCategoriesApiModels paraBody = new InteCategoriesApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.ProductType = ProductTypeID;
                paraBody.ListStoreID = LstStoreID;
                paraBody.ID = CategoryId;

                paraBody.isGetChild = true;
                paraBody.ListOrgID = ListOrganizationId;

                NSLog.Logger.Info("GetListCategory Request", paraBody);
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.InteGetCategories, null, paraBody);
                NSLog.Logger.Info("GetListCategory Result", result);
                dynamic data = result.Data;
                var lstC = data["ListCategory"];
                var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                listData = JsonConvert.DeserializeObject<List<InteCategoriesModels>>(lstContent);
                return listData;
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("GetListCategory Error", e);
                return listData;
            }
        }

        // Updated 08282017
        public List<SBInventoryBaseCateGroupViewModel> GetListCategorySortParent(List<string> LstStoreID = null, string CategoryId = null, string ProductTypeID = null, List<string> ListOrganizationId = null, string cateID = "0")
        {
            List<InteCategoriesModels> listData = new List<InteCategoriesModels>();
            List<SBInventoryBaseCateGroupViewModel> lstCateGroup = new List<SBInventoryBaseCateGroupViewModel>();
            try
            {
                InteCategoriesApiModels paraBody = new InteCategoriesApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.ProductType = ProductTypeID;
                paraBody.ListStoreID = LstStoreID;
                paraBody.ID = CategoryId;

                paraBody.isGetChild = true;
                paraBody.ListOrgID = ListOrganizationId;

                NSLog.Logger.Info("GetListCategorySortParent Request", paraBody);
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.InteGetCategories, null, paraBody);
                NSLog.Logger.Info("GetListCategorySortParent Request", result);

                dynamic data = result.Data;
                var lstC = data["ListCategory"];
                var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                listData = JsonConvert.DeserializeObject<List<InteCategoriesModels>>(lstContent);

                // Sort by Sequence, Name
                List<InteCategoriesModels> parentItems = listData.Where(w => string.IsNullOrEmpty(w.ParentID)).OrderBy(o => o.Sequence).ThenBy(o => o.Name).ToList();
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
                    List<InteCategoriesModels> childItems = listData.Where(w => !string.IsNullOrEmpty(w.ParentID) && w.ParentID == parentCat.ID).OrderBy(o => o.Sequence).ThenBy(o => o.Name).ToList();
                    if (childItems.Count > 0)
                        AddChildCateInte(ref lstCateGroup, cateID, listData, childItems, levelCate);

                }

                return lstCateGroup;
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("GetListCategorySortParent Error", e);
                // _logger.Error("Categories_GetList: " + e);
                return lstCateGroup;
            }
        }

        // Updated 08282017
        private void AddChildCateInte(ref List<SBInventoryBaseCateGroupViewModel> lstCateGroup, string cateID, List<InteCategoriesModels> lstData, List<InteCategoriesModels> childItems, int currentLevelCate)
        {
            int LevelCate = currentLevelCate + 1;
            foreach (InteCategoriesModels cItem in childItems)
            {

                lstCateGroup.Add(new SBInventoryBaseCateGroupViewModel
                {
                    Id = cItem.ID,
                    Name = cItem.Name,
                    Level = "level" + LevelCate,
                    Selected = cItem.ID.Equals(cateID) ? true : false
                });
                List<InteCategoriesModels> subChilds = lstData.Where(w => !string.IsNullOrEmpty(w.ParentID) && w.ParentID == cItem.ID).OrderBy(o => o.Sequence).ThenBy(o => o.Name).ToList();
                if (subChilds.Count > 0)
                {
                    AddChildCateInte(ref lstCateGroup, cateID, lstData, subChilds, LevelCate);
                }
            }
        }

        // Updated 09222017
        // For edit category => lst category don't include any category child of current category
        public List<SBInventoryBaseCateGroupViewModel> GetListCategorySortParentForEditCate(List<string> LstStoreID = null, string CategoryId = null, string ProductTypeID = null, List<string> ListOrganizationId = null, string curCateID = "0", string curParentID = "0")
        {
            List<InteCategoriesModels> listData = new List<InteCategoriesModels>();
            List<SBInventoryBaseCateGroupViewModel> lstCateGroup = new List<SBInventoryBaseCateGroupViewModel>();
            try
            {
                InteCategoriesApiModels paraBody = new InteCategoriesApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.ProductType = ProductTypeID;
                paraBody.ListStoreID = LstStoreID;
                paraBody.ID = CategoryId;

                paraBody.isGetChild = true;
                paraBody.ListOrgID = ListOrganizationId;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.InteGetCategories, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListCategory"];
                var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                listData = JsonConvert.DeserializeObject<List<InteCategoriesModels>>(lstContent);


                // Sort by Sequence, Name
                List<InteCategoriesModels> parentItems = listData.Where(w => string.IsNullOrEmpty(w.ParentID) && !w.ID.Equals(curCateID)).OrderBy(o => o.Sequence).ThenBy(o => o.Name).ToList();
                int levelCate = 0;
                foreach (var parentCat in parentItems)
                {
                    lstCateGroup.Add(new SBInventoryBaseCateGroupViewModel
                    {
                        Id = parentCat.ID,
                        Name = parentCat.Name,
                        Level = "level" + levelCate,
                        Selected = parentCat.ID.Equals(curParentID) ? true : false
                    });

                    // No include cate child of current cate
                    List<InteCategoriesModels> childItems = listData.Where(w => !string.IsNullOrEmpty(w.ParentID) && w.ParentID == parentCat.ID && !w.ID.Equals(curCateID)).OrderBy(o => o.Sequence).ThenBy(o => o.Name).ToList();
                    if (childItems.Count > 0)
                        AddChildCateInteForEditCate(ref lstCateGroup, curCateID, listData, childItems, levelCate, curParentID);

                }
                return lstCateGroup;
            }
            catch (Exception e)
            {
                _logger.Error("Categories_GetList: " + e);
                return lstCateGroup;
            }
        }

        private void AddChildCateInteForEditCate(ref List<SBInventoryBaseCateGroupViewModel> lstCateGroup, string cateID, List<InteCategoriesModels> lstData, List<InteCategoriesModels> childItems, int currentLevelCate, string parentID)
        {
            int LevelCate = currentLevelCate + 1;
            foreach (InteCategoriesModels cItem in childItems)
            {

                lstCateGroup.Add(new SBInventoryBaseCateGroupViewModel
                {
                    Id = cItem.ID,
                    Name = cItem.Name,
                    Level = "level" + LevelCate,
                    Selected = cItem.ID.Equals(parentID) ? true : false
                });
                List<InteCategoriesModels> subChilds = lstData.Where(w => !string.IsNullOrEmpty(w.ParentID) && w.ParentID == cItem.ID && !w.ID.Equals(cateID)).OrderBy(o => o.Sequence).ThenBy(o => o.Name).ToList();
                if (subChilds.Count > 0)
                {
                    AddChildCateInteForEditCate(ref lstCateGroup, cateID, lstData, subChilds, LevelCate, parentID);
                }
            }
        }



        //public List<CategoryApplyModels> GetListCategoryForProduct(List<string> ListStoreID = null, string Type = null, List<string> ListOrganizationId = null)
        //{
        //    List<CategoryApplyModels> listData = new List<CategoryApplyModels>();
        //    try
        //    {
        //        CategoriesApiModels paraBody = new CategoriesApiModels();
        //        paraBody.StoreID = ListStoreID[0];
        //        paraBody.Type = int.Parse(Type);
        //        paraBody.isGetChild = true;
        //        var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetCategoryForProduct, null, paraBody);
        //        dynamic data = result.Data;
        //        var lstC = data["ListCategory"];
        //        var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
        //        listData = JsonConvert.DeserializeObject<List<CategoryApplyModels>>(lstContent);
        //        return listData;
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.Error("Categories_GetList: " + e);
        //        return listData;
        //    }
        //}

        public bool InsertOrUpdateCategories(InteCategoriesModels model, ref string msg)
        {
            try
            {
                InteCategoriesApiModels paraBody = new InteCategoriesApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();
                model.Type = Convert.ToInt32(model.ProductTypeID);
                paraBody.CategoryDetail = model;
                //====================
                NSLog.Logger.Info("InsertOrUpdateCategories request", paraBody);
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.InteCreateOrEditCategories, null, paraBody);
                NSLog.Logger.Info("InsertOrUpdateCategories result", result);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        //_logger.Error(result.Message);
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
                msg = e.Message;
                NSLog.Logger.Error("InsertOrUpdateCategories error", e);
                //_logger.Error("Categories_InsertOrUpdate: " + e.Message.ToString());
                return false;
            }
        }

        public bool DeleteCategories(string ID, ref string msg)
        {
            try
            {
                InteCategoriesApiModels paraBody = new InteCategoriesApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.ID = ID;

                //====================
                NSLog.Logger.Info("DeleteCategories request", paraBody);
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.InteDeleteCategories, null, paraBody);
                NSLog.Logger.Info("DeleteCategories result", result);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        msg = result.Message;
                        //_logger.Error(result.Message);
                        return false;
                    }
                }
                else
                {
                    // _logger.Error(result);
                    return false;
                }
            }
            catch (Exception e)
            {
                msg = e.Message;
                NSLog.Logger.Error("DeleteCategories error", e);
                //_logger.Error("Categories_Delete: " + e.Message.ToString());
                return false;
            }
        }
        public SetMenuImportResultModels ExtendCategories(InteCategoriesViewModels model, ref string msg)
        {
            SetMenuImportResultModels importItems = new SetMenuImportResultModels();
            try
            {
                InteCategoriesApiModels paraBody = new InteCategoriesApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();
                paraBody.ListCategoryID = model.ListItem.Where(o => o.IsSelected).Select(s => s.ID).ToList();
                paraBody.ListStoreIDExtendTo = model.StoreExtendTo;
                paraBody.StoreID = model.StoreExtendFrom;
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.CategoryExtend, null, paraBody);
                if (result != null)
                {
                    dynamic data = result.Data;
                    msg = data["Description"];
                    if (result.Success)
                    {
                        importItems.Name = "<strong style=\"color: #d9534f;\">" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg) + "<strong>";
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("Categories_Extend: " + e);
            }
            return importItems;
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
                    foreach (var item in ListCate)
                    {
                        lstData.Add(new RFilterCategoryModel
                        {
                            Id = item["Id"],
                            Name = item["Name"],
                            StoreName = item["StoreName"],
                            ParentId = item["ParentId"],

                        });
                    }

                    var lstParentOrNotChild = lstData.Where(ww => string.IsNullOrEmpty(ww.ParentId)).ToList();
                    foreach (var item in lstParentOrNotChild)
                    {
                        item.ListChilds = lstData.Where(ww => !string.IsNullOrEmpty(ww.ParentId) && ww.ParentId == item.Id).ToList();
                        lstResult.Add(item);
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


        public StatusResponse Export(ref IXLWorksheet wsCategoryMerchant, ref IXLWorksheet wsCategoryStore, List<string> ListStoreID, List<string> ListOrgID)
        {
            StatusResponse Response = new StatusResponse();
            try
            {
                List<InteCategoriesModels> listData = new List<InteCategoriesModels>();
                InteCategoriesApiModels paraBody = new InteCategoriesApiModels();
                paraBody.ListOrgID = ListOrgID;
                paraBody.ProductType = "0";
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.InteExportCategories, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListCategory"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                listData = JsonConvert.DeserializeObject<List<InteCategoriesModels>>(lstContent);

                if (ListStoreID.Count > 0)
                    listData.ForEach(x =>
                    {
                        x.ListCategoryOnStore = x.ListCategoryOnStore.Where(z => ListStoreID.Contains(z.StoreID)).ToList();
                    });

                List<ExportCategoryStore> lstCategoryStore = new List<ExportCategoryStore>();
                int row = 1;
                string[] listCategoryHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category Merchant Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Type"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Parent Category"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("GLAccount Code"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Split Sales"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Include Net Sale"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Description"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Image URL")
                };
                for (int i = 1; i <= listCategoryHeader.Length; i++)
                    wsCategoryMerchant.Cell(row, i).Value = listCategoryHeader[i - 1];
                int cols = listCategoryHeader.Length;
                //Item
                row = 2;
                int countIndex = 1;
                int countIndexCateStore = 1;
                foreach (var item in listData)
                {
                    if (item.ListCategoryOnStore.Count == 0)
                        continue;

                    string type = "";
                    if (item.Type == (byte)Commons.EProductType.Dish)
                        type = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EProductType.Dish.ToString());
                    else if (item.Type == (byte)Commons.EProductType.Modifier)
                        type = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EProductType.Modifier.ToString());
                    else if (item.Type == (byte)Commons.EProductType.SetMenu)
                        type = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EProductType.SetMenu.ToString());

                    wsCategoryMerchant.Cell("A" + row).Value = countIndex;
                    wsCategoryMerchant.Cell("B" + row).Value = item.Name;
                    wsCategoryMerchant.Cell("C" + row).Value = type;
                    wsCategoryMerchant.Cell("D" + row).Value = item.ParentName;
                    wsCategoryMerchant.Cell("E" + row).Value = item.GLAccountCode;
                    wsCategoryMerchant.Cell("F" + row).Value = item.IsGiftCard ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");

                    wsCategoryMerchant.Cell("G" + row).Value = item.IsIncludeNetSale ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");

                    wsCategoryMerchant.Cell("H" + row).Value = item.Description;
                    wsCategoryMerchant.Cell("I" + row).Value = item.ImageURL;
                    if (item.ListCategoryOnStore != null)
                    {
                        for (int i = 0; i < item.ListCategoryOnStore.Count; i++)
                        {
                            var itemCateStore = item.ListCategoryOnStore[i];
                            ExportCategoryStore eModStore = new ExportCategoryStore()
                            {
                                CategoryMerchantIndex = countIndex,
                                CategoryStoreIndex = countIndexCateStore,
                                StoreName = itemCateStore.StoreName,
                                Sequence = itemCateStore.Sequence,
                                IsShowInReservationQueue = itemCateStore.IsShowInReservation,
                                IsShowInKiosk = itemCateStore.IsShowInKiosk
                            };
                            countIndexCateStore++;
                            lstCategoryStore.Add(eModStore);
                        }
                    }
                    row++;
                    countIndex++;
                }
                FormatExcelExport(wsCategoryMerchant, row, cols);
                //=========
                row = 1;
                string[] listCategoryStoreHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category Merchant Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category Store Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sequence"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Show in Reservation & Queue"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Show In Kiosk")
                            };
                for (int i = 1; i <= listCategoryStoreHeader.Length; i++)
                    wsCategoryStore.Cell(row, i).Value = listCategoryStoreHeader[i - 1];
                cols = listCategoryStoreHeader.Length;
                row++;

                int colCategoryStore = 1;
                foreach (var item in lstCategoryStore)
                {
                    wsCategoryStore.Cell(row, colCategoryStore++).Value = item.CategoryMerchantIndex;
                    wsCategoryStore.Cell(row, colCategoryStore++).Value = item.CategoryStoreIndex;
                    wsCategoryStore.Cell(row, colCategoryStore++).Value = item.StoreName;
                    wsCategoryStore.Cell(row, colCategoryStore++).Value = item.Sequence;
                    wsCategoryStore.Cell(row, colCategoryStore++).Value = item.IsShowInReservationQueue
                        ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")
                        : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");

                    wsCategoryStore.Cell(row, colCategoryStore++).Value = item.IsShowInKiosk
                        ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")
                        : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");

                    if (colCategoryStore >= listCategoryStoreHeader.Length)
                        colCategoryStore = 1;
                    row++;
                }

                FormatExcelExport(wsCategoryStore, row, cols);
                Response.Status = true;
            }
            catch (Exception e)
            {
                Response.Status = false;
                Response.MsgError = e.Message;
            }
            finally
            {
            }
            return Response;
        }

        public List<SetMenuImportResultItem> Import(string filePath, FileInfo[] listFileInfo, out int totalRowExel, List<string> ListOrgID, ref string msg)
        {
            totalRowExel = 0;
            List<SetMenuImportResultItem> importItems = new List<SetMenuImportResultItem>();

            DataTable dtCategoryMerchant = ReadExcelFile(@filePath, "Category Merchant");
            DataTable dtCategoryStore = ReadExcelFile(@filePath, "Category Store");

            string tmpExcelPath = System.Web.HttpContext.Current.Server.MapPath("~/ImportExportTemplate") + "/SBInventoryCategoryInte.xlsx";
            DataTable dtCategoryMerchantTmp = ReadExcelFile(@tmpExcelPath, "Category Merchant");
            DataTable dtCategoryStoreTmp = ReadExcelFile(@tmpExcelPath, "Category Store");

            if (dtCategoryMerchant.Columns.Count != dtCategoryMerchantTmp.Columns.Count)
            {
                msg = Commons._MsgDoesNotMatchFileExcel;
                return importItems;
            }
            if (dtCategoryStore.Columns.Count != dtCategoryStoreTmp.Columns.Count)
            {
                msg = Commons._MsgDoesNotMatchFileExcel;
                return importItems;
            }
            List<InteCategoriesModels> listData = new List<InteCategoriesModels>();
            SetMenuImportResultItem itemErr = null;
            bool flagInsert = true;
            string msgError = "";

            foreach (DataRow item in dtCategoryMerchant.Rows)
            {
                flagInsert = true;
                msgError = "";

                if (item[0].ToString().Equals(""))
                    continue;
                int index = int.Parse(item[0].ToString());
                string ImageUrl = "";
                if (!string.IsNullOrEmpty(item[8].ToString()))
                {
                    FileInfo file = listFileInfo.FirstOrDefault(m => m.Name.ToLower() == item[8].ToString().ToLower());
                    if (file != null)
                    {
                        if (file.Length > Commons._MaxSizeFileUploadImg)
                        {
                            flagInsert = false;
                            msgError = Commons._MsgAllowedSizeImg + "<br/>";
                        }
                        else
                        {
                            ImageUrl = Guid.NewGuid() + file.Extension;
                            byte[] photoByte = null;
                            photoByte = System.IO.File.ReadAllBytes(file.FullName);
                            //19/01/2018
                            //photoByte = file.ReadFully();
                            if (!string.IsNullOrEmpty(ImageUrl) && photoByte != null)
                            {
                                var originalDirectory = new DirectoryInfo(string.Format("{0}Uploads\\", System.Web.HttpContext.Current.Server.MapPath(@"\")));
                                var path = string.Format("{0}{1}", originalDirectory, ImageUrl);
                                MemoryStream ms = new MemoryStream(photoByte, 0, photoByte.Length);
                                ms.Write(photoByte, 0, photoByte.Length);
                                System.Drawing.Image imageTmp = System.Drawing.Image.FromStream(ms, true);
                                ImageHelper.Me.SaveCroppedImage(imageTmp, path, ImageUrl, ref photoByte);
                                FTP.Upload(ImageUrl, photoByte);
                                ImageHelper.Me.TryDeleteImageUpdated(path);
                            }
                        }
                    }
                }

                List<CategoryOnStoreWebDTO> ListCategoryOnStore = new List<CategoryOnStoreWebDTO>();
                DataRow[] CategoryOnStoreRow = dtCategoryStore.Select("[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category Merchant Index") + "] = " + index + "");
                foreach (DataRow gCategoryOnStoreRow in CategoryOnStoreRow)
                {
                    if (gCategoryOnStoreRow[0].ToString().Equals(""))
                        continue;

                    int ModifierMerchantIndex = int.Parse(gCategoryOnStoreRow[0].ToString());
                    CategoryOnStoreWebDTO itemCateOnStore = new CategoryOnStoreWebDTO()
                    {
                        OffSet = int.Parse(gCategoryOnStoreRow[1].ToString()),
                        StoreName = gCategoryOnStoreRow[2].ToString(),
                        Sequence = string.IsNullOrEmpty(gCategoryOnStoreRow[3].ToString()) ? 1 : int.Parse(gCategoryOnStoreRow[3].ToString()),


                        IsShowInReservation = string.IsNullOrEmpty(gCategoryOnStoreRow[4].ToString()) ? true
                        : bool.Parse(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(gCategoryOnStoreRow[4].ToString()).Equals(
                                _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        IsShowInKiosk = string.IsNullOrEmpty(gCategoryOnStoreRow[5].ToString()) ? true
                        : bool.Parse(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(gCategoryOnStoreRow[5].ToString()).Equals(
                                _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                    };

                    //Validation
                    if (string.IsNullOrEmpty(gCategoryOnStoreRow[0].ToString()))
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category Merchant Index is required");
                    }
                    if (string.IsNullOrEmpty(gCategoryOnStoreRow[1].ToString()))
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category Store Index is required");
                    }
                    if (string.IsNullOrEmpty(itemCateOnStore.StoreName))
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store Name is required");
                    }
                    if (itemCateOnStore.Sequence < 0)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value Sequence larger or equal to 0");
                    }
                    if (ListCategoryOnStore.Count > 0)
                    {
                        var IsExist = ListCategoryOnStore.Exists(x => x.OffSet.Equals(itemCateOnStore.OffSet));
                        if (IsExist)
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category Store Index is exist");
                        }
                    }
                    if (flagInsert)
                    {
                        ListCategoryOnStore.Add(itemCateOnStore);
                    }
                }
                //========
                string type = item[2].ToString().ToLower();
                int iType = 0;
                if (type.Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EProductType.Dish.ToString().ToLower())))
                    iType = (byte)Commons.EProductType.Dish;
                else if (type.Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EProductType.Modifier.ToString().ToLower())))
                    iType = (byte)Commons.EProductType.Modifier;
                else if (type.Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EProductType.SetMenu.ToString().ToLower()))
                    || type.Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set").ToLower()))
                    iType = (byte)Commons.EProductType.SetMenu;

                string ParentName = item[3].ToString();
                if (iType != (byte)Commons.EProductType.Dish)
                    ParentName = "";

                InteCategoriesModels model = new InteCategoriesModels
                {
                    Index = index.ToString(),
                    Name = item[1].ToString(),
                    Type = iType,
                    ParentName = ParentName,

                    GLAccountCode = item[4].ToString(),

                    IsGiftCard = string.IsNullOrEmpty(item[5].ToString()) ? false
                        : bool.Parse(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(item[5].ToString()).Equals(
                                _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                    IsIncludeNetSale = string.IsNullOrEmpty(item[6].ToString()) ? false
                        : bool.Parse(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(item[6].ToString()).Equals(
                                _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                    Description = item[7].ToString(),
                    ImageURL = ImageUrl,
                    ListCategoryOnStore = ListCategoryOnStore
                };
                //============
                if (string.IsNullOrEmpty(model.Index))
                {
                    flagInsert = false;
                    msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category Merchant Index is required");
                }
                if (string.IsNullOrEmpty(model.Name))
                {
                    flagInsert = false;
                    msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category Name is required");
                }
                if (listData.Count > 0)
                {
                    var IsExist = listData.Exists(x => x.Index.Equals(model.Index));
                    if (IsExist)
                    {
                        flagInsert = false;
                        msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Category Merchant Index is exist");
                    }
                }
                //==========
                if (flagInsert)
                {
                    listData.Add(model);
                }
                else
                {
                    SetMenuErrorItem itemerr = new SetMenuErrorItem();
                    itemerr.GroupName = model.Index;
                    itemerr.ErrorMessage = "Row:" + index + msgError;
                    itemErr = new SetMenuImportResultItem();
                    itemErr.Name = model.Name;
                    itemErr.ListFailStoreName.Add("");
                    itemErr.ErrorItems.Add(itemerr);
                    importItems.Add(itemErr);
                }
            }

            InteCategoriesApiModels paraBody = new InteCategoriesApiModels();
            paraBody.CreatedUser = Commons.CreateUser;
            paraBody.ListOrgID = ListOrgID;
            paraBody.ListCategory = listData;
            paraBody.AppKey = Commons.AppKey;
            paraBody.AppSecret = Commons.AppSecret;
            //====================
            var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.InteImportCategories, null, paraBody);
            if (result != null)
            {
                dynamic data = result.Data;
                var lstC = data["ListProperty"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                var listError = JsonConvert.DeserializeObject<List<ImportResult>>(lstContent);

                SetMenuImportResultItem importItem = new SetMenuImportResultItem();
                //importItem.Name = "<strong style=\"color: #d9534f;\">" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Have been") + " [" + (listData.Count - listError.Count) + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("row(s) import Successful") + "<strong>";
                importItem.Name = "<strong style=\"color: #d9534f;\">" + "[" + (listData.Count - listError.Count) + "]" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(" row(s) have been imported successful") + "<strong>";
                importItems.Insert(0, importItem);

                foreach (ImportResult itemError in listError)
                {
                    SetMenuErrorItem item = new SetMenuErrorItem();
                    item.GroupName = itemError.Index;
                    item.ErrorMessage = "Row: " + itemError.Index + "<br/>" + itemError.Error;

                    importItem = new SetMenuImportResultItem();
                    importItem.Name = itemError.Property;
                    importItem.ListFailStoreName.Add(itemError.StoreName);
                    importItem.ErrorItems.Add(item);
                    importItems.Add(importItem);
                }

                if (importItems.Count == 0)
                {
                    importItem = new SetMenuImportResultItem();
                    importItem.Name = "Category";
                    importItem.ListSuccessStoreName.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import Category Successful"));
                    importItems.Add(importItem);
                }

                //=====
                //importItem = new SetMenuImportResultItem();
                ////importItem.Name = "<strong style=\"color: #d9534f;\">[" + (importItems.Count) + "] rows import successfully<strong>";
                //importItem.Name = "<strong style=\"color: #d9534f;\">[" + (importItems.Count)
                //    + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("rows import successfully") + "<strong>";
                //importItems.Insert(0, importItem);
                //=====End
            }
            return importItems;
        }
    }

    /*ExportCategoryStore*/
    public class ExportCategoryStore
    {
        public int CategoryMerchantIndex { get; set; }
        public int CategoryStoreIndex { get; set; }
        public string StoreName { get; set; }
        public int Sequence { get; set; }
        public bool IsShowInReservationQueue { get; set; }
        public bool IsShowInKiosk { get; set; }
    }
}
