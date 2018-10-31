using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Models.Ingredients;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class DataEntryDetailFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public DataEntryDetailFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public bool Insert(List<DataEntryDetailModels> models, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    List<I_DataEntryDetail> listInsert = new List<I_DataEntryDetail>();
                    I_DataEntryDetail item = null;
                    foreach (var model in models)
                    {
                        item = new I_DataEntryDetail();
                        item.Id = Guid.NewGuid().ToString();
                        item.DataEntryId = model.DataEntryId;
                        item.IngredientId = model.IngredientId;
                        //item.CloseBal = 0;//model.CloseBal;

                        listInsert.Add(item);
                    }

                    cxt.I_DataEntryDetail.AddRange(listInsert);
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

        public bool Update(DataEntryDetailModels model, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var itemUpdate = (from tb in cxt.I_DataEntryDetail
                                      where tb.Id == model.Id
                                      select tb).FirstOrDefault();

                    itemUpdate.DataEntryId = model.DataEntryId;
                    itemUpdate.IngredientId = model.IngredientId;
                    //itemUpdate.CloseBal = 0; //model.CloseBal;

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

        public bool Delete(string Id, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    I_DataEntryDetail itemDelete = (from tb in cxt.I_DataEntryDetail
                                                    where tb.Id == Id
                                                    select tb).FirstOrDefault();
                    //cxt.I_DataEntryDetail.Remove(itemDelete);
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

        public List<DataEntryDetailModels> GetData(DataEntryViewModels model, List<string> listStoreId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from tb in cxt.I_DataEntryDetail
                                     from i in cxt.I_Ingredient
                                     where tb.IngredientId.Equals(i.Id)
                                     select new DataEntryDetailModels()
                                     {
                                         Id = tb.Id,
                                         DataEntryId = tb.DataEntryId,
                                         IngredientId = tb.IngredientId,
                                         IngredientCode = i.Code,
                                         IngredientName = i.Name,

                                         //CloseBal = 0 //tb.CloseBal,

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

        public DataEntryDetailModels GetDetail(string ID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var model = (from tb in cxt.I_DataEntryDetail
                                 from i in cxt.I_Ingredient
                                 where tb.Id == ID && tb.IngredientId.Equals(i.Id)
                                 select new DataEntryDetailModels()
                                 {
                                     Id = tb.Id,
                                     DataEntryId = tb.DataEntryId,
                                     IngredientId = tb.IngredientId,
                                     IngredientCode = i.Code,
                                     IngredientName = i.Name,
                                     //CloseBal = 0
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
    }
}
