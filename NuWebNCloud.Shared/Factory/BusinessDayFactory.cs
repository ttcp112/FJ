using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Factory.Ingredients;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory
{
    public class BusinessDayFactory : ReportFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private DataEntryFactory _dataEntryFactory = null;
        private BaseFactory _baseFactory = null;
        public BusinessDayFactory()
        {
            _baseFactory = new BaseFactory();
            _dataEntryFactory = new DataEntryFactory();
        }

        public bool Insert(List<BusinessDayModels> lstInfo)
        {
            bool result = true;
            var info = lstInfo.FirstOrDefault();
            //_logger.Info("==========================================================");
            //_logger.Info(string.Format("Insert BusinessDay: StoreId: [{0}] | BusinessId: [{1}]", info.StoreId, info.Id));

            using (NuWebContext cxt = new NuWebContext())
            {
                //Check Exist
                var obj = cxt.G_BusinessDay.Where(ww => ww.Id == info.Id).FirstOrDefault();
                if (obj != null)
                {
                    NSLog.Logger.Info("Insert Business Day data exist", lstInfo);
                    return result;
                }
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<G_BusinessDay> lstInsert = new List<G_BusinessDay>();
                        G_BusinessDay itemInsert = null;
                        var business = lstInfo.FirstOrDefault(); 
                        foreach (var item in lstInfo)
                        {
                            itemInsert = new G_BusinessDay();
                            itemInsert.Id = item.Id;
                            itemInsert.StoreId = item.StoreId;
                            itemInsert.StartedOn = item.StartedOn;
                            itemInsert.ClosedOn = item.ClosedOn;
                            itemInsert.CreatedDate = item.CreatedDate;
                            itemInsert.CreatedUser = item.CreatedUser;
                            itemInsert.LastDateModified = item.LastDateModified;
                            itemInsert.LastUserModified = item.LastUserModified;
                            itemInsert.Mode = item.Mode;

                            lstInsert.Add(itemInsert);
                        }
                        cxt.G_BusinessDay.AddRange(lstInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        //_logger.Info(string.Format("Insert BusinessDay: StoreId: [{0}] | BusinessId: [{1}] Success", info.StoreId, info.Id));
                        NSLog.Logger.Info("Insert Business Day data success", lstInfo);
                        //AutoCreate DataEntry
                        //Task.Run(() => _dataEntryFactory.AutoCreatedDataEntry(business.StoreId, business.Id));
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Info("Insert Business Day data fail", lstInfo);
                        NSLog.Logger.Error("Insert Business Day data fail", ex);
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
            //_baseFactory.InsertTrackingLog("G_BusinessDay", jsonContent, info.StoreId.ToString(), result);

            return result;
        }

        public List<BusinessDayModels> GetDataForStore(string StoreId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from tb in cxt.G_BusinessDay
                                     where tb.StoreId.Equals(StoreId)
                                     orderby tb.StartedOn descending
                                     select new BusinessDayModels()
                                     {
                                         Id = tb.Id,
                                         StartedOn = tb.StartedOn,
                                         ClosedOn = tb.ClosedOn,
                                         StoreId = tb.StoreId
                                     }).ToList();
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }
        public List<ShiftHistoryDTO> GetDataForStoreOnApi(string StoreId)
        {
            List<ShiftHistoryDTO> listData = new List<ShiftHistoryDTO>();
            try
            {
                GetBusinessDayHistoryModelRequest paraBody = new GetBusinessDayHistoryModelRequest();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.PageIndex = 1;
                paraBody.StoreId = StoreId;
                paraBody.PageSize = 2;
                paraBody.Mode = "1";
                paraBody.From = CommonHelper.ConvertToUTCTime(Commons.MinDate);
                paraBody.To = CommonHelper.ConvertToUTCTime(Commons.MinDate);

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.BusinessDay_Get_History, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListShiftHistory"];
                var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                listData = JsonConvert.DeserializeObject<List<ShiftHistoryDTO>>(lstContent);
                return listData;
            }
            catch (Exception e)
            {
                _logger.Error("Business_GetList: " + e);
                return listData;
            }

        }
    }
}
