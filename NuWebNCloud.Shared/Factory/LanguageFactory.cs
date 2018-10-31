using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Api.Language;
using NuWebNCloud.Shared.Models.Sandbox;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace NuWebNCloud.Shared.Factory
{
    public class LanguageFactory : BaseFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public LanguageFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<LanguageModels> GetListLanguage()
        {
            List<LanguageModels> listLanguage = new List<LanguageModels>();
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    listLanguage = (from l in cxt.G_Language
                                    where l.Status != (byte)Commons.EStatus.Deleted
                                    orderby l.Name
                                    select new LanguageModels
                                    {
                                        Id = l.Id,
                                        IsDefault = l.IsDefault,
                                        Name = l.Name,
                                        Symbol = l.Symbol
                                    }).ToList();

                    NSLog.Logger.Info("Insert [Get List Language] data success");
                }
                catch (Exception ex)
                {
                    NSLog.Logger.Error("Insert [Get List Language] data fail", ex);
                    //_logger.Error(ex.Message);
                }
                finally
                {

                }
            }
            return listLanguage;
        }

        public List<LanguageLinkDetailModels> GetListLanguageForText(string LanguageId)
        {
            List<LanguageLinkDetailModels> listLanguage = new List<LanguageLinkDetailModels>();
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    listLanguage = (from ld in cxt.G_LanguageLinkDetail
                                    join ll in cxt.G_LanguageLink on ld.LanguageLinkId equals ll.Id
                                    where ld.Status == (int)Commons.EStatus.Actived && ld.LanguageId.Equals(LanguageId)
                                    //&& ll.Name== "Inventory Management"
                                    select new LanguageLinkDetailModels
                                    {
                                        Id = ld.Id,
                                        LanguageLinkName = ll.Name,
                                        Text = ld.Text
                                    }).ToList();

                    //listLanguage = (from l in cxt.G_Language
                    //                join lld in cxt.G_LanguageLinkDetail on l.Id equals lld.LanguageId
                    //                join ll in cxt.G_LanguageLink on lld.LanguageLinkId equals ll.Id
                    //                where l.Status != (byte)Commons.EStatus.Deleted && lld.Status != (byte)Commons.EStatus.Deleted
                    //                         && ll.Status != (byte)Commons.EStatus.Deleted && lld.LanguageId.Equals(LanguageId)
                    //                select new LanguageLinkDetailModels
                    //                {
                    //                    Id = lld.Id,
                    //                    LanguageName = l.Name,
                    //                    LanguageLinkName = ll.Name,
                    //                    Text = lld.Text
                    //                }).ToList();

                    NSLog.Logger.Info("Insert [Get List Language For Text] data success");
                }
                catch (Exception ex)
                {
                    NSLog.Logger.Error("Insert [Get List Language For Text] data fail", ex);
                    //_logger.Error(ex.Message);
                }
                finally
                {

                }
            }
            return listLanguage;
        }

        public bool Insert(LanguageModels info)
        {
            bool result = false;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        var isExsit = cxt.G_Language.Any(x => x.Symbol.ToLower().Equals(info.Symbol.ToLower()));
                        if (isExsit)
                            return result;

                        G_Language item = new G_Language();
                        item.Id = Guid.NewGuid().ToString();
                        item.Name = info.Name;
                        item.Symbol = info.Symbol;
                        item.Status = (byte)Commons.EStatus.Actived;
                        item.IsDefault = info.IsDefault;
                        item.CreatedDate = DateTime.Now;
                        item.LastModified = DateTime.Now;
                        item.CreatedUser = info.CreatedUser;
                        item.ModifiedUser = info.ModifiedUser;
                        cxt.G_Language.Add(item);
                        cxt.SaveChanges();
                        transaction.Commit();

                        result = true;

                        NSLog.Logger.Info("Insert [Insert Language] data success", info);

                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert [Insert Language] data fail", ex);
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
            //_baseFactory.InsertTrackingLog("G_POSAPIMerchantConfig", jsonContent, info.NuPOSInstance, result);
            return result;
        }

        public bool Update(LanguageModels info, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var itemUpdate = cxt.G_Language.Where(x => x.Id.Equals(info.Id)).FirstOrDefault();
                    if (itemUpdate != null)
                    {
                        itemUpdate.Name = info.Name;
                        itemUpdate.Symbol = info.Symbol;
                        itemUpdate.Status = info.Status;
                        itemUpdate.IsDefault = info.IsDefault;
                        itemUpdate.LastModified = DateTime.Now;
                        itemUpdate.ModifiedUser = info.ModifiedUser;

                        cxt.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    msg = "Can not update language";
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

        /*Import */

        public ImportModel Import(string filePath, string _LanguageId, string _CreatedUser, ref string msg)
        {
            ImportModel importItems = new ImportModel();
            DataTable dtLang = ReadExcelFile(filePath, "Sheet1");
            ImportItem itemErr = null;
            bool flagInsert = true;
            string msgError = "";
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        List<G_LanguageLink> listInsert = new List<G_LanguageLink>();
                        List<G_LanguageLinkDetail> listInsertDetail = new List<G_LanguageLinkDetail>();
                        int _index = 0;

                        foreach (DataRow item in dtLang.Rows)
                        {
                            flagInsert = true;
                            msgError = "";
                            if (_index == 0)
                            {
                                foreach (var itemIndex in item.ItemArray)
                                {
                                    if (!string.IsNullOrEmpty(itemIndex.ToString()))
                                    {
                                        if (itemIndex.ToString().Equals(_LanguageId))
                                        {
                                            break;
                                        }
                                    }
                                    _index++;
                                }
                            }
                            if (item[1].ToString().Equals("") || item[1].ToString().ToLower().Equals("key"))
                                continue;

                            string KEY = item[1].ToString().Trim();
                            string LanguageLinkId = Guid.NewGuid().ToString();
                            var ListObjKey = cxt.G_LanguageLink.Where(x => x.Name == KEY).ToList();
                            G_LanguageLink ObjKey = null;
                            ListObjKey.ForEach(x =>
                            {
                                if (x.Name.Equals(KEY))
                                {
                                    ObjKey = new G_LanguageLink();
                                    ObjKey = x;
                                }
                            });
                            if (ObjKey != null)
                            {
                                LanguageLinkId = ObjKey.Id;
                            }
                            else
                            {
                                listInsert.Add(new G_LanguageLink()
                                {
                                    Id = LanguageLinkId,
                                    Name = KEY,
                                    Status = (byte)Commons.EStatus.Actived,
                                    CreatedDate = DateTime.Now,
                                    LastModified = DateTime.Now,
                                    CreatedUser = _CreatedUser,
                                    ModifiedUser = _CreatedUser
                                });
                            }
                            string Text = item[_index].ToString();
                            var ObjText = cxt.G_LanguageLinkDetail.Where(x => x.LanguageId.Equals(_LanguageId) && x.LanguageLinkId.Equals(LanguageLinkId)).FirstOrDefault();
                            if (ObjText != null)
                            {
                                flagInsert = false;
                                msgError = "Text [<strong>" + Text + "</strong>] is exists !!!!";
                            }
                            if (string.IsNullOrEmpty(Text))
                            {
                                flagInsert = false;
                                msgError += "<br/> => [<strong style=\"color:#d9534f;\">" + KEY + "</strong>] | Text [<strong>" + Text + "</strong>] is emppty !!!!";
                            }
                            //=====
                            if (flagInsert)
                            {
                                listInsertDetail.Add(new G_LanguageLinkDetail
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    LanguageId = _LanguageId,
                                    LanguageLinkId = LanguageLinkId,
                                    Text = Text,
                                    //=============
                                    Status = (byte)Commons.EStatus.Actived,
                                    CreatedDate = DateTime.Now,
                                    LastModified = DateTime.Now,
                                    CreatedUser = _CreatedUser,
                                    ModifiedUser = _CreatedUser
                                });
                            }
                            else
                            {
                                itemErr = new ImportItem();
                                itemErr.Name = KEY;
                                itemErr.ListFailStoreName.Add("");
                                itemErr.ListErrorMsg.Add("KEY [<strong>" + KEY + "</strong>]: " + msgError);
                                importItems.ListImport.Add(itemErr);
                            }
                            //====
                            //break;
                        }
                        //=========
                        cxt.G_LanguageLink.AddRange(listInsert);
                        cxt.G_LanguageLinkDetail.AddRange(listInsertDetail);

                        cxt.SaveChanges();
                        transaction.Commit();

                        NSLog.Logger.Info("Import [Insert Language] data success");

                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Import [Insert Language] data fail", ex);
                        transaction.Rollback();
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                    }
                }
            }
            //=============
            if (importItems.ListImport.Count == 0)
            {
                ImportItem item = new ImportItem();
                item.Name = "Language";
                item.ListSuccessStoreName.Add("Import Language Successful");
                importItems.ListImport.Add(item);
            }
            return importItems;
        }

        #region Get languages from api
        public List<LanguageModels> GetLanguages()
        {
            List<LanguageModels> lstLanguages = new List<LanguageModels>();
            
                try
                {
                    int langTypeNum = 4;
                    string urlLang = ConfigurationManager.AppSettings["LanguageApi"];
                    try
                    {
                        var langType = ConfigurationManager.AppSettings["LanguageType"];
                        langTypeNum = int.Parse(langType);
                    }
                    catch
                    { }
                   
                    RequestGetLanguage paraBody = new RequestGetLanguage();
                    paraBody.Type = langTypeNum;

                    var result = (ResponseGetLanguage)ApiResponse.PostWithoutHostConfig<ResponseGetLanguage>(urlLang + "/" +Commons.Language_Get, null, paraBody);
  
                    lstLanguages = result.ListData;

                    NSLog.Logger.Info("GetLanguages data success", lstLanguages);
                }
                catch (Exception ex)
                {
                    NSLog.Logger.Error("GetLanguagesdata fail", ex);
                }
                finally
                {

                }
            return lstLanguages;
        }
        public List<LanguageLinkDetailModels> GetLanguageDataToTrans(string LanguageId)
        {
            List<LanguageLinkDetailModels> lstLanguageDetail = new List<LanguageLinkDetailModels>();
   
                try
                {
                    int langTypeNum = 4;
                    string urlLang = ConfigurationManager.AppSettings["LanguageApi"];
                    try
                    {
                        var langType = ConfigurationManager.AppSettings["LanguageType"];
                        langTypeNum = int.Parse(langType);
                    }
                    catch
                    { }

                    RequestGetLanguage paraBody = new RequestGetLanguage();
                    paraBody.Type = langTypeNum;
                    paraBody.LanguageID = LanguageId;

                    var result = (ResponseGetLanguage)ApiResponse.PostWithoutHostConfig<ResponseGetLanguage>(urlLang + "/" + Commons.Language_Get, null, paraBody);

                    LanguageModels language = result.ListData.FirstOrDefault();

                    if (language != null)
                    {
                        lstLanguageDetail = (from ld in language.ListText
                                             select new LanguageLinkDetailModels
                                             {
                                                 Id = ld.KeyID,
                                                 LanguageLinkName = ld.KeyName,
                                                 Text = ld.Text
                                             }).ToList();

                    }
                    NSLog.Logger.Info("GetLanguageDataToTrans data success");
                }
                catch (Exception ex)
                {
                    NSLog.Logger.Error("GetLanguageDataToTrans data fail", ex);
                }
                finally
                {

                }

            return lstLanguageDetail;
        }
        public List<LanguageLinkDetailModels> GetLanguageDataToTranslateForLogin(string LanguageId)
        {
            List<LanguageLinkDetailModels> lstLanguageDetail = new List<LanguageLinkDetailModels>();

            try
            {
                int langTypeNum = 4;
                string urlLang = ConfigurationManager.AppSettings["LanguageApi"];
                try
                {
                    var langType = ConfigurationManager.AppSettings["LanguageType"];
                    langTypeNum = int.Parse(langType);
                }
                catch
                { }

                RequestGetLanguage paraBody = new RequestGetLanguage();
                paraBody.Type = langTypeNum;
                paraBody.LanguageID = LanguageId;
                paraBody.ListKey = new List<string>() {"Login", "Log in", "Email", "Password" ,"Remember me", "Version"};

                var result = (ResponseGetLanguage)ApiResponse.PostWithoutHostConfig<ResponseGetLanguage>(urlLang + "/" + Commons.Language_Get, null, paraBody);

                LanguageModels language = result.ListData.FirstOrDefault();

                if (language != null)
                {
                    lstLanguageDetail = (from ld in language.ListText
                                         select new LanguageLinkDetailModels
                                         {
                                             Id = ld.KeyID,
                                             LanguageLinkName = ld.KeyName,
                                             Text = ld.Text
                                         }).ToList();

                }
                NSLog.Logger.Info("GetLanguageDataToTranslateForLogin data success");
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("GetLanguageDataToTranslateForLogin data fail", ex);
            }
            finally
            {

            }

            return lstLanguageDetail;
        }
        #endregion End get language from api

    }
}
