using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class WorkOrderFactory : BaseFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        private InventoryFactory _inventoryFactory = null;

        public WorkOrderFactory()
        {
            _baseFactory = new BaseFactory();
            _inventoryFactory = new InventoryFactory();
        }

        public bool Insert(WorkOrderModels model, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        I_Work_Order item = new I_Work_Order();
                        item.Id = Guid.NewGuid().ToString();

                        item.StoreId = model.StoreId;
                        item.Code = CommonHelper.GetGenNo(Commons.ETableZipCode.WorkOrder, model.StoreId);
                        item.WODate = model.WODate;
                        item.DateCompleted = model.DateCompleted;
                        item.Total = model.Total;
                        item.Status = model.Status;
                        item.Note = model.Note;

                        item.CreatedBy = model.CreatedBy;
                        item.CreatedDate = model.CreatedDate;
                        item.ModifierBy = model.ModifierBy;
                        item.ModifierDate = model.ModifierDate;
                        item.IsActived = model.IsActived;
                        //=================
                        List<I_Work_Order_Detail> ListInsert = new List<I_Work_Order_Detail>();
                        foreach (var WODetail in model.ListItem)
                        {
                            ListInsert.Add(new I_Work_Order_Detail
                            {
                                Id = Guid.NewGuid().ToString(),
                                WorkOrderId = item.Id,
                                IngredientId = WODetail.IngredientId,
                                Qty = WODetail.Qty,

                                UnitPrice = WODetail.UnitPrice,
                                Amount = WODetail.Qty * WODetail.UnitPrice,
                                ReceiptNoteQty = 0,
                                ReturnReceiptNoteQty = 0,

                                BaseQty = WODetail.Qty * (WODetail.IngReceivingQty == 0 ? 1 : WODetail.IngReceivingQty),
                                Status = (int)Commons.EStatus.Actived
                            });
                            //lstInventory.Add(new InventoryModels()
                            //{
                            //    StoreId = model.StoreID,
                            //    IngredientId = WODetail.IngredientId,
                            //    Price = WODetail.UnitPrice,
                            //    Quantity = WODetail.Qty * (WODetail.IngReceivingQty == 0 ? 1 : WODetail.IngReceivingQty)
                            //});
                        }

                        cxt.I_Work_Order.Add(item);
                        cxt.I_Work_Order_Detail.AddRange(ListInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        //Update inventory
                        //var isUpdateInentoryPO = _inventoryFactory.UpdateInventoryForPO(lstInventory);
                        //_logger.Info(string.Format("UpdateInventoryForPO: [{0}]", isUpdateInentoryPO));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        result = false;
                        transaction.Rollback();
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                    }
                }
            }
            return result;
        }

        public bool Update(WorkOrderModels model, List<string> listIdDelete, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        var itemUpdate = (from tb in cxt.I_Work_Order
                                          where tb.Id == model.Id
                                          select tb).FirstOrDefault();

                        itemUpdate.StoreId = model.StoreId;

                        itemUpdate.WODate = model.WODate;
                        itemUpdate.DateCompleted = model.DateCompleted;

                        itemUpdate.Total = model.Total;
                        itemUpdate.Note = model.Note;

                        itemUpdate.ModifierBy = model.ModifierBy;
                        itemUpdate.ModifierDate = model.ModifierDate;

                        //=================
                        //For inventory
                        //List<InventoryModels> lstInventory = new List<InventoryModels>();
                        //InventoryModels inventory = null;

                        if (model.ListItem != null && model.ListItem.Count > 0)
                        {
                            /*Delete*/
                            List<I_Work_Order_Detail> listDelete = (from tb in cxt.I_Work_Order_Detail
                                                                    where listIdDelete.Contains(tb.Id)
                                                                    select tb).ToList();
                            if (listDelete != null && listDelete.Count > 0)
                            {
                                foreach (var item in listDelete)
                                {
                                    item.Status = (int)Commons.EStatus.Deleted;
                                    //inventory
                                    //inventory = new InventoryModels();
                                    //inventory.StoreId = model.StoreID;
                                    //inventory.IngredientId = item.IngredientId;
                                    //inventory.Price = item.UnitPrice;
                                    //inventory.Quantity = -(item.BaseQty.HasValue ? item.BaseQty.Value : 0);

                                    //lstInventory.Add(inventory);
                                }
                            }

                            //Update and Insert
                            List<I_Work_Order_Detail> listInsert = new List<I_Work_Order_Detail>();
                            foreach (var WODetail in model.ListItem)
                            {
                                //inventory = new InventoryModels();
                                //inventory.StoreId = model.StoreID;
                                //inventory.IngredientId = WODetail.IngredientId;
                                //inventory.Price = WODetail.UnitPrice;

                                var itemUpdateWOD = (from tb in cxt.I_Work_Order_Detail
                                                     where tb.Id.Equals(WODetail.Id)
                                                     select tb).FirstOrDefault();
                                if (itemUpdateWOD != null) //Update
                                {
                                    //inventory.Quantity = ((WODetail.Qty - itemUpdateWOD.Qty) * (WODetail.IngReceivingQty == 0 ? 1 : WODetail.IngReceivingQty));

                                    itemUpdateWOD.Qty = WODetail.Qty;
                                    itemUpdateWOD.UnitPrice = WODetail.UnitPrice;
                                    itemUpdateWOD.Amount = WODetail.Qty * WODetail.UnitPrice;
                                    itemUpdateWOD.BaseQty = WODetail.Qty * (WODetail.IngReceivingQty == 0 ? 1 : WODetail.IngReceivingQty);
                                }
                                else //Insert
                                {
                                    itemUpdateWOD = new I_Work_Order_Detail()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        WorkOrderId = itemUpdate.Id,
                                        IngredientId = WODetail.IngredientId,
                                        Qty = WODetail.Qty,
                                        UnitPrice = WODetail.UnitPrice,
                                        Amount = WODetail.Qty * WODetail.UnitPrice,
                                        ReceiptNoteQty = 0,
                                        ReturnReceiptNoteQty = 0,
                                        BaseQty = WODetail.Qty * (WODetail.IngReceivingQty == 0 ? 1 : WODetail.IngReceivingQty),
                                        Status = (int)Commons.EStatus.Actived
                                    };
                                    listInsert.Add(itemUpdateWOD);
                                    //inventory.Quantity = WODetail.Qty * (WODetail.IngReceivingQty == 0 ? 1 : WODetail.IngReceivingQty);
                                }
                                //lstInventory.Add(inventory);
                            }
                            if (listInsert.Count > 0)
                            {
                                cxt.I_Work_Order_Detail.AddRange(listInsert);
                            }
                        }
                        cxt.SaveChanges();
                        transaction.Commit();
                        //Update inventory
                        //_inventoryFactory.UpdateInventoryForPO(lstInventory);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        result = false;
                        transaction.Rollback();
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                    }
                }
            }
            return result;
        }

        public bool Delete(string Id, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    //if (IsCanDelete(Id))
                    {
                        I_Work_Order itemDelete = (from tb in cxt.I_Work_Order
                                                   where tb.Id == Id
                                                   select tb).FirstOrDefault();
                        if (itemDelete != null)
                        {
                            itemDelete.Status = (int)Commons.EStatus.Deleted;

                            var listWOD = (from wod in cxt.I_Work_Order_Detail
                                           where wod.WorkOrderId.Equals(itemDelete.Id)
                                           select wod).ToList();
                            listWOD.ForEach(x => x.Status = (int)Commons.EStatus.Deleted);

                            cxt.SaveChanges();
                        }
                    }
                    //else
                    //{
                    //    msg = "This Work Order has been in used. Can't Delete";
                    //    result = false;
                    //}
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

        //private bool IsCanDelete(string id)
        //{
        //    using (var cxt = new NuWebContext())
        //    {
        //        var isExists = cxt.I_Receipt_Purchase_Order.Any(x => x.PurchaseOrderId == id && x.IsActived);
        //        return !isExists;
        //    }
        //}

        public List<WorkOrderModels> GetData(string StoreId,  List<string> listStoreIds, DateTime? dFrom, DateTime? dTo)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var query = (from tb in cxt.I_Work_Order
                                 where tb.Status != (int)Commons.EStatus.Deleted
                                 select new { tb });

                    DateTime currentDate = DateTime.Now;
                    if (listStoreIds != null && listStoreIds.Any())
                    {
                        query = query.Where(x => listStoreIds.Contains(x.tb.StoreId));
                    }
                    if (StoreId != null)
                    {
                        query = query.Where(x => x.tb.StoreId.Equals(StoreId));
                    }
                   
                    if (dFrom.HasValue)
                    {
                        dFrom = new DateTime(dFrom.Value.Year, dFrom.Value.Month, dFrom.Value.Day, 0, 0, 0);
                        query = query.Where(x => x.tb.WODate >= dFrom.Value);
                    }
                    if (dTo.HasValue)
                    {
                        dTo = new DateTime(dTo.Value.Year, dTo.Value.Month, dTo.Value.Day, 23, 59, 59);
                        query = query.Where(x => x.tb.WODate <= dTo.Value);
                    }

                    var lstResult = query.Select(ss => new WorkOrderModels()
                    {
                        Id = ss.tb.Id,
                        WONumber = ss.tb.Code,
                        DateCompleted = ss.tb.DateCompleted,
                        WODate = ss.tb.WODate,
                        Total = ss.tb.Total,
                        Status = ss.tb.Status,
                        StoreId = ss.tb.StoreId,
                        ColorAlert = "",
                        Symbol = ""
                    }).ToList();

                    lstResult.ForEach(x =>
                    {
                        if (x.Status == (int)Commons.EPOStatus.Open || x.Status == (int)Commons.EPOStatus.Approved)
                        {
                            if ((currentDate.Date - x.DateCompleted.Date).Days >= 1)
                            {
                                x.ColorAlert = "red";
                            }
                            else if (currentDate.Date >= x.DateCompleted.Date.AddDays(-3) || currentDate.Date <= x.DateCompleted.Date)
                            {
                                x.ColorAlert = "yellow";
                            }
                        }
                    });
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public WorkOrderModels GetDetail(string ID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var model = (from tb in cxt.I_Work_Order
                                 where tb.Id.Equals(ID)
                                 select new WorkOrderModels()
                                 {
                                     Id = tb.Id,
                                     WONumber = tb.Code,
                                     Note = tb.Note,
                                     WODate = tb.WODate,
                                     DateCompleted = tb.DateCompleted,
                                     Total = tb.Total,
                                     Status = tb.Status,
                                     CreatedBy = tb.CreatedBy,
                                     CreatedDate = tb.CreatedDate,
                                     ModifierBy = tb.ModifierBy,
                                     ModifierDate = tb.ModifierDate,
                                     IsActived = tb.IsActived,
                                     StoreId = tb.StoreId
                                 }).FirstOrDefault();
                    var index = 0;
                    model.ListItem = (from WOD in cxt.I_Work_Order_Detail
                                      from uom in cxt.I_UnitOfMeasure
                                      from I in cxt.I_Ingredient
                                      where WOD.IngredientId.Equals(I.Id) && WOD.WorkOrderId.Equals(ID)
                                      && (WOD.Status != (int)Commons.EStatus.Deleted && WOD.Status != null)
                                      && uom.IsActive && I.ReceivingUOMId.Equals(uom.Id)
                                      select new WOIngredient()
                                      {
                                          Id = WOD.Id, //Update
                                          OffSet = index + 1,
                                          BaseUOM = uom.Name,
                                          Description = I.Description,
                                          IngredientCode = I.Code,
                                          IngredientId = WOD.IngredientId,
                                          IngredientName = I.Name,
                                          Qty = WOD.Qty,
                                          UnitPrice = WOD.UnitPrice.Value,
                                          IngReceivingQty = WOD.BaseQty.HasValue ? (WOD.BaseQty.Value / WOD.Qty) : 1
                                      }).ToList();
                    return model;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public bool ChangeStatus(List<string> lstId, int status)
        {
            using (var cxt = new NuWebContext())
            {
                var lstObj = cxt.I_Work_Order.Where(ww => lstId.Contains(ww.Id)).ToList();
                if (lstObj != null && lstObj.Count > 0)
                {
                    lstObj.ForEach(ss => ss.Status = status);
                    cxt.SaveChanges();

                    //Approved
                    if (status == (int)Commons.EPOStatus.Approved)
                    {
                        List<InventoryModels> _lstWODetail = new List<InventoryModels>();
                        foreach (var item in lstObj)
                        {
                            //var storeId = lstObj.Select(ss => ss.StoreId).FirstOrDefault();
                            //update inventory
                            var lstWODetail = cxt.I_Work_Order_Detail.Where(ww => ww.WorkOrderId == item.Id).Select(ss => new InventoryModels
                            {
                                StoreId = item.StoreId,
                                IngredientId = ss.IngredientId,
                                Price = ss.UnitPrice.Value,
                                Quantity = ss.BaseQty.HasValue ? ss.BaseQty.Value : 0
                            }).ToList();

                            _lstWODetail.AddRange(lstWODetail);
                        }
                        //Update inventory
                        var isUpdateInentoryWO = _inventoryFactory.UpdateInventoryForWO(_lstWODetail);
                        _logger.Info(string.Format("UpdateInventoryForWO: [{0}] when approved", isUpdateInentoryWO));
                    }
                    else //close
                    {
                        List<InventoryModels> _lstWODetail = new List<InventoryModels>();
                        foreach (var item in lstObj)
                        {
                            var lstWODetail = cxt.I_Work_Order_Detail.Where(ww => ww.WorkOrderId == item.Id).Select(ss => new InventoryModels
                            {
                                StoreId = item.StoreId,
                                IngredientId = ss.IngredientId,
                                Price = ss.UnitPrice.Value,
                                Quantity = ss.BaseQty.HasValue ? ss.BaseQty.Value : 0,
                                POQty = ss.Qty,
                                ReceiptQty = ss.ReceiptNoteQty.HasValue ? ss.ReceiptNoteQty.Value : 0,
                                ReturnQty = ss.ReturnReceiptNoteQty.HasValue ? ss.ReturnReceiptNoteQty.Value : 0
                            }).ToList();

                            _lstWODetail.AddRange(lstWODetail);
                        }
                        //Update inventory
                        var isUpdateInentoryWO = _inventoryFactory.UpdateInventoryForWOForCloseManual(_lstWODetail);
                        _logger.Info(string.Format("UpdateInventoryForWO: [{0}] when close manual", isUpdateInentoryWO));
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public List<string> listItemOnData(string ID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    List<string> Data = (from WOD in cxt.I_Work_Order_Detail
                                         where WOD.WorkOrderId.Equals(ID)
                                            && (WOD.Status != (int)Commons.EStatus.Deleted && WOD.Status != null)
                                         select WOD.Id).ToList();
                    return Data;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public StatusResponse Export(ref IXLWorksheet wsWO, ref IXLWorksheet wsWOD, WorkOrderModels modelFilter,
            List<SelectListItem> listStore, List<string> listCompanyId)
        {
            StatusResponse Response = new StatusResponse();
            try
            {
                using (NuWebContext cxt = new NuWebContext())
                {
                    var listData = (from tb in cxt.I_Work_Order
                                    where modelFilter.ListStores.Contains(tb.StoreId) && tb.WODate >= modelFilter.WODate && tb.WODate <= modelFilter.DateCompleted
                                    select new WorkOrderModels()
                                    {
                                        Id = tb.Id,
                                        WONumber = tb.Code,
                                        WODate = tb.WODate,
                                        DateCompleted = tb.DateCompleted,
                                        Total = tb.Total,
                                        Note = tb.Note,
                                        Status = tb.Status,
                                        StoreId = tb.StoreId,
                                    }).ToList();

                    int row = 1;
                    string[] listHeader = new string[] {
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("WO Number"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("WO Date"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Completed Date"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Note"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Status"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Purchasing Company")
                    };

                    int cols = listHeader.Length;
                    // Report Name
                    wsWO.Cell(row, 1).Value = string.Format("{0}", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("WORK ORDER").ToString().ToUpper());
                    wsWO.Range(row, 1, row, cols).Merge();
                    wsWO.Range(row, 1, row, cols).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
                    wsWO.Row(row).Style.Font.FontSize = 16;
                    wsWO.Row(row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wsWO.Row(row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    wsWO.Row(row).Height = 40;
                    // Date 
                    row++;
                    wsWO.Range(row, 1, row, cols).Merge();
                    wsWO.Cell(row, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date") + ":" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("From") + modelFilter.WODate.ToString("dd/MM/yyyy") + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("To") + modelFilter.DateCompleted.ToString("dd/MM/yyyy");
                    wsWO.Row(row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    //Header
                    row++;
                    for (int i = 1; i <= listHeader.Length; i++)
                        wsWO.Cell(row, i).Value = listHeader[i - 1];

                    //Item
                    row++;
                    int countHeader = 1;
                    int countDetail = 1;

                    List<ExportWorkOrderDetail> lstWOD = new List<ExportWorkOrderDetail>();
                    foreach (var item in listData)
                    {
                        var store = listStore.Where(x => x.Value.Equals(item.StoreId)).FirstOrDefault();
                        string WOStatus = "";
                        if (item.Status == (int)Commons.EPOStatus.Open)
                        {
                            WOStatus = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EPOStatus.Open.ToString());
                        }
                        else if (item.Status == (int)Commons.EPOStatus.Approved)
                        {
                            WOStatus = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EPOStatus.Approved.ToString());
                        }
                        else if (item.Status == (int)Commons.EPOStatus.InProgress)
                        {
                            WOStatus = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EPOStatus.InProgress.ToString());
                        }
                        else if (item.Status == (int)Commons.EPOStatus.Closed)
                        {
                            WOStatus = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EPOStatus.Closed.ToString());
                        }
                        wsWO.Cell("A" + row).Value = countHeader;
                        wsWO.Cell("B" + row).Value = item.WONumber;
                        wsWO.Cell("C" + row).Value = "'" + item.WODate.ToString("dd/MM/yyyy");
                        wsWO.Cell("D" + row).Value = "'" + item.DateCompleted.ToString("dd/MM/yyyy");
                        wsWO.Cell("E" + row).Value = item.Note;
                        wsWO.Cell("F" + row).Value = WOStatus;
                        wsWO.Cell("G" + row).Value = store == null ? "" : store.Text;

                        var ListWODetail = (from WOD in cxt.I_Work_Order_Detail
                                            from I in cxt.I_Ingredient
                                            from UOM in cxt.I_UnitOfMeasure
                                            where WOD.WorkOrderId.Equals(item.Id)
                                                    && I.IsActive && I.Id.Equals(WOD.IngredientId)
                                                    && (WOD.Status != (int)Commons.EStatus.Deleted && WOD.Status != null)
                                                    && UOM.IsActive && UOM.Id.Equals(I.BaseUOMId)
                                            select new
                                            {
                                                WOD,
                                                I,
                                                UOM
                                            }).ToList();

                        foreach (var itemDetail in ListWODetail)
                        {
                            ExportWorkOrderDetail ExportWOD = new ExportWorkOrderDetail()
                            {
                                Index = countDetail,
                                WONumber = item.WONumber,
                                ItemCode = itemDetail.I.Code,
                                ItemName = itemDetail.I.Name,
                                Description = itemDetail.I.Description,
                                Quantity = itemDetail.WOD.Qty,
                                UOM = itemDetail.UOM.Name,
                                UnitPrice = 0,//itemDetail.WOD.UnitPrice,
                                ItemTotal = 0,// itemDetail.WOD.Amount
                            };
                            countDetail++;
                            lstWOD.Add(ExportWOD);
                        }
                        row++;
                        countHeader++;
                    }
                    //FormatExcelExport(wsPO, row, cols);

                    wsWO.Range(3, 1, 3, cols).Style.Font.SetBold(true);
                    wsWO.Range(3, 1, row, cols).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wsWO.Range(3, 1, row, cols).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    wsWO.Range(1, 1, row - 1, cols).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    wsWO.Range(1, 1, row - 1, cols).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    wsWO.Columns(1, row).AdjustToContents();
                    //========= Purchasing Order Detail
                    row = 1;
                    string[] listDetailHeader = new string[] {
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("WO Number"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Code") ,_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item Name"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Description"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Quantity"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("UOM")
                    };
                    for (int i = 1; i <= listDetailHeader.Length; i++)
                        wsWOD.Cell(row, i).Value = listDetailHeader[i - 1];
                    cols = listDetailHeader.Length;
                    row++;
                    foreach (var item in lstWOD)
                    {
                        wsWOD.Cell("A" + row).Value = item.Index;
                        wsWOD.Cell("B" + row).Value = item.WONumber;
                        wsWOD.Cell("C" + row).Value = item.ItemCode;
                        wsWOD.Cell("D" + row).Value = item.ItemName;
                        wsWOD.Cell("E" + row).Value = item.Description;
                        wsWOD.Cell("F" + row).Value = item.Quantity;
                        wsWOD.Cell("G" + row).Value = item.UOM;
                        row++;
                    }
                    FormatExcelExport(wsWOD, row, cols);
                    //========
                    Response.Status = true;
                }
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

    }
}
