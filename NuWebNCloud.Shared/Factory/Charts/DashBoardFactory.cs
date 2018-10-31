using NuWebNCloud.Data;
using NuWebNCloud.Shared.Models.Charts;
using NuWebNCloud.Shared.Models.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuWebNCloud.Shared.Models;
using System.Data.Entity.SqlServer;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory;
using NuWebNCloud.Shared.Models.Api;
using System.Threading;

namespace NuWebNCloud.Shared.Factory.Charts
{
    public class DashBoardFactory
    {
        private DiscountAndMiscReportFactory _discountAndMiscReportFactory;
        private ItemizedSalesAnalysisReportFactory _itemizedSalesAnalysisReportFactory;
        private BaseFactory _baseFactory = null;

        public DashBoardFactory()
        {
            _discountAndMiscReportFactory = new DiscountAndMiscReportFactory();
            _itemizedSalesAnalysisReportFactory = new ItemizedSalesAnalysisReportFactory();
            _baseFactory = new BaseFactory();
        }

        #region Revenue Chart Report
        public async Task<List<RevenueResponseModels>> GetRevenueWeekReportAsync(BaseChartRequestModels request)
        {
            List<RevenueResponseModels> result = new List<RevenueResponseModels>();
            // Get datetime depend on business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(request.DateFrom, request.DateTo, request.ListStoreIds, (int)Commons.EStatus.Actived);
            if (_lstBusDayAllStore != null && _lstBusDayAllStore.Any())
            {
                request.DateFrom = _lstBusDayAllStore.Min(mm => mm.DateFrom);
                request.DateTo = _lstBusDayAllStore.Max(mm => mm.DateTo);
                var data = GetRevenueDataChart(request);

                // Report new DB
                //var data = GetDataRevenueChart_NewDB(request);

                if (data != null && data.Any())
                {
                    var lstBusIdData = data.Select(ss => ss.BusinessId).Distinct().ToList();
                    _lstBusDayAllStore = _lstBusDayAllStore.Where(ww => lstBusIdData.Contains(ww.Id)).ToList();
                    foreach (var busDay in _lstBusDayAllStore)
                    {
                        var lstDataOfBus = data.Where(w => w.BusinessId == busDay.Id).ToList();
                        result.Add(new RevenueResponseModels()
                        {
                            Date = busDay.DateFrom.ToString("dd/MM"),
                            Receipt = new RevenueResponseValueModels()
                            {
                                TC = lstDataOfBus.Count(),
                                ReceiptTotal = Math.Round(lstDataOfBus.Sum(aa => aa.ReceiptTotal), 2)
                            }
                        });
                    }
                }
            }
            return result;
        }

        public List<RevenueResponseModels> GetRevenueWeekReport(BaseChartRequestModels request)
        {
            List<RevenueResponseModels> result = new List<RevenueResponseModels>();
            // Get datetime depend on business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(request.DateFrom, request.DateTo, request.ListStoreIds, (int)Commons.EStatus.Actived);
            if (_lstBusDayAllStore != null && _lstBusDayAllStore.Any())
            {
                request.DateFrom = _lstBusDayAllStore.Min(mm => mm.DateFrom);
                request.DateTo = _lstBusDayAllStore.Max(mm => mm.DateTo);
                var data = GetRevenueDataChart(request);

                // Report new DB
                //var data = GetDataRevenueChart_NewDB(request);

                if (data != null && data.Any())
                {
                    var lstBusIdData = data.Select(ss => ss.BusinessId).Distinct().ToList();
                    _lstBusDayAllStore = _lstBusDayAllStore.Where(ww => lstBusIdData.Contains(ww.Id)).ToList();
                    foreach (var busDay in _lstBusDayAllStore)
                    {
                        var lstDataOfBus = data.Where(w => w.BusinessId == busDay.Id).ToList();
                        result.Add(new RevenueResponseModels()
                        {
                            Date = busDay.DateFrom.ToString("dd/MM"),
                            Receipt = new RevenueResponseValueModels()
                            {
                                TC = lstDataOfBus.Count(),
                                ReceiptTotal = Math.Round(lstDataOfBus.Sum(aa => aa.ReceiptTotal), 2)
                            }
                        });
                    }
                }
            }
            return result;
        }

        public List<RevenueResponseModels> GetRevenueMonthReport(BaseChartRequestModels request)
        {
            List<RevenueResponseModels> result = new List<RevenueResponseModels>();

            // Get datetime depend on business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(request.DateFrom, request.DateTo, request.ListStoreIds, (int)Commons.EStatus.Actived);
            if (_lstBusDayAllStore != null && _lstBusDayAllStore.Any())
            {
                request.DateFrom = _lstBusDayAllStore.Min(mm => mm.DateFrom);
                request.DateTo = _lstBusDayAllStore.Max(mm => mm.DateTo);

                var data = GetRevenueDataChart(request);

                // Report new DB
                //var data = GetDataRevenueChart_NewDB(request);

                if (data != null && data.Any())
                {
                    var lstBusIdData = data.Select(ss => ss.BusinessId).Distinct().ToList();
                    _lstBusDayAllStore = _lstBusDayAllStore.Where(ww => lstBusIdData.Contains(ww.Id)).ToList();
                    if (_lstBusDayAllStore != null && _lstBusDayAllStore.Any())
                    {
                        var lstBusDayGroup = _lstBusDayAllStore.GroupBy(gg => new { gg.DateFrom.Year, gg.DateFrom.Month })
                            .Select(ss => new
                            {
                                ss.Key.Year,
                                ss.Key.Month,
                                ListBusId = ss.Select(s => s.Id).ToList()
                            }).OrderBy(oo => oo.Year).ThenBy(oo => oo.Month).ToList();

                        foreach (var busDay in lstBusDayGroup)
                        {
                            var lstDataOfBus = data.Where(w => busDay.ListBusId.Contains(w.BusinessId)).ToList();
                            result.Add(new RevenueResponseModels()
                            {
                                Date = busDay.Month.ToString("00") + "/" + busDay.Year.ToString(),
                                Receipt = new RevenueResponseValueModels()
                                {
                                    TC = lstDataOfBus.Count(),
                                    ReceiptTotal = Math.Round(lstDataOfBus.Sum(aa => aa.ReceiptTotal), 2)
                                }
                            });
                        }
                    }
                }
            }

            return result;
        }
        public async Task<List<RevenueResponseModels>> GetRevenueMonthReportAsync(BaseChartRequestModels request)
        {
            List<RevenueResponseModels> result = new List<RevenueResponseModels>();

            // Get datetime depend on business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(request.DateFrom, request.DateTo, request.ListStoreIds, (int)Commons.EStatus.Actived);
            if (_lstBusDayAllStore != null && _lstBusDayAllStore.Any())
            {
                request.DateFrom = _lstBusDayAllStore.Min(mm => mm.DateFrom);
                request.DateTo = _lstBusDayAllStore.Max(mm => mm.DateTo);

                var data = GetRevenueDataChart(request);

                // Report new DB
                //var data = GetDataRevenueChart_NewDB(request);

                if (data != null && data.Any())
                {
                    var lstBusIdData = data.Select(ss => ss.BusinessId).Distinct().ToList();
                    _lstBusDayAllStore = _lstBusDayAllStore.Where(ww => lstBusIdData.Contains(ww.Id)).ToList();
                    if (_lstBusDayAllStore != null && _lstBusDayAllStore.Any())
                    {
                        var lstBusDayGroup = _lstBusDayAllStore.GroupBy(gg => new { gg.DateFrom.Year, gg.DateFrom.Month })
                            .Select(ss => new
                            {
                                ss.Key.Year,
                                ss.Key.Month,
                                ListBusId = ss.Select(s => s.Id).ToList()
                            }).OrderBy(oo => oo.Year).ThenBy(oo => oo.Month).ToList();

                        foreach (var busDay in lstBusDayGroup)
                        {
                            var lstDataOfBus = data.Where(w => busDay.ListBusId.Contains(w.BusinessId)).ToList();
                            result.Add(new RevenueResponseModels()
                            {
                                Date = busDay.Month.ToString("00") + "/" + busDay.Year.ToString(),
                                Receipt = new RevenueResponseValueModels()
                                {
                                    TC = lstDataOfBus.Count(),
                                    ReceiptTotal = Math.Round(lstDataOfBus.Sum(aa => aa.ReceiptTotal), 2)
                                }
                            });
                        }
                    }
                }
            }

            return result;
        }

