using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Models;
using System.Data.Entity.SqlServer;
using System.Data.Entity;
using NuWebNCloud.Shared.Models.Reports;

namespace NuWebNCloud.Shared.Factory
{
    public class DiscountAndMiscReportFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public DiscountAndMiscReportFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public bool Insert(List<DiscountAndMiscReportModels> lstInfo)
        {
            var result = true;
            var info = lstInfo.FirstOrDefault();
            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.R_DiscountAndMiscReport.Where(ww => ww.StoreId == info.StoreId
                                && ww.CreatedDate == info.CreatedDate).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert Misc data exist");
                    return result;
                }
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<R_DiscountAndMiscReport> lstInsert = new List<R_DiscountAndMiscReport>();
                        R_DiscountAndMiscReport itemInsert = null;
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new R_DiscountAndMiscReport();
                            itemInsert.Id = Guid.NewGuid().ToString();
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.DiscountValue = item.DiscountValue;
                            itemInsert.MiscValue = item.MiscValue;
                            itemInsert.Mode = item.Mode;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.R_DiscountAndMiscReport.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("Insert Misc data success", lstInfo);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert Misc data fail", ex);
                        //_logger.Error(ex);
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
            //var jsonContent = JsonConvert.SerializeObject(lstInfo);
            //_baseFactory.InsertTrackingLog("R_DiscountAndMiscReport", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        public List<DiscountAndMiscReportModels> GetReceiptDiscountAndMisc(DateTime fromDate, DateTime toDate, List<string> StoreIds, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = new List<DiscountAndMiscReportModels>();
                var query = (from m in cxt.R_DiscountAndMiscReport
                             where StoreIds.Contains(m.StoreId) && m.CreatedDate >= fromDate && m.CreatedDate <= toDate
                             && m.Mode == mode
                             group m by new
                             {
                                 Hour = (int?)SqlFunctions.DatePart("HH", m.CreatedDate),
                                 StoreId = m.StoreId,
                                 //Date = DbFunctions.TruncateTime(m.CreatedDate)
                             } into g
                             select g).ToList();
                if (query != null && query.Any())
                {
                    lstData = query.Select(g => new DiscountAndMiscReportModels
                    {
                        StoreId = g.Key.StoreId,
                        Hour = g.Key.Hour.Value,
                        TimeSpanHour = new TimeSpan(g.Key.Hour.Value, 0, 0),
                        //CreatedDate = g.Key.Date.Value,
                        DiscountValue = g.Sum(ss => ss.DiscountValue),
                        MiscValue = g.Sum(ss => ss.MiscValue)
                    }).ToList();
                }


                return lstData;
            }
        }

        /// <summary>
        /// For ItemizedSalesAnalysisDetailReport filter time from input (report old DB)
        /// Updated 05082018
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="StoreIds"></param>
        /// <param name="mode"></param>
        /// <param name="dtFromFilter"></param>
        /// <param name="dtToFilter"></param>
        /// <returns></returns>
        public List<DiscountAndMiscReportModels> GetReceiptDiscountAndMisc(DateTime fromDate, DateTime toDate, List<string> StoreIds, int mode, DateTime dtFromFilter, DateTime dtToFilter, int filterType = 0)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = new List<DiscountAndMiscReportModels>();
                var query = (from m in cxt.R_DiscountAndMiscReport.AsNoTracking()
                             where StoreIds.Contains(m.StoreId) && m.CreatedDate >= fromDate && m.CreatedDate <= toDate && m.Mode == mode
                             select new
                             {
                                 m.StoreId, m.CreatedDate, m.DiscountValue, m.MiscValue
                             }).ToList();
                switch (filterType)
                {
                    case (int)Commons.EFilterType.OnDay:
                        query = query.Where(ww => ww.CreatedDate.TimeOfDay >= dtFromFilter.TimeOfDay && ww.CreatedDate.TimeOfDay <= dtToFilter.TimeOfDay).ToList();
                        break;
                    case (int)Commons.EFilterType.Days:
                        query = query.Where(ww => ww.CreatedDate.TimeOfDay >= dtFromFilter.TimeOfDay || ww.CreatedDate.TimeOfDay <= dtToFilter.TimeOfDay).ToList();
                        break;
                }
                if (query != null && query.Any())
                {
                    lstData = query.GroupBy(gg => new
                        {
                            Hour = gg.CreatedDate.Hour,
                            StoreId = gg.StoreId,
                        })
                        .Select(g => new DiscountAndMiscReportModels
                        {
                            StoreId = g.Key.StoreId,
                            Hour = g.Key.Hour,
                            TimeSpanHour = new TimeSpan(g.Key.Hour, 0, 0),
                            //CreatedDate = g.Key.Date.Value,
                            DiscountValue = g.Sum(ss => ss.DiscountValue),
                            MiscValue = g.Sum(ss => ss.MiscValue)
                        }).ToList();
                }


                return lstData;
            }
        }

        public List<DiscountAndMiscReportModels> GetReceiptDiscountAndMiscByDateTime(DateTime fromDate, DateTime toDate, List<string> StoreIds, int mode)
        {
            using (var cxt = new NuWebContext())
            {
                var lstData = new List<DiscountAndMiscReportModels>();
                var query = (from m in cxt.R_DiscountAndMiscReport
                             where StoreIds.Contains(m.StoreId) && m.CreatedDate >= fromDate && m.CreatedDate <= toDate
                             && m.Mode == mode
                             group m by new
                             {
                                 Date = DbFunctions.TruncateTime(m.CreatedDate),
                                 Hour = (int?)SqlFunctions.DatePart("HH", m.CreatedDate),
                                 StoreId = m.StoreId,
                             } into g
                             select g).ToList();
                if (query != null && query.Any())
                {
                    lstData = query.Select(g => new DiscountAndMiscReportModels
                    {
                        StoreId = g.Key.StoreId,
                        Hour = g.Key.Hour.Value,
                        TimeSpanHour = new TimeSpan(g.Key.Hour.Value, 0, 0),
                        CreatedDate = g.Key.Date.Value,
                        DiscountValue = g.Sum(ss => ss.DiscountValue),
                        MiscValue = g.Sum(ss => ss.MiscValue)
                    }).ToList();
                }
                return lstData;
            }
        }

        public List<DiscountAndMiscReportModels> GetMiscs(BaseReportModel model)
        {
            using (var cxt = new NuWebContext())
            {
                //var lstData = new List<DiscountAndMiscReportModels>();
                var lstData = (from m in cxt.R_DiscountAndMiscReport
                               where model.ListStores.Contains(m.StoreId) && m.CreatedDate >= model.FromDate && m.CreatedDate <= model.ToDate
                               && m.Mode == model.Mode
                               && m.MiscValue > 0
                               select new DiscountAndMiscReportModels
                               {
                                   StoreId = m.StoreId,
                                   CreatedDate = m.CreatedDate,
                                   MiscValue = m.MiscValue
                               }).ToList();
                return lstData;
            }

        }
    }
}

