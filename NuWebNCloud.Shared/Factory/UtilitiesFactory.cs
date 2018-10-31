using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.IO;
using System.Linq;
using System.Web;

namespace NuWebNCloud.Shared.Factory
{
    public class UtilitiesFactory
    {
        //var jsonContent = JsonConvert.SerializeObject(lstInfo);
        //_baseFactory.InsertTrackingLog("G_DateOfWeeks", jsonContent, "DateOfWeeksId", result);

        public ResultModels DelDataSalereport(ClearDataSaleReportModels request)
        {
            ResultModels result = new ResultModels();
            result.IsOk = true;

            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        var lstBusinesDay = cxt.G_BusinessDay.Where(ww => ww.StartedOn >= request.DFrom && ww.StartedOn <= request.DTo && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.G_BusinessDay.RemoveRange(lstBusinesDay);

                        var lstOrderTip = cxt.G_OrderTip.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.G_OrderTip.RemoveRange(lstOrderTip);

                        var lstPayments = cxt.G_PaymentMenthod.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.G_PaymentMenthod.RemoveRange(lstPayments);

                        var lstAudits = cxt.R_AuditTrailReport.Where(ww => ww.ReceiptDate >= request.DFrom && ww.ReceiptDate <= request.DTo
                        && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_AuditTrailReport.RemoveRange(lstAudits);

                        var lstCashIn = cxt.R_CashInOutReport.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo
                        && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_CashInOutReport.RemoveRange(lstCashIn);

                        var lstCloseReceipts = cxt.R_ClosedReceiptReport.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo
                        && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_ClosedReceiptReport.RemoveRange(lstCloseReceipts);

                        var lstDailyItems = cxt.R_DailyItemizedSalesReportDetail.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo
                        && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_DailyItemizedSalesReportDetail.RemoveRange(lstDailyItems);

                        var lstDailyItemsForSet = cxt.R_DailyItemizedSalesReportDetailForSet.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo
                       && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_DailyItemizedSalesReportDetailForSet.RemoveRange(lstDailyItemsForSet);


                        var lstDailyReceipt = cxt.R_DailyReceiptReport.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo
                        && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_DailyReceiptReport.RemoveRange(lstDailyReceipt);

                        var lstDailySale = cxt.R_DailySalesReport.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo
                       && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_DailySalesReport.RemoveRange(lstDailySale);

                        var lstDiscounts = cxt.R_DiscountAndMiscReport.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo
                      && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_DiscountAndMiscReport.RemoveRange(lstDiscounts);

                        var lstDiscountDetails = cxt.R_DiscountDetailsReport.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo
                      && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_DiscountDetailsReport.RemoveRange(lstDiscountDetails);

                        var lstHourlyItem = cxt.R_HourlyItemizedSalesReport.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo
                      && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_HourlyItemizedSalesReport.RemoveRange(lstHourlyItem);

                        var lstHourlySale = cxt.R_HourlySalesReport.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo
                     && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_HourlySalesReport.RemoveRange(lstHourlySale);

                        var lstItemizedSales = cxt.R_ItemizedSalesAnalysisReport.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo
                     && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_ItemizedSalesAnalysisReport.RemoveRange(lstItemizedSales);

                        var lstItemizedSaleDetails = cxt.R_ItemizedSalesAnalysisReportDetail.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo
                  && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_ItemizedSalesAnalysisReportDetail.RemoveRange(lstItemizedSaleDetails);

                        var lstNoIncludeSale = cxt.R_NoIncludeOnSaleDataReport.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo
                  && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_NoIncludeOnSaleDataReport.RemoveRange(lstNoIncludeSale);

                        var lstNoSale = cxt.R_NoSaleDetailReport.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo
                  && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_NoSaleDetailReport.RemoveRange(lstNoSale);

                        var lstReceiptsbyPaymentMethods = cxt.R_ReceiptsbyPaymentMethodsReport.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo
                 && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_ReceiptsbyPaymentMethodsReport.RemoveRange(lstReceiptsbyPaymentMethods);

                        var lstTimeClock = cxt.R_TimeClockReport.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo
                && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_TimeClockReport.RemoveRange(lstTimeClock);

                        var lstTopSale = cxt.R_TopSellingProductsReport.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo
                && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_TopSellingProductsReport.RemoveRange(lstTopSale);


                        var lstRefund = cxt.R_Refund.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo
               && request.ListStoreIds.Contains(ww.StoreId));
                        if (lstRefund != null && lstRefund.Any())
                        {
                            var lstId = lstRefund.Select(ss => ss.Id).ToList();
                            var lstRefundDetail = cxt.R_RefundDetail.Where(ww => lstId.Contains(ww.RefundId));
                            cxt.R_RefundDetail.RemoveRange(lstRefundDetail);
                            cxt.R_Refund.RemoveRange(lstRefund);
                        }

                        var lstHourlyItemized = cxt.R_HourlyItemizedSalesReport.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo
               && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_HourlyItemizedSalesReport.RemoveRange(lstHourlyItemized);

                        var lstItemizedCancelOrRefund = cxt.R_ItemizedCancelOrRefundData.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo
               && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_ItemizedCancelOrRefundData.RemoveRange(lstItemizedCancelOrRefund);

                        //shiftlog
                        var lstShiftLog = cxt.R_ShiftLog.Where(ww => ww.StartedOn >= request.DFrom && ww.StartedOn <= request.DTo
              && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_ShiftLog.RemoveRange(lstShiftLog);

                        //Pos Sale
                        var posSales = cxt.R_PosSale.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_PosSale.RemoveRange(posSales);

                        var posSalesDetail = cxt.R_PosSaleDetail.Where(ww => ww.CreatedDate >= request.DFrom && ww.CreatedDate <= request.DTo && request.ListStoreIds.Contains(ww.StoreId));
                        cxt.R_PosSaleDetail.RemoveRange(posSalesDetail);


                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("Delete data sale report success", request);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Delete data sale report fail", ex);
                        //_logger.Error(ex);
                        result.IsOk = false;
                        result.Message = ex.Message;
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

        public ResultModels CreateOrUpdateMerchantExtend(MerchantExtendRequestModel request)
        {
            ResultModels result = new ResultModels();
            result.IsOk = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        var merchantExtend = (from em in cxt.G_EmployeeOnMerchantExtend
                                              join pe in cxt.G_POSEmployeeConfig on em.POSEmployeeConfigId equals pe.Id
                                              where pe.UserName == request.UserName && em.POSAPIMerchantConfigId == request.POSAPIMerchantConfigExtendId
                                              select new { Id = em.Id, POSEmpConfigId = em.POSEmployeeConfigId }).FirstOrDefault();
                        if (merchantExtend != null)//exist
                        {
                            //check list stores
                            if (request.ListStoreExtendIds == null || request.ListStoreExtendIds.Count == 0)//hide all extend
                            {

                                var lstEmOnStores = cxt.G_EmployeeOnStoreExtend.Where(ww => ww.EmpOnMerchantExtendId == merchantExtend.Id).ToList();
                                lstEmOnStores.ForEach(ss => ss.IsActived = false);
                                //hide header
                                var empOnMerchant = cxt.G_EmployeeOnMerchantExtend.Where(ww => ww.Id == merchantExtend.Id).FirstOrDefault();
                                empOnMerchant.IsActived = false;
                            }
                            else //update store extend
                            {
                                List<G_EmployeeOnStoreExtend> lstEmpOnStoreNew = new List<G_EmployeeOnStoreExtend>();
                                G_EmployeeOnStoreExtend obj = null;
                                var lstEmOnStores = cxt.G_EmployeeOnStoreExtend.Where(ww => ww.EmpOnMerchantExtendId == merchantExtend.Id).ToList();
                                foreach (var item in request.ListStoreExtendIds)
                                {
                                    obj = lstEmOnStores.Where(ww => ww.StoreExtendId == item).FirstOrDefault();
                                    if (obj == null)//new
                                    {
                                        obj = new G_EmployeeOnStoreExtend();
                                        obj.Id = Guid.NewGuid().ToString();
                                        obj.EmpOnMerchantExtendId = merchantExtend.Id;
                                        obj.StoreExtendId = item;
                                        obj.IsActived = true;

                                        lstEmpOnStoreNew.Add(obj);
                                    }
                                    else
                                    {
                                        obj.IsActived = true;
                                    }
                                }
                                var empOnMerchant = cxt.G_EmployeeOnMerchantExtend.Where(ww => ww.Id == merchantExtend.Id).FirstOrDefault();
                                empOnMerchant.IsActived = true;

                                cxt.G_EmployeeOnStoreExtend.AddRange(lstEmpOnStoreNew);

                            }
                        }
                        else //add new
                        {
                            //check emp
                            var empId = cxt.G_POSEmployeeConfig.Where(ww => ww.UserName == request.UserName && ww.IsActived).Select(ss => ss.Id).FirstOrDefault();
                            G_EmployeeOnMerchantExtend empOnMerchant = new G_EmployeeOnMerchantExtend();
                            empOnMerchant.Id = Guid.NewGuid().ToString();
                            empOnMerchant.IsActived = true;
                            empOnMerchant.POSAPIMerchantConfigId = request.POSAPIMerchantConfigExtendId;
                            empOnMerchant.POSEmployeeConfigId = empId;
                            empOnMerchant.CreatedDate = DateTime.Now;

                            //detail
                            List<G_EmployeeOnStoreExtend> lstEmpOnStoreNew = new List<G_EmployeeOnStoreExtend>();
                            G_EmployeeOnStoreExtend obj = null;
                            foreach (var item in request.ListStoreExtendIds)
                            {
                                obj = new G_EmployeeOnStoreExtend();
                                obj.Id = Guid.NewGuid().ToString();
                                obj.EmpOnMerchantExtendId = empOnMerchant.Id;
                                obj.StoreExtendId = item;
                                obj.IsActived = true;
                                obj.CreatedDate = DateTime.Now;

                                lstEmpOnStoreNew.Add(obj);

                            }

                            cxt.G_EmployeeOnMerchantExtend.Add(empOnMerchant);
                            cxt.G_EmployeeOnStoreExtend.AddRange(lstEmpOnStoreNew);
                        }
                        cxt.SaveChanges();
                        transaction.Commit();
                        NSLog.Logger.Info("CreateOrUpdateMerchantExtend success", request);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("CreateOrUpdateMerchantExtend fail", ex);

                        result.IsOk = false;
                        result.Message = ex.Message;
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

        public ResultModels IsDirty()
        {
            try
            {
                NSLog.Logger.Info("Checking DbContext Changes");
                var config = new NuWebNCloud.Data.Migrations.Configuration();
                NSLog.Logger.Info("Initialized Configuration", config);
                var migrator = new DbMigrator(config);
                NSLog.Logger.Info("Initialized Migrator", migrator);
                var scriptor = new MigratorScriptingDecorator(migrator);
                NSLog.Logger.Info("Initialized Scriptor", scriptor);
                NSLog.Logger.Info("Generating Migration Update Script...");
                var script = scriptor.ScriptUpdate(null, null);
                if (string.IsNullOrEmpty(script)) { NSLog.Logger.Info("No pending explicit migrations."); }
                else
                {
                    var dateLog= $"{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}{DateTime.Now.Millisecond}";
                    using (var sw = new StreamWriter(HttpContext.Current.Server.MapPath($"~/logs/{dateLog}.sql")))
                    {
                        sw.WriteLine(script);
                    }
                }
                script = script.Replace("\n", " ").Replace("\r", " ");

                NSLog.Logger.Info("Generated Script", script);

                return new ResultModels()
                {
                    IsOk = true,
                    Message = "There are some changes on Database Structure. Please copy all the data in RawData and run it on SQL Server or look for a <yyyymmddhhMMssttt>.sql file inside Nuweb 'logs' folder.",
                    RawData = script,
                };
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("IsDirty", ex);
                return new ResultModels()
                {
                    IsOk = false,
                    RawData = ex
                };
            }
        }
    }
}