        private List<Data.Models.RevenueTempDataModels> GetRevenueDataChart(BaseChartRequestModels request)
        {
            using (var db = new NuWebContext())
            {
                return db.GetRevenueDataChart(new Data.Models.BaseReportDataModel() {ListStores = request.ListStoreIds, FromDate = request.DateFrom
                    , ToDate = request.DateTo, Mode = (int)Commons.EStatus.Actived });
                //var data = db.R_DailyReceiptReport.Where(ww => request.ListStoreIds.Contains(ww.StoreId)
                //                    && ww.CreatedDate >= request.DateFrom && ww.CreatedDate <= request.DateTo
                //                    && ww.Mode == (int)Commons.EStatus.Actived
                //                    && string.IsNullOrEmpty(ww.CreditNoteNo)) // Only Receipt
                //                    .Select(ss => new RevenueTempModels()
                //                    {
                //                        Id = ss.Id,
                //                        CreatedDate = ss.CreatedDate,
                //                        ReceiptTotal = ss.ReceiptTotal,
                //                        BusinessId = ss.BusinessDayId
                //                    })
                //                    .ToList();
                //return data;
            }
        }
        #endregion Revenue Chart Report

        #region Hourly Sale Chart Report
        public HourlySaleChartResponseModels GetHourlySaleChartReport(HourlySaleChartRequestModels request)
        {
            HourlySaleChartResponseModels result = new HourlySaleChartResponseModels();

            // Get datetime depend on business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(request.DateFrom, request.DateTo, request.ListStoreIds, (int)Commons.EStatus.Actived);
            if (_lstBusDayAllStore != null && _lstBusDayAllStore.Any())
            {
                request.DateFrom = _lstBusDayAllStore.Min(mm => mm.DateFrom);
                request.DateTo = _lstBusDayAllStore.Max(mm => mm.DateTo);
                HourlySalesReportFactory _hourlySalesReportFactory = new HourlySalesReportFactory();

                var datas = _hourlySalesReportFactory.GetDataForChart(request.DateFrom, request.DateTo, request.ListStoreIds, (int)Commons.EStatus.Actived);

                // Report new DB
                //var datas = GetDataHourlySaleChart_NewDB(request.DateFrom, request.DateTo, request.ListStoreIds, (int)Commons.EStatus.Actived);

                if (datas != null && datas.Any())
                {
                    string time = string.Empty;
                    int timeFrom = request.TimeFrom.Hours;
                    int timeTo = request.TimeTo.Hours;
                    // Time range: from = to = 0
                    // Time: 0 => 24
                    if ((timeFrom == timeTo) && (timeFrom == 0))
                    {
                        for (int i = timeFrom; i < 24; i++)
                        {
                            time = string.Format("{0}:00 - {1}:00", i, i + 1);
                            var data = datas.Where(ww => ww.Time == i).ToList();
                            if (data != null && data.Any())
                            {
                                result.ListTimes.Add(time);
                                result.ListReceiptTotals.Add(Math.Round(data.Sum(ss => ss.ReceiptTotal), 2));
                                result.ListTC.Add(data.Sum(ss => ss.NoOfReceipt));
                                result.ListTA.Add(Math.Round((data.Sum(ss => ss.ReceiptTotal) / data.Sum(ss => ss.NoOfReceipt)), 2));
                            }
                        }
                    }
                    else
                    {
                        // Time range: from < to
                        // Time: from => to
                        if (timeFrom < timeTo)
                        {
                            for (int i = timeFrom; i < timeTo; i++)
                            {
                                time = string.Format("{0}:00 - {1}:00", i, i + 1);
                                var data = datas.Where(ww => ww.Time == i).ToList();
                                if (data != null && data.Any())
                                {
                                    result.ListTimes.Add(time);
                                    result.ListReceiptTotals.Add(Math.Round(data.Sum(ss => ss.ReceiptTotal), 2));
                                    result.ListTC.Add(data.Sum(ss => ss.NoOfReceipt));
                                    result.ListTA.Add(Math.Round((data.Sum(ss => ss.ReceiptTotal) / data.Sum(ss => ss.NoOfReceipt)), 2));
                                }
                            }
                        }
                        else // Time range: from > to || from = to != 0
                             // Time: from => 24 => 0 => to
                        {
                            // from => 24
                            for (int i = timeFrom; i < 24; i++)
                            {
                                time = string.Format("{0}:00 - {1}:00", i, i + 1);
                                var data = datas.Where(ww => ww.Time == i).ToList();
                                if (data != null && data.Any())
                                {
                                    result.ListTimes.Add(time);
                                    result.ListReceiptTotals.Add(Math.Round(data.Sum(ss => ss.ReceiptTotal), 2));
                                    result.ListTC.Add(data.Sum(ss => ss.NoOfReceipt));
                                    result.ListTA.Add(Math.Round((data.Sum(ss => ss.ReceiptTotal) / data.Sum(ss => ss.NoOfReceipt)), 2));
                                }
                            }
                            // 0 => to
                            for (int i = 0; i < timeTo; i++)
                            {
                                time = string.Format("{0}:00 - {1}:00", i, i + 1);
                                var data = datas.Where(ww => ww.Time == i).ToList();
                                if (data != null && data.Any())
                                {
                                    result.ListTimes.Add(time);
                                    result.ListReceiptTotals.Add(Math.Round(data.Sum(ss => ss.ReceiptTotal), 2));
                                    result.ListTC.Add(data.Sum(ss => ss.NoOfReceipt));
                                    result.ListTA.Add(Math.Round((data.Sum(ss => ss.ReceiptTotal) / data.Sum(ss => ss.NoOfReceipt)), 2));
                                }
                            }

                        }
                    }
                }
            }

            return result;
        }
        public async Task<HourlySaleChartResponseModels> GetHourlySaleChartReportAsync(HourlySaleChartRequestModels request)
        {
            HourlySaleChartResponseModels result = new HourlySaleChartResponseModels();

            // Get datetime depend on business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(request.DateFrom, request.DateTo, request.ListStoreIds, (int)Commons.EStatus.Actived);
            if (_lstBusDayAllStore != null && _lstBusDayAllStore.Any())
            {
                request.DateFrom = _lstBusDayAllStore.Min(mm => mm.DateFrom);
                request.DateTo = _lstBusDayAllStore.Max(mm => mm.DateTo);
                HourlySalesReportFactory _hourlySalesReportFactory = new HourlySalesReportFactory();

                var datas = _hourlySalesReportFactory.GetDataForChart(request.DateFrom, request.DateTo, request.ListStoreIds, (int)Commons.EStatus.Actived);

                // Report new DB
                //var datas = GetDataHourlySaleChart_NewDB(request.DateFrom, request.DateTo, request.ListStoreIds, (int)Commons.EStatus.Actived);

                if (datas != null && datas.Any())
                {
                    string time = string.Empty;
                    int timeFrom = request.TimeFrom.Hours;
                    int timeTo = request.TimeTo.Hours;
                    // Time range: from = to = 0
                    // Time: 0 => 24
                    if ((timeFrom == timeTo) && (timeFrom == 0))
                    {
                        for (int i = timeFrom; i < 24; i++)
                        {
                            time = string.Format("{0}:00 - {1}:00", i, i + 1);
                            var data = datas.Where(ww => ww.Time == i).ToList();
                            if (data != null && data.Any())
                            {
                                result.ListTimes.Add(time);
                                result.ListReceiptTotals.Add(Math.Round(data.Sum(ss => ss.ReceiptTotal), 2));
                                result.ListTC.Add(data.Sum(ss => ss.NoOfReceipt));
                                result.ListTA.Add(Math.Round((data.Sum(ss => ss.ReceiptTotal) / data.Sum(ss => ss.NoOfReceipt)), 2));
                            }
                        }
                    }
                    else
                    {
                        // Time range: from < to
                        // Time: from => to
                        if (timeFrom < timeTo)
                        {
                            for (int i = timeFrom; i < timeTo; i++)
                            {
                                time = string.Format("{0}:00 - {1}:00", i, i + 1);
                                var data = datas.Where(ww => ww.Time == i).ToList();
                                if (data != null && data.Any())
                                {
                                    result.ListTimes.Add(time);
                                    result.ListReceiptTotals.Add(Math.Round(data.Sum(ss => ss.ReceiptTotal), 2));
                                    result.ListTC.Add(data.Sum(ss => ss.NoOfReceipt));
                                    result.ListTA.Add(Math.Round((data.Sum(ss => ss.ReceiptTotal) / data.Sum(ss => ss.NoOfReceipt)), 2));
                                }
                            }
                        }
                        else // Time range: from > to || from = to != 0
                             // Time: from => 24 => 0 => to
                        {
                            // from => 24
                            for (int i = timeFrom; i < 24; i++)
                            {
                                time = string.Format("{0}:00 - {1}:00", i, i + 1);
                                var data = datas.Where(ww => ww.Time == i).ToList();
                                if (data != null && data.Any())
                                {
                                    result.ListTimes.Add(time);
                                    result.ListReceiptTotals.Add(Math.Round(data.Sum(ss => ss.ReceiptTotal), 2));
                                    result.ListTC.Add(data.Sum(ss => ss.NoOfReceipt));
                                    result.ListTA.Add(Math.Round((data.Sum(ss => ss.ReceiptTotal) / data.Sum(ss => ss.NoOfReceipt)), 2));
                                }
                            }
                            // 0 => to
                            for (int i = 0; i < timeTo; i++)
                            {
                                time = string.Format("{0}:00 - {1}:00", i, i + 1);
                                var data = datas.Where(ww => ww.Time == i).ToList();
                                if (data != null && data.Any())
                                {
                                    result.ListTimes.Add(time);
                                    result.ListReceiptTotals.Add(Math.Round(data.Sum(ss => ss.ReceiptTotal), 2));
                                    result.ListTC.Add(data.Sum(ss => ss.NoOfReceipt));
                                    result.ListTA.Add(Math.Round((data.Sum(ss => ss.ReceiptTotal) / data.Sum(ss => ss.NoOfReceipt)), 2));
                                }
                            }

                        }
                    }
                }
            }

            return result;
        }
        #endregion Hourly Sale Chart Report

