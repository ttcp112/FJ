using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models.Ingredients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class ReturnNoteDetailFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public ReturnNoteDetailFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public bool Insert(List<ReturnNoteDetailModels> models, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    List<I_Return_Note_Detail> listInsert = new List<I_Return_Note_Detail>();
                    I_Return_Note_Detail item = null;
                    foreach (var model in models)
                    {
                        item = new I_Return_Note_Detail();
                        item.Id = Guid.NewGuid().ToString();
                        item.ReturnNoteId = model.ReturnNoteId;
                        item.ReceiptNoteDetailId = model.ReceiptNoteDetailId;
                        item.ReceivedQty = model.ReceivedQty;
                        item.ReturnQty = model.ReturnQty;
                        item.IsActived = model.IsActived;

                        listInsert.Add(item);
                    }
                    cxt.I_Return_Note_Detail.AddRange(listInsert);
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

        public bool Update(ReturnNoteDetailModels model, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    //var itemUpdate = (from tb in cxt.I_Return_Note_Detail
                    //                  where tb.Id == model.Id
                    //                  select tb).FirstOrDefault();
                    //itemUpdate.ReceiptNoteId = model.ReceiptNoteDetailId;
                    //itemUpdate.ReceivedQty = model.ReceivedQty;
                    //itemUpdate.ReturnQty = model.ReturnQty;
                    //itemUpdate.IsActived = model.IsActived;

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
                    //I_Return_Note_Detail itemDelete = (from tb in cxt.I_Return_Note_Detail
                    //                            where tb.Id == Id
                    //                            select tb).FirstOrDefault();
                    //cxt.I_Return_Note_Detail.Remove(itemDelete);
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

        public List<ReturnNoteDetailModels> GetData()
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    //var lstResult = (from tb in cxt.I_Return_Note_Detail
                    //                 select new ReturnNoteDetailModels()
                    //                 {
                    //                     Id = tb.Id,
                    //                     ReceiptNoteDetailId = tb.ReceiptNoteDetailId,
                    //                     ReceivedQty = tb.ReceivedQty,
                    //                     ReturnQty = tb.ReturnQty,
                    //                     IsActived = tb.IsActived,
                    //                 }).ToList();
                    //return lstResult;
                    return new List<ReturnNoteDetailModels>();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public ReturnNoteDetailModels GetDetail(string ID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    //var model = (from tb in cxt.I_Return_Note_Detail
                    //             where tb.Id == ID
                    //             select new ReturnNoteDetailModels()
                    //             {
                    //                 Id = tb.Id,
                    //                 ReceiptNoteDetailId = tb.ReceiptNoteDetailId,
                    //                 ReceivedQty = tb.ReceivedQty,
                    //                 ReturnQty = tb.ReturnQty,
                    //                 IsActived = tb.IsActived
                    //             }).FirstOrDefault();
                    //return model;
                    return new ReturnNoteDetailModels();
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
