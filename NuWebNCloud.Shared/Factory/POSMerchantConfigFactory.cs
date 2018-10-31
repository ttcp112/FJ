using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory
{
    public class POSMerchantConfigFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public POSMerchantConfigFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public async Task<ResponeMerchantConfig> GetListMerchantConfig()
        {
            ResponeMerchantConfig result = new ResponeMerchantConfig();
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var listPOSAPIMerchantConfig = (from merchant in cxt.G_POSAPIMerchantConfig
                                                    where merchant.IsActived
                                                    select new MerchantConfigApiModels
                                                    {
                                                        BreakfastEnd = merchant.BreakfastEnd,
                                                        BreakfastStart = merchant.BreakfastStart,
                                                        DinnerEnd = merchant.DinnerEnd,
                                                        DinnerStart = merchant.DinnerStart,
                                                        FTPHost = merchant.FTPHost,
                                                        FTPPassword = merchant.FTPPassword,
                                                        FTPUser = merchant.FTPUser,
                                                        Id = merchant.Id,
                                                        ImageBaseUrl = merchant.ImageBaseUrl,
                                                        LunchEnd = merchant.LunchEnd,
                                                        LunchStart = merchant.LunchStart,
                                                        //MerchantName = merchant.MerchantName,
                                                        POSAPIUrl = merchant.POSAPIUrl,
                                                        IsActived = merchant.IsActived,
                                                        CreatedDate = merchant.CreatedDate,
                                                        POSInstanceVersion = merchant.POSInstanceVersion,

                                                        MorningStart = merchant.MorningStart.HasValue? merchant.MorningStart.Value: new TimeSpan(),
                                                        MorningEnd = merchant.MorningEnd.HasValue? merchant.MorningEnd.Value: new TimeSpan(),
                                                        MidDayStart = merchant.MidDayStart.HasValue? merchant.MidDayStart.Value: new TimeSpan(),
                                                        MidDayEnd = merchant.MidDayEnd.HasValue? merchant.MidDayEnd.Value: new TimeSpan(),
                                                    }).ToList();
                    var listPOSEmployeeConfig = (from employee in cxt.G_POSEmployeeConfig
                                                 where employee.IsActived
                                                 select new EmployeeConfigApiModels
                                                 {
                                                     Id = employee.Id,
                                                     IsActived = employee.IsActived,
                                                     UserName = employee.UserName,
                                                     POSAPIMerchantConfigId = employee.POSAPIMerchantConfigId,
                                                     CreatedDate = employee.CreatedDate
                                                 }).ToList();

                    listPOSAPIMerchantConfig.ForEach(x =>
                    {
                        x.ListEmployees = listPOSEmployeeConfig.Where(z => z.POSAPIMerchantConfigId.Equals(x.Id)).ToList();
                    });
                    result.IsOk = true;
                    result.Data = listPOSAPIMerchantConfig;

                    NSLog.Logger.Info("Insert [Get List Merchant Config] data success");
                }
                catch (Exception ex)
                {
                    NSLog.Logger.Error("Insert [Get List Merchant Config] data fail", ex);
                    //_logger.Error(ex.Message);
                }
                finally
                {

                }
            }
            return result;
        }

        /*Merchant*/
        public ResponeMerchantConfig Insert(MerchantConfigApiModels info)
        {
            ResponeMerchantConfig result = new ResponeMerchantConfig();
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        var isExsitMerchant = cxt.G_POSAPIMerchantConfig.Any(x => x.POSAPIUrl.ToLower().Equals(info.POSAPIUrl.ToLower()));
                        if (isExsitMerchant)
                        {
                            NSLog.Logger.Info(string.Format("The merchant [{0}] already exist", info.POSAPIUrl));
                            return result;
                        }

                        G_POSAPIMerchantConfig itemMerchantConfig = new G_POSAPIMerchantConfig();
                        /*string POSAPIMerchantConfigId =Guid.NewGuid().ToString();*/
                        string POSAPIMerchantConfigId = string.Empty;
                        if (string.IsNullOrEmpty(info.Id))
                            POSAPIMerchantConfigId = Guid.NewGuid().ToString();
                        else
                            POSAPIMerchantConfigId = info.Id;
                        itemMerchantConfig.Id = POSAPIMerchantConfigId;
                        itemMerchantConfig.NuPOSInstance = info.NuPOSInstance;
                        itemMerchantConfig.POSAPIUrl = info.POSAPIUrl;
                        itemMerchantConfig.FTPHost = info.FTPHost;
                        itemMerchantConfig.FTPUser = info.FTPUser;
                        itemMerchantConfig.FTPPassword = info.FTPPassword;
                        itemMerchantConfig.ImageBaseUrl = info.ImageBaseUrl;

                        itemMerchantConfig.BreakfastStart = info.BreakfastStart;
                        itemMerchantConfig.BreakfastEnd = info.BreakfastEnd;
                        itemMerchantConfig.LunchStart = info.LunchStart;
                        itemMerchantConfig.LunchEnd = info.LunchEnd;
                        itemMerchantConfig.DinnerStart = info.DinnerStart;
                        itemMerchantConfig.DinnerEnd = info.DinnerEnd;
                        itemMerchantConfig.CreatedDate = DateTime.Now;
                        itemMerchantConfig.IsActived = true;
                        itemMerchantConfig.POSInstanceVersion = info.POSInstanceVersion;

                        itemMerchantConfig.MorningStart = info.MorningStart;
                        itemMerchantConfig.MorningEnd = info.MorningEnd;
                        itemMerchantConfig.MidDayStart = info.MidDayStart;
                        itemMerchantConfig.MidDayEnd = info.MidDayEnd;
                        List<G_POSEmployeeConfig> lstInsertEmployee = new List<G_POSEmployeeConfig>();
                        G_POSEmployeeConfig itemEmpInsert = null;
                        foreach (var item in info.ListEmployees)
                        {
                            itemEmpInsert = new G_POSEmployeeConfig();
                            if (string.IsNullOrEmpty(item.Id))
                                itemEmpInsert.Id = Guid.NewGuid().ToString();
                            else
                                itemEmpInsert.Id = item.Id;
                            //itemEmpInsert.Id = Guid.NewGuid().ToString();
                            itemEmpInsert.POSAPIMerchantConfigId = POSAPIMerchantConfigId;
                            itemEmpInsert.UserName = item.UserName;
                            itemEmpInsert.Password = CommonHelper.GetSHA512(item.Password);
                            itemEmpInsert.CreatedDate = DateTime.Now;
                            itemEmpInsert.IsActived = true;
                            lstInsertEmployee.Add(itemEmpInsert);
                        }
                        cxt.G_POSAPIMerchantConfig.Add(itemMerchantConfig);
                        cxt.G_POSEmployeeConfig.AddRange(lstInsertEmployee);
                        cxt.SaveChanges();
                        transaction.Commit();

                        result.IsOk = true;
                        result.POSIntanceID = itemMerchantConfig.Id;

                        NSLog.Logger.Info("Insert [Insert Pos Api Merchant Config] data success", info);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert [Insert Pos Api Merchant Config] data fail", ex);
                        //_logger.Error(ex);
                        result.IsOk = false;
                        transaction.Rollback();
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                    }
                }
            }
            //var jsonContent = JsonConvert.SerializeObject(info);
            //_baseFactory.InsertTrackingLog("G_POSAPIMerchantConfig", jsonContent, info.NuPOSInstance, result);

            return result;
        }

        //2018-01-02
        public ResponeMerchantConfig RegisterAccountFromCSC(MerchantConfigApiModels info)
        {
            ResponeMerchantConfig result = new ResponeMerchantConfig();
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        var merchant = cxt.G_POSAPIMerchantConfig.Where(x => x.POSAPIUrl == info.POSAPIUrl && x.IsActived).FirstOrDefault();
                        if (merchant != null)
                        {
                            List<G_POSEmployeeConfig> lstInsertEmployee = new List<G_POSEmployeeConfig>();
                            G_POSEmployeeConfig itemEmpInsert = null;
                            foreach (var item in info.ListEmployees)
                            {
                                itemEmpInsert = new G_POSEmployeeConfig();
                                if (string.IsNullOrEmpty(item.Id))
                                    itemEmpInsert.Id = Guid.NewGuid().ToString();
                                else
                                    itemEmpInsert.Id = item.Id;
                               itemEmpInsert.POSAPIMerchantConfigId = merchant.Id;
                                itemEmpInsert.UserName = item.UserName;
                                itemEmpInsert.Password = item.Password;
                                itemEmpInsert.CreatedDate = DateTime.Now;
                                itemEmpInsert.IsActived = true;
                                lstInsertEmployee.Add(itemEmpInsert);
                            }
                            cxt.G_POSEmployeeConfig.AddRange(lstInsertEmployee);
                        }
                        else
                        {

                            G_POSAPIMerchantConfig itemMerchantConfig = new G_POSAPIMerchantConfig();
  
                            itemMerchantConfig.Id = info.Id;
                            itemMerchantConfig.NuPOSInstance = info.NuPOSInstance;
                            itemMerchantConfig.POSAPIUrl = info.POSAPIUrl;
                            itemMerchantConfig.FTPHost = info.FTPHost;
                            itemMerchantConfig.FTPUser = info.FTPUser;
                            itemMerchantConfig.FTPPassword = info.FTPPassword;
                            itemMerchantConfig.ImageBaseUrl = info.ImageBaseUrl;

                            itemMerchantConfig.BreakfastStart = info.BreakfastStart;
                            itemMerchantConfig.BreakfastEnd = info.BreakfastEnd;
                            itemMerchantConfig.LunchStart = info.LunchStart;
                            itemMerchantConfig.LunchEnd = info.LunchEnd;
                            itemMerchantConfig.DinnerStart = info.DinnerStart;
                            itemMerchantConfig.DinnerEnd = info.DinnerEnd;
                            itemMerchantConfig.CreatedDate = DateTime.Now;
                            itemMerchantConfig.IsActived = true;
                            itemMerchantConfig.POSInstanceVersion = info.POSInstanceVersion;

                            itemMerchantConfig.MorningStart = info.MorningStart;
                            itemMerchantConfig.MorningEnd = info.MorningEnd;
                            itemMerchantConfig.MidDayStart = info.MidDayStart;
                            itemMerchantConfig.MidDayEnd = info.MidDayEnd;
                            List<G_POSEmployeeConfig> lstInsertEmployee = new List<G_POSEmployeeConfig>();
                            G_POSEmployeeConfig itemEmpInsert = null;
                            foreach (var item in info.ListEmployees)
                            {
                                itemEmpInsert = new G_POSEmployeeConfig();
                                if (string.IsNullOrEmpty(item.Id))
                                    itemEmpInsert.Id = Guid.NewGuid().ToString();
                                else
                                    itemEmpInsert.Id = item.Id;
                                //itemEmpInsert.Id = Guid.NewGuid().ToString();
                                itemEmpInsert.POSAPIMerchantConfigId = info.Id;
                                itemEmpInsert.UserName = item.UserName;
                                itemEmpInsert.Password = item.Password;
                                itemEmpInsert.CreatedDate = DateTime.Now;
                                itemEmpInsert.IsActived = true;
                                lstInsertEmployee.Add(itemEmpInsert);
                            }
                            cxt.G_POSAPIMerchantConfig.Add(itemMerchantConfig);
                            cxt.G_POSEmployeeConfig.AddRange(lstInsertEmployee);
                        }
                        cxt.SaveChanges();
                        transaction.Commit();

                        result.IsOk = true;
                        result.POSIntanceID = info.Id;

                        NSLog.Logger.Info("Insert [RegisterAccountFromCSC] data success", info);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert [RegisterAccountFromCSC] data fail", ex);
                        //_logger.Error(ex);
                        result.IsOk = false;
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
        /*Employee*/
        public bool InsertEmployee(EmployeeConfigApiModels info)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        G_POSEmployeeConfig itemInsert = null;
                        itemInsert = new G_POSEmployeeConfig();
                        if (string.IsNullOrEmpty(info.Id))
                            itemInsert.Id = Guid.NewGuid().ToString();
                        else
                            itemInsert.Id = info.Id;
                        itemInsert.POSAPIMerchantConfigId = info.POSAPIMerchantConfigId;
                        itemInsert.UserName = info.UserName;
                        itemInsert.Password = CommonHelper.GetSHA512(info.Password);
                        itemInsert.CreatedDate = DateTime.Now;
                        itemInsert.IsActived = true;
                        cxt.G_POSEmployeeConfig.Add(itemInsert);
                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("Insert [Insert Pos Api Employee Config] data success", info);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert [Insert Pos Api Employee Config] data fail", ex);
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
            //var jsonContent = JsonConvert.SerializeObject(info);
            //_baseFactory.InsertTrackingLog("G_POSEmployeeConfig", jsonContent, info.POSAPIMerchantConfigId, result);
            return result;
        }

        public bool UpdateEmployee(EmployeeConfigApiModels info)
        {
            bool result = true;

            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var itemUpdate = cxt.G_POSEmployeeConfig.Where(x => x.UserName.ToLower().Equals(info.UserName.ToLower())
                    && x.POSAPIMerchantConfigId == info.POSAPIMerchantConfigId).FirstOrDefault();
                    if(itemUpdate== null)
                    {
                        itemUpdate  = cxt.G_POSEmployeeConfig.Where(x => x.UserName.ToLower().Equals(info.UserName.ToLower())).FirstOrDefault();
                    }
                   if(itemUpdate != null)
                    {
                        itemUpdate.Password = info.Password;
                        itemUpdate.IsActived = info.IsActived;
                          cxt.SaveChanges();
                    }
                    NSLog.Logger.Info("UpdateEmployee", itemUpdate);
                }
                catch (Exception ex)
                {
                    NSLog.Logger.Error("UpdateEmployee", ex);
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

        public MerchantConfigApiModels GetValueCommons(string email, string password, bool isExtendUrl)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    if(!isExtendUrl)
                    password = CommonHelper.GetSHA512(password);
                    var model = (from merchant in cxt.G_POSAPIMerchantConfig
                                 from employee in cxt.G_POSEmployeeConfig
                                 where merchant.Id.Equals(employee.POSAPIMerchantConfigId)
                                 where merchant.IsActived 
                                        && employee.IsActived
                                        && employee.UserName.ToLower().Equals(email.ToLower())
                                        && employee.Password == password
                                 select new MerchantConfigApiModels
                                 {
                                     BreakfastEnd = merchant.BreakfastEnd,
                                     BreakfastStart = merchant.BreakfastStart,
                                     DinnerEnd = merchant.DinnerEnd,
                                     DinnerStart = merchant.DinnerStart,
                                     FTPHost = merchant.FTPHost,
                                     FTPPassword = merchant.FTPPassword,
                                     FTPUser = merchant.FTPUser,
                                     Id = merchant.Id,
                                     ImageBaseUrl = merchant.ImageBaseUrl,
                                     LunchEnd = merchant.LunchEnd,
                                     LunchStart = merchant.LunchStart,
                                     NuPOSInstance = merchant.NuPOSInstance,
                                     POSAPIUrl = merchant.POSAPIUrl,
                                     POSInstanceVersion = merchant.POSInstanceVersion,
                                     EmpConfigId = employee.Id,
                                     WebHostUrl = merchant.WebHostUrl
                                 }).FirstOrDefault();

                    NSLog.Logger.Info("GetValueCommons", model);
                    return model;
                }
                catch (Exception ex)
                {
                    NSLog.Logger.Error("GetValueCommons Error",ex);
                    return null;
                }
            }
        }

        public MerchantConfigApiModels GetTimeSettingForFJDailySale(string urlMerchant)
        {
            using (var cxt =  new NuWebContext())
            {
                try
                {
                    var result = cxt.G_POSAPIMerchantConfig.Where(ww => ww.POSAPIUrl == urlMerchant)
                        .Select(ss=> new MerchantConfigApiModels() {
                            Id = ss.Id,
                            POSAPIUrl = ss.POSAPIUrl,
                            MorningStart = ss.MorningStart.HasValue? ss.MorningStart.Value: new TimeSpan(7,0,0),
                            MorningEnd = ss.MorningEnd.HasValue ? ss.MorningEnd.Value : new TimeSpan(11, 0, 0),
                            MidDayStart = ss.MidDayStart.HasValue ? ss.MidDayStart.Value : new TimeSpan(11, 0, 0),
                            MidDayEnd = ss.MidDayEnd.HasValue ? ss.MidDayEnd.Value : new TimeSpan(16, 0, 0)
                        })
                        .FirstOrDefault();

                    NSLog.Logger.Info("GetTimeSettingForFJDailySale", result);
                    return result;
                }
                catch (Exception ex)
                {
                    NSLog.Logger.Error("GetTimeSettingForFJDailySale", ex);
                    return new MerchantConfigApiModels();
                }
            }
        }
        public bool UpdateTimeSettingForFJDailySale(MerchantConfigApiModels model)
        {
            using (var cxt = new NuWebContext())
            {
                try
                {
                    var result = cxt.G_POSAPIMerchantConfig.Where(ww => ww.Id == model.Id)
                        .FirstOrDefault();
                    if (result != null)
                    {
                        result.MorningStart = model.MorningStart;
                        result.MorningEnd = model.MorningEnd;
                        result.MidDayStart = model.MidDayStart;
                        result.MidDayEnd = model.MidDayEnd;

                        cxt.SaveChanges();
                    }

                    NSLog.Logger.Info("UpdateTimeSettingForFJDailySale", result);
                    return true;
                }
                catch (Exception ex)
                {
                    NSLog.Logger.Error("GetTimeSettingForFJDailySale", ex);
                    return false;
                }
            }
        }
    }

}