        #region Category (GL account code)
        public CategoryChartResponseModels GetCategoryChartReport(CategoryChartRequestModels request)
        {
            Random rnd = new Random();
            CategoryChartResponseModels result = new CategoryChartResponseModels();

            // Get datetime depend on business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(request.DateFrom, request.DateTo, request.ListStoreIds, (int)Commons.EStatus.Actived);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                return result;
            }

            request.DateFrom = _lstBusDayAllStore.Min(mm => mm.DateFrom);
            request.DateTo = _lstBusDayAllStore.Max(mm => mm.DateTo);

            using (var db = new NuWebContext())
            {
                //var data = db.R_ItemizedSalesAnalysisReport.Where(ww => request.ListStoreIds.Contains(ww.StoreId)
                //        && ww.CreatedDate >= request.DateFrom && ww.CreatedDate <= request.DateTo
                //        && !string.IsNullOrEmpty(ww.GLAccountCode) && ww.Mode == (int)Commons.EStatus.Actived)
                //        .Select(ss => new CategoryTmpChartResponseModels()
                //        {
                //            GLAccountCode = ss.GLAccountCode,
                //            CategoryName = ss.CategoryName,
                //            Amount = (ss.TotalAmount.Value - ss.TotalDiscount.Value - ss.PromotionAmount - ((ss.TaxType == (int)Commons.ETax.Inclusive) ? ss.Tax : 0)),
                //            CreateDate = ss.CreatedDate,
                //            CategoryId = ss.CategoryId
                //        }).ToList();

                var dataTmp = db.GetCategoryChartDataReport(new Data.Models.BaseReportDataModel()
                {
                    ListStores = request.ListStoreIds,
                    FromDate = request.DateFrom,
                    ToDate = request.DateTo,
                    Mode = (int)Commons.EStatus.Actived
                });

                if (dataTmp != null && dataTmp.Any())
                //if (data != null && data.Any())
                {
                    var data = dataTmp.Select(ss => new CategoryTmpChartResponseModels()
                    {
                        GLAccountCode = ss.GLAccountCode,
                        CategoryName = ss.CategoryName,
                        Amount = ss.Amount,
                        CreateDate = ss.CreateDate,
                        CategoryId = ss.CategoryId
                    });
                    CategoryDetailChartResponseModels caregoryDetail = new CategoryDetailChartResponseModels();
                    var model = new BaseReportModel()
                    {
                        FromDate = request.DateFrom,
                        ToDate = request.DateTo,
                        ListStores = request.ListStoreIds,
                        Mode = (int)Commons.EStatus.Actived
                    };

                    var lstMiscs = _discountAndMiscReportFactory.GetMiscs(model);
                    var lstRound = _itemizedSalesAnalysisReportFactory.GetRoundingAmount(model);

                    var lstData = new List<CategoryTmpChartResponseModels>();
                    var lstDataBusDay = new List<CategoryTmpChartResponseModels>();
                    var lstDataBusDayGroupGLCode = new List<CategoryTmpChartResponseModels>();

                    // Get info of category with GLAccountCode start with D_
                    var existCateD_ = data.Where(ww => ww.GLAccountCode.StartsWith("D_")).FirstOrDefault();
                    var cateD_ = new CategoryTmpChartResponseModels();
                    var newCateD_ = new CategoryTmpChartResponseModels();
                    if (existCateD_ == null)
                    {
                        //====== Get list categories
                        CategoriesFactory _categoriesFactory = new CategoriesFactory();
                        CategoryApiRequestModel cateRequest = new CategoryApiRequestModel();
                        cateRequest.ListStoreIds = model.ListStores;
                        var _lstCates = _categoriesFactory.GetAllCategoriesForDailySale(cateRequest);
                        var infoCateD_ = new RFilterCategoryV1Model();
                        if (_lstCates != null && _lstCates.Any())
                        {
                            infoCateD_ = _lstCates.Where(ww => ww.GLCode.StartsWith("D_")).FirstOrDefault();
                            if (infoCateD_ != null)
                            {
                                newCateD_.GLAccountCode = infoCateD_.GLCode;
                                newCateD_.CategoryId = infoCateD_.Id;
                                newCateD_.CategoryName = infoCateD_.Name;
                                newCateD_.Amount = 0;
                            }
                        }
                    } else
                    {
                        newCateD_.GLAccountCode = existCateD_.GLAccountCode;
                        newCateD_.CategoryId = existCateD_.CategoryId;
                        newCateD_.CategoryName = existCateD_.CategoryName;
                        newCateD_.Amount = 0;
                    }

                    foreach (var busDay in _lstBusDayAllStore)
                    {
                        lstDataBusDay = data.Where(w => w.CreateDate >= busDay.DateFrom && w.CreateDate <= busDay.DateTo).ToList();
                        lstDataBusDay.Add(newCateD_);
                        lstDataBusDayGroupGLCode = lstDataBusDay
                            .GroupBy(gg => new { gg.GLAccountCode, gg.CategoryId, gg.CategoryName })
                            .Select(ss => new CategoryTmpChartResponseModels() {
                                GLAccountCode = ss.Key.GLAccountCode,
                                CategoryId = ss.Key.CategoryId,
                                CategoryName = ss.Key.CategoryName,
                                Amount = ss.Sum(su => su.Amount)
                            }).ToList();

                        cateD_ = lstDataBusDayGroupGLCode.Where(ww => ww.GLAccountCode.StartsWith("D_")).FirstOrDefault();
                        
                        if (cateD_ != null)
                        {
                            cateD_.Amount += lstMiscs.Where(ww => ww.CreatedDate >= busDay.DateFrom && ww.CreatedDate <= busDay.DateTo).Sum(ss => ss.MiscValue);

                            cateD_.Amount += lstRound.Where(ww => ww.CreatedDate >= busDay.DateFrom && ww.CreatedDate <= busDay.DateTo).Sum(ss => ss.Rounding);

                            if (cateD_.Amount < 0)
                            {
                                cateD_.Amount = 0;
                            }
                        }

                        lstData.AddRange(lstDataBusDayGroupGLCode);
                    }

                    if (lstData != null && lstData.Any())
                    {
                        var lstDataGroupGLCode = lstData.GroupBy(gg => gg.GLAccountCode).OrderBy(oo => (oo.Key.StartsWith("D_") ? oo.Key.Replace("D_", "") : oo.Key)).ToList();

                        string GLCodeName = "";
                        foreach (var item in lstDataGroupGLCode)
                        {
                            GLCodeName = item.Key;
                            if (GLCodeName.StartsWith("D_"))
                            {
                                // Remove D_ in category
                                GLCodeName = GLCodeName.Replace("D_", "");
                            }
                            result.GLAccountCode.Add(GLCodeName);
                            result.ListReceiptTotals.Add(item.Sum(ss => ss.Amount));

                            caregoryDetail = new CategoryDetailChartResponseModels();
                            caregoryDetail.GLAccountCode = GLCodeName;

                            var dataDetailGroupByCate = item.GroupBy(gg => new { gg.CategoryId, gg.CategoryName }).ToList();
                            foreach (var subItem in dataDetailGroupByCate)
                            {

                                caregoryDetail.CategoryName.Add(subItem.Key.CategoryName);
                                caregoryDetail.ListReceiptTotals.Add(subItem.Sum(ss => ss.Amount));

                                System.Drawing.Color randomColor = System.Drawing.Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                                caregoryDetail.Colors.Add(System.Drawing.ColorTranslator.ToHtml(randomColor));
                            }
                            result.ListCategoryDetail.Add(caregoryDetail);
                        }
                    }
                }
                return result;
            }
        }

