using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class UnitOfMeasureFactory : BaseFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public UnitOfMeasureFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public ResultModels InsertUnit(UnitOfMeasureModel model)
        {
            ResultModels result = new ResultModels();
            result.IsOk = true;
            using (var cxt = new NuWebContext())
            {
                //check code
                var itemDb = cxt.I_UnitOfMeasure.Where(ww => ww.Code.ToUpper() == model.Code.Trim().ToUpper()).FirstOrDefault();
                if (itemDb != null)
                {
                    result.IsOk = false;
                    result.Message = "UOM is exist!";
                    return result;
                }
                var item = new I_UnitOfMeasure();
                item.Id = Guid.NewGuid().ToString();
                item.Code = item.Code.Trim();
                item.Name = item.Name;
                item.IsActive = item.IsActive;
                item.UpdatedBy = item.UpdatedBy;
                item.UpdatedDate = DateTime.Now;
                item.CreatedBy = item.CreatedBy;
                item.CreatedDate = item.CreatedDate;
                item.Status = (int)Commons.EStatus.Actived;
                cxt.I_UnitOfMeasure.Add(item);
                cxt.SaveChanges();
            }

            return result;
        }

        public bool Insert(UnitOfMeasureModel model, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    //if (cxt.I_UnitOfMeasure.Where(xx => xx.Code == model.Code).Select(ss => ss.Code).SingleOrDefault() != null)
                    //{
                    //    msg = "UOM code already exist, please input another one!";
                    //    return false;
                    //}

                    var itemExsit = cxt.I_UnitOfMeasure.Any(x => x.Code.ToLower().Equals(model.Code.ToLower()) && x.Status != (int)Commons.EStatus.Deleted && x.OrganizationId == model.OrganizationId);
                    if (itemExsit)
                    {
                        msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("UOM Code") + " [" + model.Code + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("is duplicated");
                        return false;
                    }
                    if (string.IsNullOrEmpty(model.Name))
                        model.Name = model.Code;

                    I_UnitOfMeasure item = new I_UnitOfMeasure();

                    item.Id = Guid.NewGuid().ToString();
                    item.Code = model.Code;
                    item.Name = model.Name;
                    item.IsActive = model.IsActive;
                    item.CreatedBy = model.CreatedBy;
                    item.CreatedDate = model.CreatedDate;
                    item.UpdatedBy = model.UpdatedBy;
                    item.UpdatedDate = model.UpdatedDate;
                    item.OrganizationId = model.OrganizationId;
                    item.Description = model.Description;
                    item.Status = (int)Commons.EStatus.Actived;

                    cxt.I_UnitOfMeasure.Add(item);
                    cxt.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            return result;
        }

        public bool Update(UnitOfMeasureModel model, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var itemExsit = cxt.I_UnitOfMeasure.Where(x => x.Code.ToLower().Equals(model.Code.ToLower()) && x.Status != (int)Commons.EStatus.Deleted && x.OrganizationId == model.OrganizationId).FirstOrDefault();
                    if (itemExsit != null)
                    {
                        if (!itemExsit.Id.Equals(model.Id))
                        {
                            msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("UOM Code") + " [" + model.Code + "] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("is duplicated");
                            return false;
                        }
                    }
                    if (string.IsNullOrEmpty(model.Name))
                        model.Name = model.Code;

                    var itemUpdate = (from tb in cxt.I_UnitOfMeasure
                                      where tb.Id == model.Id
                                      select tb).FirstOrDefault();
                    itemUpdate.Code = model.Code;
                    itemUpdate.Name = model.Name;
                    itemUpdate.IsActive = model.IsActive;
                    itemUpdate.UpdatedBy = model.UpdatedBy;
                    itemUpdate.UpdatedDate = model.UpdatedDate;
                    itemUpdate.Description = model.Description;
                    cxt.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    result = false;
                }
                finally
                {
                    if (cxt != null)
                        cxt.Dispose();
                }
            }
            return result;
        }


        private bool IsCanDelete(string id)
        {
            using (var cxt = new NuWebContext())
            {
                var isExists = cxt.I_Ingredient.Any(x => x.BaseUOMId == id);
                if (!isExists)
                {
                    isExists = cxt.I_Ingredient.Any(x => x.ReceivingUOMId == id);
                    if (!isExists)
                    {
                        isExists = cxt.I_Ingredient_UOM.Any(x => (x.UOMId == id));
                        if (!isExists)
                        {
                            isExists = cxt.I_Recipe_Ingredient.Any(x => (x.UOMId == id));
                            if (!isExists)
                            {
                                isExists = cxt.I_Recipe_Item.Any(x => (x.UOMId == id));
                                if (!isExists)
                                {
                                    isExists = cxt.I_Recipe_Modifier.Any(x => (x.UOMId == id));
                                    if (!isExists)
                                    {
                                        isExists = cxt.I_Stock_Transfer_Detail.Any(x => (x.UOMId == id));
                                    }
                                }
                            }
                        }
                    }
                }
                return !isExists;
            }
        }

        public bool Delete(string Id, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    //var isExist1 = (from io in cxt.I_Ingredient_UOM
                    //                where io.UOMId.Equals(Id)
                    //                select new { io }).FirstOrDefault();
                    //var isExist2 = (from rm in cxt.I_Recipe_Modifier
                    //                where rm.UOMId.Equals(Id)
                    //                select new { rm }).FirstOrDefault();
                    //var isExist3 = (from ri in cxt.I_Recipe_Item
                    //                where ri.UOMId.Equals(Id)
                    //                select new { ri }).FirstOrDefault();
                    //var isExist4 = (from rin in cxt.I_Recipe_Ingredient
                    //                where rin.UOMId.Equals(Id)
                    //                select new { rin }).FirstOrDefault();
                    //if (isExist1 != null || isExist2 != null || isExist3 != null || isExist4 != null)
                    //{
                    //    result = false;
                    //    msg = "This UOM has been in used. Please deactivate it only.";
                    //}
                    //else
                    //{
                    //    I_UnitOfMeasure itemDelete = (from tb in cxt.I_UnitOfMeasure
                    //                                  where tb.Id == Id
                    //                                  select tb).FirstOrDefault();
                    //    //cxt.I_UnitOfMeasure.Remove(itemDelete);
                    //    itemDelete.IsActive = false;
                    //    cxt.SaveChanges();
                    //}

                    if (IsCanDelete(Id))
                    {
                        var item = cxt.I_UnitOfMeasure.Where(tb => tb.Id == Id).FirstOrDefault();
                        if (item != null)
                        {
                            item.Status = (int)Commons.EStatus.Deleted;
                            item.IsActive = false;
                            cxt.SaveChanges();
                        }
                    }
                    else
                    {
                        msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("This UOM has been in used. Please deactivate it only");
                        //msg = "This UOM has been in used. Please deactivate it only.";
                        result = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    result = false;
                }
                finally
                {
                    if (cxt != null)
                        cxt.Dispose();
                }
            }
            return result;
        }

        public List<UnitOfMeasureModel> GetData(List<string> lstMerchantIds)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from uom in cxt.I_UnitOfMeasure
                                     where (uom.Status != (int)Commons.EStatus.Deleted && uom.Status != null && lstMerchantIds.Contains(uom.OrganizationId))
                                     orderby uom.CreatedDate descending
                                     select new UnitOfMeasureModel()
                                     {
                                         Id = uom.Id,
                                         Name = uom.Name,
                                         Code = uom.Code,
                                         IsActive = uom.IsActive,
                                         CreatedDate = uom.CreatedDate,
                                         CreatedBy = uom.CreatedBy,
                                         UpdatedBy = uom.UpdatedBy,
                                         UpdatedDate = uom.UpdatedDate,
                                         OrganizationId = uom.OrganizationId,
                                         Description = uom.Description
                                     }).OrderBy(oo => oo.Name).ToList();
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public List<UnitOfMeasureModel> GetDataUOMRecipe(string IngredientId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var _UsageUOM = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Usage UOM");
                    var _BaseUOM = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Base UOM");
                    var _ReceivingUOM = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Receiving UOM");
                    var lstResult = (from uom in cxt.I_UnitOfMeasure
                                     from IUOM in cxt.I_Ingredient_UOM
                                     where (uom.Status != (int)Commons.EStatus.Deleted && uom.Status != null)
                                            && IUOM.UOMId.Equals(uom.Id)
                                            && IUOM.IsActived && IUOM.IngredientId.Equals(IngredientId)
                                     select new UnitOfMeasureModel()
                                     {
                                         Id = uom.Id,
                                         Name = uom.Name + " [" + _UsageUOM + "]",
                                     }).ToList();

                    lstResult.AddRange((from uom in cxt.I_UnitOfMeasure
                                        from I in cxt.I_Ingredient
                                        where (uom.Status != (int)Commons.EStatus.Deleted && uom.Status != null)
                                               && I.BaseUOMId.Equals(uom.Id) && I.Id.Equals(IngredientId)
                                        select new UnitOfMeasureModel()
                                        {
                                            Id = uom.Id,
                                            Name = uom.Name + " [" + _BaseUOM + "]",
                                        }).ToList());

                    lstResult.AddRange((from uom in cxt.I_UnitOfMeasure
                                        from I in cxt.I_Ingredient
                                        where (uom.Status != (int)Commons.EStatus.Deleted && uom.Status != null)
                                               && I.ReceivingUOMId.Equals(uom.Id) && I.Id.Equals(IngredientId)
                                        select new UnitOfMeasureModel()
                                        {
                                            Id = uom.Id,
                                            Name = uom.Name + " [" + _ReceivingUOM + "]",
                                        }).ToList());

                    var listIng = lstResult;
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public UnitOfMeasureModel GetDetail(string ID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var model = (from uom in cxt.I_UnitOfMeasure
                                 where uom.Id == ID
                                 select new UnitOfMeasureModel()
                                 {
                                     Id = uom.Id,
                                     Code = uom.Code,
                                     Name = uom.Name,
                                     IsActive = uom.IsActive,
                                     CreatedBy = uom.CreatedBy,
                                     CreatedDate = uom.CreatedDate,
                                     UpdatedBy = uom.UpdatedBy,
                                     UpdatedDate = uom.UpdatedDate,
                                     OrganizationId = uom.OrganizationId,
                                     Description = uom.Description
                                 }).FirstOrDefault();
                    return model;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        //public List<UnitOfMeasureModel> _GetTables()
        //{
        //    using (NuWebContext cxt = new NuWebContext())
        //    {
        //        try
        //        {
        //            var lstResult = (from uom in cxt.I_UnitOfMeasure
        //                             select new UnitOfMeasureModel()
        //                             {
        //                                 Id = uom.Id,
        //                                 Code = uom.Code,
        //                                 Name = uom.Name,
        //                                 IsActive = uom.IsActive,
        //                                 CreatedBy = uom.CreatedBy,
        //                                 CreatedDate = uom.CreatedDate,
        //                                 UpdatedBy = uom.UpdatedBy,
        //                                 UpdatedDate = uom.UpdatedDate,
        //                                 OrganizationId = uom.OrganizationId,
        //                                 Description = uom.Description
        //                             }).ToList();
        //            return lstResult;
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.Error(ex.Message);
        //            return null;
        //        }
        //    }
        //}

        public StatusResponse Import(string filePath, ref ImportModel importModel, ref string msg, UserSession User)
        {
           

            StatusResponse Response = new StatusResponse();
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    DataTable dtUOM = ReadExcelFile(filePath, "Sheet1");
                    string tmpExcelPath = HttpContext.Current.Server.MapPath("~/ImportExportTemplate") + "/SBUnitOfMeasure.xlsx";
                    DataTable dtTmpUOM = ReadExcelFile(@tmpExcelPath, "Sheet1");
                    if (dtUOM.Columns.Count != dtTmpUOM.Columns.Count)
                    {
                        msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);
                        return Response;
                    }
                    //========
                    bool flagInsert = true;
                    string msgError = "";

                    ImportItem itemErr = null;

                    List<UnitOfMeasureModel> Models = new List<UnitOfMeasureModel>();
                    UnitOfMeasureModel UOMModel = null;
                    foreach (DataRow item in dtUOM.Rows)
                    {

                        flagInsert = true;
                        msgError = "";

                        if (item[0].ToString().Equals(""))
                            continue;

                        int index = int.Parse(item[0].ToString());

                        UOMModel = new UnitOfMeasureModel();
                        UOMModel.Id = Guid.NewGuid().ToString();
                        UOMModel.Code = item[1].ToString().Trim().Replace("  ", " ");
                        UOMModel.Name = item[2].ToString().Trim().Replace("  ", " ");
                        UOMModel.Description = item[3].ToString();
                        UOMModel.IsActive = GetBoolValue(item[4].ToString());

                        UOMModel.CreatedBy = User.UserName;
                        UOMModel.CreatedDate = DateTime.Now;
                        UOMModel.UpdatedBy = User.UserName;
                        UOMModel.UpdatedDate = DateTime.Now;
                        UOMModel.OrganizationId = User.ListOrganizationId[0];
                        UOMModel.Status = (int)Commons.EStatus.Actived;

                        if (string.IsNullOrEmpty(UOMModel.Code))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("UOM Code is required");
                        }
                        if (string.IsNullOrEmpty(UOMModel.Name))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("UOM Name is required");
                        }
                        //======================
                        if (flagInsert)
                        {
                            Models.Add(UOMModel);
                        }
                        else
                        {
                            itemErr = new ImportItem();
                            itemErr.Name = UOMModel.Name;
                            itemErr.ListFailStoreName.Add("");
                            itemErr.ListErrorMsg.Add("Row: " + UOMModel.Index + msgError);
                            importModel.ListImport.Add(itemErr);
                        }
                    }
                    //=================
                    Response.Status = true;
                    try
                    {
                        var lstCodes = Models.Select(ss => ss.Code.ToLower()).ToList();
                        var lstExists = cxt.I_UnitOfMeasure.Where(ww => lstCodes.Contains(ww.Code.ToLower()) 
                        && User.ListOrganizationId.Contains(ww.OrganizationId)
                        && ww.Status != (int)Commons.EStatus.Deleted).ToList();
                        if (lstExists != null && lstExists.Count > 0)
                        {
                            lstCodes = new List<string>();
                            foreach (var item in lstExists)
                            {
                                msgError = string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("UOM Code") + " [{0}] " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("is exist") + "!", item.Code);
                                itemErr = new ImportItem();
                                itemErr.Name = item.Name;
                                itemErr.ListFailStoreName.Add("");
                                itemErr.ListErrorMsg.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ": " + msgError);
                                importModel.ListImport.Add(itemErr);

                                lstCodes.Add(item.Code);
                            }
                        }

                        //Models = Models.Where(x => !lstCodes.Contains(x.Code)).ToList();
                        if(importModel.ListImport.Count ==0)
                        {
                            List<I_UnitOfMeasure> lstSave = new List<I_UnitOfMeasure>();
                            I_UnitOfMeasure item = null;
                            foreach (var model in Models)
                            {
                                item = new I_UnitOfMeasure();
                                item.Id = model.Id;
                                item.Code = model.Code;
                                item.Name = model.Name;
                                item.Description = model.Description;
                                item.OrganizationId = model.OrganizationId;

                                item.CreatedBy = model.CreatedBy;
                                item.CreatedDate = model.CreatedDate;
                                item.UpdatedDate = model.UpdatedDate;
                                item.UpdatedBy = model.UpdatedBy;
                                item.IsActive = model.IsActive;
                                item.Status = (int)Commons.EStatus.Actived;

                                lstSave.Add(item);
                            }
                            cxt.I_UnitOfMeasure.AddRange(lstSave);
                            cxt.SaveChanges();
                            transaction.Commit();

                            //if (importModel.ListImport.Count == 0)
                            //{
                                ImportItem impItem = new ImportItem();
                                impItem.Name = "Import Unit Of Measurement Successful";
                                impItem.ListSuccessStoreName.Add("Import Unit Of Measurement Successful");
                                importModel.ListImport.Add(impItem);
                            //}
                            NSLog.Logger.Info("Import UOM Successful", lstSave);
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        //_logger.Error(ex);
                        NSLog.Logger.Error("Import UOM error", ex);
                    }
                }
            }
            return Response;
        }

        public StatusResponse Export(ref IXLWorksheet ws, List<string> lstStore, List<string> lstMerchatIds)
        {
            StatusResponse Response = new StatusResponse();
            try
            {
                List<UnitOfMeasureModel> listData = new List<UnitOfMeasureModel>();
                listData = GetData(lstMerchatIds);
                listData = listData.OrderBy(x => x.Name).ToList();
                int cols = 5;
                //Header
                int row = 1;
                ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index") /*"Index"*/;
                ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Code") /*"Code"*/;
                ws.Cell("C" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name")/*"Name"*/;
                ws.Cell("D" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Description")/*"Description"*/;
                ws.Cell("E" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Status")/*"Status"*/;
                //Item
                row = 2;
                int countIndex = 1;
                foreach (var item in listData)
                {
                    ws.Cell("A" + row).Value = countIndex;
                    ws.Cell("B" + row).Value = item.Code;
                    ws.Cell("C" + row).Value = item.Name;
                    ws.Cell("D" + row).Value = item.Description;
                    ws.Cell("E" + row).Value = item.IsActive ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active")/*"Active"*/ 
                                    : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("InActive") /*"InActive"*/;
                    row++;
                    countIndex++;
                }
                FormatExcelExport(ws, row, cols);
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

        public bool EnableActive(List<string> lstId, bool active)
        {
            //if (!active)
            //{
            //    lstId = ListIngInActive(lstId);
            //}
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var resuft = cxt.I_UnitOfMeasure.Where(ss => lstId.Contains(ss.Id)).ToList();
                    foreach (var item in resuft)
                    {
                        item.IsActive = active;
                    }
                    cxt.SaveChanges();
                }
                catch (Exception ex)
                {

                    _logger.Error(ex);
                }

            }
            return result;
        }

        private List<string> ListIngInActive(List<string> lstId)
        {
            List<string> lstIdInActive = lstId;
            using (var cxt = new NuWebContext())
            {
                var listIng = cxt.I_Ingredient.Where(aa => lstIdInActive.Contains(aa.BaseUOMId) && aa.Status != (int)Commons.EStatus.Deleted).Select(x => x.BaseUOMId).ToList();
                listIng.AddRange(cxt.I_Ingredient.Where(aa => lstIdInActive.Contains(aa.ReceivingUOMId) && aa.Status != (int)Commons.EStatus.Deleted).Select(x => x.ReceivingUOMId).ToList());
                listIng.AddRange(cxt.I_Ingredient_UOM.Where(aa => lstIdInActive.Contains(aa.UOMId)/* && aa.Status != (int)Commons.EStatus.Deleted*/).Select(x => x.UOMId).ToList());

                listIng.AddRange(cxt.I_Recipe_Ingredient.Where(aa => (lstIdInActive.Contains(aa.UOMId)) && aa.Status != (int)Commons.EStatus.Deleted).Select(x => x.UOMId).ToList());
                listIng.AddRange(cxt.I_Recipe_Item.Where(aa => lstIdInActive.Contains(aa.UOMId) && aa.Status != (int)Commons.EStatus.Deleted).Select(x => x.UOMId).ToList());
                listIng.AddRange(cxt.I_Recipe_Modifier.Where(aa => lstIdInActive.Contains(aa.UOMId) && aa.Status != (int)Commons.EStatus.Deleted).Select(x => x.UOMId).ToList());

                listIng.AddRange(cxt.I_Stock_Transfer_Detail.Where(aa => lstIdInActive.Contains(aa.UOMId) /*&& aa.Status != (int)Commons.EStatus.Deleted*/).Select(x => x.UOMId).ToList());

                //Remove
                lstIdInActive.RemoveAll(i => listIng.Contains(i));
                return lstIdInActive;
            }
        }
    }
}