        public async Task<CategoryChartResponseModels> GetCategoryChartReportAsync(CategoryChartRequestModels request)
        {
            Random rnd = new Random();
            CategoryChartResponseModels result = new CategoryChartResponseModels();

            // Get datetime depend on business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(request.DateFrom, request.DateTo, request.ListStoreIds, (int)Commons.EStatus.Actived);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                return result;
            }

            request.DateFrom = _lstBusDayAllStore.Min(mm => mm.DateFrom);
            request.DateTo = _lstBusDayAllStore.Max(mm => mm.DateTo);

            using (var db = new NuWebContext())
            {
                //var data = db.R_ItemizedSalesAnalysisReport.Where(ww => request.ListStoreIds.Contains(ww.StoreId)
                //        && ww.CreatedDate >= request.DateFrom && ww.CreatedDate <= request.DateTo
                //        && !string.IsNullOrEmpty(ww.GLAccountCode) && ww.Mode == (int)Commons.EStatus.Actived)
                //        .Select(ss => new CategoryTmpChartResponseModels()
                //        {
                //            GLAccountCode = ss.GLAccountCode,
                //            CategoryName = ss.CategoryName,
                //            Amount = (ss.TotalAmount.Value - ss.TotalDiscount.Value - ss.PromotionAmount - ((ss.TaxType == (int)Commons.ETax.Inclusive) ? ss.Tax : 0)),
                //            CreateDate = ss.CreatedDate,
                //            CategoryId = ss.CategoryId
                //        }).ToList();

                var dataTmp = db.GetCategoryChartDataReport(new Data.Models.BaseReportDataModel()
                {
                    ListStores = request.ListStoreIds,
                    FromDate = request.DateFrom,
                    ToDate = request.DateTo,
                    Mode = (int)Commons.EStatus.Actived
                });

                if (dataTmp != null && dataTmp.Any())
                //if (data != null && data.Any())
                {
                    var data = dataTmp.Select(ss => new CategoryTmpChartResponseModels()
                    {
                        GLAccountCode = ss.GLAccountCode,
                        CategoryName = ss.CategoryName,
                        Amount = ss.Amount,
                        CreateDate = ss.CreateDate,
                        CategoryId = ss.CategoryId
                    });
                    CategoryDetailChartResponseModels caregoryDetail = new CategoryDetailChartResponseModels();
                    var model = new BaseReportModel()
                    {
                        FromDate = request.DateFrom,
                        ToDate = request.DateTo,
                        ListStores = request.ListStoreIds,
                        Mode = (int)Commons.EStatus.Actived
                    };

                    var lstMiscs = _discountAndMiscReportFactory.GetMiscs(model);
                    var lstRound = _itemizedSalesAnalysisReportFactory.GetRoundingAmount(model);

                    var lstData = new List<CategoryTmpChartResponseModels>();
                    var lstDataBusDay = new List<CategoryTmpChartResponseModels>();
                    var lstDataBusDayGroupGLCode = new List<CategoryTmpChartResponseModels>();

                    // Get info of category with GLAccountCode start with D_
                    var existCateD_ = data.Where(ww => ww.GLAccountCode.StartsWith("D_")).FirstOrDefault();
                    var cateD_ = new CategoryTmpChartResponseModels();
                    var newCateD_ = new CategoryTmpChartResponseModels();
                    if (existCateD_ == null)
                    {
                        //====== Get list categories
                        CategoriesFactory _categoriesFactory = new CategoriesFactory();
                        CategoryApiRequestModel cateRequest = new CategoryApiRequestModel();
                        cateRequest.ListStoreIds = model.ListStores;
                        var _lstCates = _categoriesFactory.GetAllCategoriesForDailySale(cateRequest);
                        var infoCateD_ = new RFilterCategoryV1Model();
                        if (_lstCates != null && _lstCates.Any())
                        {
                            infoCateD_ = _lstCates.Where(ww => ww.GLCode.StartsWith("D_")).FirstOrDefault();
                            if (infoCateD_ != null)
                            {
                                newCateD_.GLAccountCode = infoCateD_.GLCode;
                                newCateD_.CategoryId = infoCateD_.Id;
                                newCateD_.CategoryName = infoCateD_.Name;
                                newCateD_.Amount = 0;
                            }
                        }
                    }
                    else
                    {
                        newCateD_.GLAccountCode = existCateD_.GLAccountCode;
                        newCateD_.CategoryId = existCateD_.CategoryId;
                        newCateD_.CategoryName = existCateD_.CategoryName;
                        newCateD_.Amount = 0;
                    }

                    foreach (var busDay in _lstBusDayAllStore)
                    {
                        lstDataBusDay = data.Where(w => w.CreateDate >= busDay.DateFrom && w.CreateDate <= busDay.DateTo).ToList();
                        lstDataBusDay.Add(newCateD_);
                        lstDataBusDayGroupGLCode = lstDataBusDay
                            .GroupBy(gg => new { gg.GLAccountCode, gg.CategoryId, gg.CategoryName })
                            .Select(ss => new CategoryTmpChartResponseModels()
                            {
                                GLAccountCode = ss.Key.GLAccountCode,
                                CategoryId = ss.Key.CategoryId,
                                CategoryName = ss.Key.CategoryName,
                                Amount = ss.Sum(su => su.Amount)
                            }).ToList();

                        cateD_ = lstDataBusDayGroupGLCode.Where(ww => ww.GLAccountCode.StartsWith("D_")).FirstOrDefault();

                        if (cateD_ != null)
                        {
                            cateD_.Amount += lstMiscs.Where(ww => ww.CreatedDate >= busDay.DateFrom && ww.CreatedDate <= busDay.DateTo).Sum(ss => ss.MiscValue);

                            cateD_.Amount += lstRound.Where(ww => ww.CreatedDate >= busDay.DateFrom && ww.CreatedDate <= busDay.DateTo).Sum(ss => ss.Rounding);

                            if (cateD_.Amount < 0)
                            {
                                cateD_.Amount = 0;
                            }
                        }

                        lstData.AddRange(lstDataBusDayGroupGLCode);
                    }

                    if (lstData != null && lstData.Any())
                    {
                        var lstDataGroupGLCode = lstData.GroupBy(gg => gg.GLAccountCode).OrderBy(oo => (oo.Key.StartsWith("D_") ? oo.Key.Replace("D_", "") : oo.Key)).ToList();

                        string GLCodeName = "";
                        foreach (var item in lstDataGroupGLCode)
                        {
                            GLCodeName = item.Key;
                            if (GLCodeName.StartsWith("D_"))
                            {
                                // Remove D_ in category
                                GLCodeName = GLCodeName.Replace("D_", "");
                            }
                            result.GLAccountCode.Add(GLCodeName);
                            result.ListReceiptTotals.Add(item.Sum(ss => ss.Amount));

                            caregoryDetail = new CategoryDetailChartResponseModels();
                            caregoryDetail.GLAccountCode = GLCodeName;

                            var dataDetailGroupByCate = item.GroupBy(gg => new { gg.CategoryId, gg.CategoryName }).ToList();
                            foreach (var subItem in dataDetailGroupByCate)
                            {

                                caregoryDetail.CategoryName.Add(subItem.Key.CategoryName);
                                caregoryDetail.ListReceiptTotals.Add(subItem.Sum(ss => ss.Amount));

                                System.Drawing.Color randomColor = System.Drawing.Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                                caregoryDetail.Colors.Add(System.Drawing.ColorTranslator.ToHtml(randomColor));
                            }
                            result.ListCategoryDetail.Add(caregoryDetail);
                        }
                    }
                }
                return result;
            }
        }
        #endregion

        #region Top Selling Chart Report
        public TopSellingChartReponseModels GetTopSellingChartReport(TopSellingChartRequestModels request)
        {
            Random rnd = new Random();
            TopSellingChartReponseModels result = new TopSellingChartReponseModels();
            List<ItemTopSellingChartReponseModels> lstItem = new List<ItemTopSellingChartReponseModels>();
            ItemTopSellingChartReponseModels item = new ItemTopSellingChartReponseModels();

            // Get data
            using (var db = new NuWebContext())
            {
                //var data = db.R_TopSellingProductsReport.Where(ww => request.ListStoreIds.Contains(ww.StoreId)
                //    && (ww.CreatedDate >= request.DateFrom && ww.CreatedDate <= request.DateTo)
                //    && ww.Mode == (int)Commons.EStatus.Actived)
                //    .GroupBy(gg => new { ItemId = gg.ItemId, ItemName = gg.ItemName })
                //    .Select(ss => new TopSellingChartTmpModels()
                //    {
                //        ItemName = ss.Key.ItemName,
                //        Qty = ss.Sum(s => s.Qty),
                //        Amount = ss.Sum(s => s.Amount)
                //    }).OrderByDescending(oo => (request.ItemType == 0 ? oo.Qty : oo.Amount))
                //    .Take(request.TopSell)
                //    .ToList();

                var data = db.GetTopSellingChartReport(new Data.Models.BaseReportDataModel()
                {
                    FromDate = request.DateFrom,
                    ToDate = request.DateTo,
                    Mode = (int)Commons.EStatus.Actived,
                    TopTake = request.TopSell,
                    ItemType = request.ItemType,
                    ListStores = request.ListStoreIds
                });

                // Report new DB
                //var data = GetDataTopSellingChart_NewDB(request);

                if (data != null && data.Any())
                {
                    int length = data.Count() + 2;
                    result.labels = new string[length];
                    result.labels[0] = "";
                    int rank = 1;
                    data.ForEach(itm =>
                    {
                        item = new ItemTopSellingChartReponseModels();
                        item.data = new string[length];
                        // Name
                        item.label = itm.ItemName;
                        // Color
                        System.Drawing.Color randomColor = System.Drawing.Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                        item.backgroundColor = System.Drawing.ColorTranslator.ToHtml(randomColor);
                        // Value
                        if (request.ItemType == 1)
                        {
                            item.data[rank] = itm.Amount.ToString();
                        }
                        else
                        {
                            item.data[rank] = itm.Qty.ToString();
                        }
                        // Set return data
                        result.labels[rank] = rank.ToString();
                        result.datasets.Add(item);
                        rank++;
                    });
                    result.labels[length - 1] = "";
                } else
                {
                    result.labels = new string[0];
                }
                return result;
            }

        }

        public async Task<TopSellingChartReponseModels> GetTopSellingChartReportAsync(TopSellingChartRequestModels request)
        {
            Random rnd = new Random();
            TopSellingChartReponseModels result = new TopSellingChartReponseModels();
            List<ItemTopSellingChartReponseModels> lstItem = new List<ItemTopSellingChartReponseModels>();
            ItemTopSellingChartReponseModels item = new ItemTopSellingChartReponseModels();

            // Get data
            using (var db = new NuWebContext())
            {
                //var data = db.R_TopSellingProductsReport.Where(ww => request.ListStoreIds.Contains(ww.StoreId)
                //    && (ww.CreatedDate >= request.DateFrom && ww.CreatedDate <= request.DateTo)
                //    && ww.Mode == (int)Commons.EStatus.Actived)
                //    .GroupBy(gg => new { ItemId = gg.ItemId, ItemName = gg.ItemName })
                //    .Select(ss => new TopSellingChartTmpModels()
                //    {
                //        ItemName = ss.Key.ItemName,
                //        Qty = ss.Sum(s => s.Qty),
                //        Amount = ss.Sum(s => s.Amount)
                //    }).OrderByDescending(oo => (request.ItemType == 0 ? oo.Qty : oo.Amount))
                //    .Take(request.TopSell)
                //    .ToList();

                var data = db.GetTopSellingChartReport(new Data.Models.BaseReportDataModel()
                {
                    FromDate = request.DateFrom,
                    ToDate = request.DateTo,
                    Mode = (int)Commons.EStatus.Actived,
                    TopTake = request.TopSell,
                    ItemType = request.ItemType,
                    ListStores = request.ListStoreIds
                });

                // Report new DB
                //var data = GetDataTopSellingChart_NewDB(request);

                if (data != null && data.Any())
                {
                    int length = data.Count() + 2;
                    result.labels = new string[length];
                    result.labels[0] = "";
                    int rank = 1;
                    data.ForEach(itm =>
                    {
                        item = new ItemTopSellingChartReponseModels();
                        item.data = new string[length];
                        // Name
                        item.label = itm.ItemName;
                        // Color
                        System.Drawing.Color randomColor = System.Drawing.Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                        item.backgroundColor = System.Drawing.ColorTranslator.ToHtml(randomColor);
                        // Value
                        if (request.ItemType == 1)
                        {
                            item.data[rank] = itm.Amount.ToString();
                        }
                        else
                        {
                            item.data[rank] = itm.Qty.ToString();
                        }
                        // Set return data
                        result.labels[rank] = rank.ToString();
                        result.datasets.Add(item);
                        rank++;
                    });
                    result.labels[length - 1] = "";
                }
                else
                {
                    result.labels = new string[0];
                }
                return result;
            }

        }
        #endregion Top Selling Chart Report

        #region Chart report with new DB
        #region Revenue Chart Report
        private List<RevenueTempModels> GetDataRevenueChart_NewDB(BaseChartRequestModels request)
        {
            using (var db = new NuWebContext())
            {
                var data = db.R_PosSale.Where(ww => request.ListStoreIds.Contains(ww.StoreId)
                                    && ww.ReceiptCreatedDate >= request.DateFrom && ww.ReceiptCreatedDate <= request.DateTo
                                    && ww.Mode == (int)Commons.EStatus.Actived
                                    && string.IsNullOrEmpty(ww.CreditNoteNo)) // Only Receipt
                                    .Select(ss => new RevenueTempModels()
                                    {
                                        Id = ss.Id,
                                        CreatedDate = ss.CreatedDate,
                                        ReceiptTotal = ss.ReceiptTotal,
                                        BusinessId = ss.BusinessId
                                    }).ToList();
                return data;
            }
        }
        #endregion Revenue Chart Report

        #region Hourly Sale Chart Report
        public List<HourlySalesReportModels> GetDataHourlySaleChart_NewDB(DateTime dFrom, DateTime dTo, List<string> lstStores, int mode)
        {
            using (var db = new NuWebContext())
            {
                var lstData = (from tb in db.R_PosSale
                               where lstStores.Contains(tb.StoreId)
                                     && (tb.ReceiptCreatedDate >= dFrom && tb.ReceiptCreatedDate <= dTo)
                                     && tb.Mode == mode
                                     && string.IsNullOrEmpty(tb.CreditNoteNo) // Only receipt
                               group tb by new
                               {
                                   StoreId = tb.StoreId,
                                   Time = (int?)SqlFunctions.DatePart("HH", tb.ReceiptCreatedDate.Value),
                               } into g
                               select new HourlySalesReportModels
                               {
                                   StoreId = g.Key.StoreId,
                                   Time = g.Key.Time.Value,
                                   ReceiptTotal = g.Sum(x => x.ReceiptTotal),
                                   NoOfReceipt = g.Count(),
                                    NoOfPerson = g.Sum(x => (int?)x.NoOfPerson) == null ? 0 : g.Sum(x => x.NoOfPerson),
                                   PerNoOfReceipt = (g.Sum(x => x.ReceiptTotal) / (g.Count() == 0 ? 1 : g.Count())),
                                   PerNoOfPerson = g.Sum(x => (int?)x.NoOfPerson) == null ? 0 :
                                    g.Sum(x => x.ReceiptTotal) / (g.Sum(x => x.NoOfPerson) == 0 ? 1 : g.Sum(x => x.NoOfPerson)),
                               }).ToList();
                return lstData;
            }
        }
        #endregion Hourly Sale Chart Report

        #region Category (GL account code)
        public CategoryChartResponseModels GetCategoryChartReport_NewDB(CategoryChartRequestModels request)
        {
            Random rnd = new Random();
            CategoryChartResponseModels result = new CategoryChartResponseModels();

            // Get datetime depend on business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(request.DateFrom, request.DateTo, request.ListStoreIds, (int)Commons.EStatus.Actived);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                return result;
            }

            request.DateFrom = _lstBusDayAllStore.Min(mm => mm.DateFrom);
            request.DateTo = _lstBusDayAllStore.Max(mm => mm.DateTo);

            using (var db = new NuWebContext())
            {
                var data = (from ps in db.R_PosSale
                            from psd in db.R_PosSaleDetail.Where(w => w.StoreId == ps.StoreId && w.OrderId == ps.OrderId)
                            where request.ListStoreIds.Contains(psd.StoreId)
                                && ps.ReceiptCreatedDate >= request.DateFrom && ps.ReceiptCreatedDate <= request.DateTo
                                && psd.Mode == (int)Commons.EStatus.Actived && !string.IsNullOrEmpty(psd.GLAccountCode)
                            select new CategoryTmpChartResponseModels()
                            {
                                GLAccountCode = psd.GLAccountCode,
                                CategoryName = psd.CategoryName,
                                Amount = (psd.TotalAmount - (psd.IsDiscountTotal.HasValue && psd.IsDiscountTotal.Value ? 0: psd.Discount) - psd.PromotionAmount - ((psd.TaxType == (int)Commons.ETax.Inclusive) ? psd.Tax : 0)),
                                CreateDate = psd.CreatedDate,
                                CategoryId = psd.CategoryId,
                                ReceiptId = psd.OrderId
                            }).ToList();

                if (data != null && data.Any())
                {
                    CategoryDetailChartResponseModels caregoryDetail = new CategoryDetailChartResponseModels();
                    var model = new BaseReportModel()
                    {
                        FromDate = request.DateFrom,
                        ToDate = request.DateTo,
                        ListStores = request.ListStoreIds,
                        Mode = (int)Commons.EStatus.Actived
                    };

                    PosSaleFactory posSaleFactory = new PosSaleFactory();
                    var lstOrderId = data.Select(ss => ss.ReceiptId).Distinct().ToList();
                    var lstMiscs = posSaleFactory.GetMiscs(request.ListStoreIds, lstOrderId, (int)Commons.EStatus.Actived);
                    var lstRound = posSaleFactory.GetRoundingAmount(request.ListStoreIds, lstOrderId, (int)Commons.EStatus.Actived);

                    var lstData = new List<CategoryTmpChartResponseModels>();
                    var lstDataBusDay = new List<CategoryTmpChartResponseModels>();
                    var lstDataBusDayGroupGLCode = new List<CategoryTmpChartResponseModels>();

                    // Get info of category with GLAccountCode start with D_
                    var existCateD_ = data.Where(ww => ww.GLAccountCode.StartsWith("D_")).FirstOrDefault();
                    var cateD_ = new CategoryTmpChartResponseModels();
                    var newCateD_ = new CategoryTmpChartResponseModels();
                    if (existCateD_ == null)
                    {
                        //====== Get list categories
                        CategoriesFactory _categoriesFactory = new CategoriesFactory();
                        CategoryApiRequestModel cateRequest = new CategoryApiRequestModel();
                        cateRequest.ListStoreIds = model.ListStores;
                        var _lstCates = _categoriesFactory.GetAllCategoriesForDailySale(cateRequest);
                        var infoCateD_ = new RFilterCategoryV1Model();
                        if (_lstCates != null && _lstCates.Any())
                        {
                            infoCateD_ = _lstCates.Where(ww => ww.GLCode.StartsWith("D_")).FirstOrDefault();
                            if (infoCateD_ != null)
                            {
                                newCateD_.GLAccountCode = infoCateD_.GLCode;
                                newCateD_.CategoryId = infoCateD_.Id;
                                newCateD_.CategoryName = infoCateD_.Name;
                                newCateD_.Amount = 0;
                            }
                        }
                    }
                    else
                    {
                        newCateD_.GLAccountCode = existCateD_.GLAccountCode;
                        newCateD_.CategoryId = existCateD_.CategoryId;
                        newCateD_.CategoryName = existCateD_.CategoryName;
                        newCateD_.Amount = 0;
                    }

                    foreach (var busDay in _lstBusDayAllStore)
                    {
                        lstDataBusDay = data.Where(w => w.CreateDate >= busDay.DateFrom && w.CreateDate <= busDay.DateTo).ToList();
                        lstDataBusDay.Add(newCateD_);
                        lstDataBusDayGroupGLCode = lstDataBusDay
                            .GroupBy(gg => new { gg.GLAccountCode, gg.CategoryId, gg.CategoryName })
                            .Select(ss => new CategoryTmpChartResponseModels()
                            {
                                GLAccountCode = ss.Key.GLAccountCode,
                                CategoryId = ss.Key.CategoryId,
                                CategoryName = ss.Key.CategoryName,
                                Amount = ss.Sum(su => su.Amount)
                            }).ToList();

                        cateD_ = lstDataBusDayGroupGLCode.Where(ww => ww.GLAccountCode.StartsWith("D_")).FirstOrDefault();

                        if (cateD_ != null)
                        {
                            cateD_.Amount += lstMiscs.Where(ww => ww.CreatedDate >= busDay.DateFrom && ww.CreatedDate <= busDay.DateTo).Sum(ss => ss.MiscValue);

                            cateD_.Amount += lstRound.Where(ww => ww.CreatedDate >= busDay.DateFrom && ww.CreatedDate <= busDay.DateTo).Sum(ss => ss.Rounding);

                            if (cateD_.Amount < 0)
                            {
                                cateD_.Amount = 0;
                            }
                        }

                        lstData.AddRange(lstDataBusDayGroupGLCode);
                    }

                    if (lstData != null && lstData.Any())
                    {
                        var lstDataGroupGLCode = lstData.GroupBy(gg => gg.GLAccountCode).OrderBy(oo => (oo.Key.StartsWith("D_") ? oo.Key.Replace("D_", "") : oo.Key)).ToList();

                        string GLCodeName = "";
                        foreach (var item in lstDataGroupGLCode)
                        {
                            GLCodeName = item.Key;
                            if (GLCodeName.StartsWith("D_"))
                            {
                                // Remove D_ in category
                                GLCodeName = GLCodeName.Replace("D_", "");
                            }
                            result.GLAccountCode.Add(GLCodeName);
                            result.ListReceiptTotals.Add(item.Sum(ss => ss.Amount));

                            caregoryDetail = new CategoryDetailChartResponseModels();
                            caregoryDetail.GLAccountCode = GLCodeName;

                            var dataDetailGroupByCate = item.GroupBy(gg => new { gg.CategoryId, gg.CategoryName }).ToList();
                            foreach (var subItem in dataDetailGroupByCate)
                            {

                                caregoryDetail.CategoryName.Add(subItem.Key.CategoryName);
                                caregoryDetail.ListReceiptTotals.Add(subItem.Sum(ss => ss.Amount));

                                System.Drawing.Color randomColor = System.Drawing.Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                                caregoryDetail.Colors.Add(System.Drawing.ColorTranslator.ToHtml(randomColor));
                            }
                            result.ListCategoryDetail.Add(caregoryDetail);
                        }
                    }
                }

                return result;
            }
        }
        #endregion

        #region Top Selling Chart Report
        public List<TopSellingChartTmpModels> GetDataTopSellingChart_NewDB(TopSellingChartRequestModels request)
        {
            using (var db = new NuWebContext())
            {
                var data = (from ps in db.R_PosSale
                            from psd in db.R_PosSaleDetail.Where(w => w.StoreId == ps.StoreId && w.OrderId == ps.OrderId)
                            where request.ListStoreIds.Contains(ps.StoreId)
                                 && ps.ReceiptCreatedDate >= request.DateFrom && ps.ReceiptCreatedDate <= request.DateTo
                                 && psd.Mode == (int)Commons.EStatus.Actived
                                 && (psd.ItemTypeId == (int)Commons.EProductType.Dish || psd.ItemTypeId == (int)Commons.EProductType.SetMenu)
                                 && string.IsNullOrEmpty(ps.CreditNoteNo) && !string.IsNullOrEmpty(psd.ItemId)// Only receipt
                            group psd by new { ItemId = psd.ItemId, ItemName = psd.ItemName } into gg
                            select new TopSellingChartTmpModels()
                            {
                                ItemName = gg.Key.ItemName,
                                Qty = gg.Sum(s => s.Quantity),
                                Amount = gg.Sum(s => s.TotalAmount)
                            }).OrderByDescending(oo => (request.ItemType == 0 ? oo.Qty : oo.Amount))
                            .Take(request.TopSell).ToList();

                return data;
            }
        }
        #endregion Top Selling Chart Report
        #endregion Chart report with new DB
    }
}
