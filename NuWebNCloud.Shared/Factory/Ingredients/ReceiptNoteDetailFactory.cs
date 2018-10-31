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
    public class ReceiptNoteDetailFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public ReceiptNoteDetailFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public bool Insert(List<ReceiptNoteDetailModels> models, ref string msg)
        {
            using (var cxt = new NuWebContext())
            {
                try
                {
                    List<I_ReceiptNoteDetail> ListInsert = new List<I_ReceiptNoteDetail>();
                    I_ReceiptNoteDetail item = null;
                    foreach (var model in models)
                    {
                        item = new I_ReceiptNoteDetail();
                        item.Id = Guid.NewGuid().ToString();
                        item.ReceiptNoteId = model.ReceiptNoteId;
                        item.PurchaseOrderDetailId = model.PurchaseOrderDetailId;

                        item.ReceivedQty = model.ReceivedQty;
                        item.ReceivingQty = model.ReceivingQty;
                        item.RemainingQty = model.RemainingQty;

                        ListInsert.Add(item);
                    }
                    cxt.I_ReceiptNoteDetail.AddRange(ListInsert);
                    cxt.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    return false;
                }
            }
        }

        public bool InsertReceiptNoteDetail(ReceiptNoteDetailModels model)
        {
            using (var cxt = new NuWebContext())
            {
                try
                {
                    var item = new I_ReceiptNoteDetail();
                    item.Id = Guid.NewGuid().ToString();
                    //item.IngredientId = model.IngredientId;
                    item.ReceiptNoteId = model.ReceiptNoteId;
                    //item.Quantity = model.Quantity;
                    //item.Price = model.Price;
                    cxt.I_ReceiptNoteDetail.Add(item);
                    cxt.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    return false;
                }
            }
        }

        public bool UpdateReceiptNoteDetail(ReceiptNoteDetailModels model)
        {
            try
            {
                using (var cxt = new NuWebContext())
                {
                    var item = cxt.I_ReceiptNoteDetail.Where(ww => ww.Id == model.Id).FirstOrDefault();
                    if (item != null)
                    {
                        //item.Quantity = model.Quantity;
                        //item.Price = model.Price;

                        cxt.SaveChanges();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<ReceiptNoteDetailModels> _GetReceiptNoteDetailData(string strSearch)
        {
            List<ReceiptNoteDetailModels> lstResults = new List<ReceiptNoteDetailModels>();
            using (var cxt = new NuWebContext())
            {
                var query = (from rnd in cxt.I_ReceiptNoteDetail
                             from i in cxt.I_Ingredient
                                 //where rnd.IngredientId == i.Id
                             select new { rnd, i });
                if (query != null && query.Any())
                {
                    if (!string.IsNullOrEmpty(strSearch))
                    {
                        query = query.Where(ww => ww.rnd.ReceiptNoteId.ToLower().Contains(strSearch.ToLower()));
                    }

                    lstResults = query.Select(item => new ReceiptNoteDetailModels()
                    {
                        Id = item.rnd.Id,
                        ReceiptNoteId = item.rnd.ReceiptNoteId,
                        //IngredientId = item.rnd.IngredientId,
                        //IngredientCode = item.i.Code,
                        //IngredientName = item.i.Name,
                        //BaseUOM = item.i.BaseUOMName,
                        //Price = item.rnd.Price,
                        //Quantity = item.rnd.Quantity,
                    }).ToList();
                }
                return lstResults;
            }
        }

        public ReceiptNoteDetailModels GetReceiptNoteDetailById(string id)
        {
            using (var cxt = new NuWebContext())
            {
                var result = cxt.I_ReceiptNoteDetail.Where(ww => ww.Id == id)
                                .Select(rn => new ReceiptNoteDetailModels()
                                {
                                    Id = rn.Id,
                                    ReceiptNoteId = rn.ReceiptNoteId,
                                    //IngredientId = rn.IngredientId,
                                    //Price = rn.Price,
                                    //Quantity = rn.Quantity,
                                })
                                .FirstOrDefault();
                return result;
            }
        }

        public bool DeleteReceiptNoteDetail(string ReceiptNoteId)
        {
            try
            {
                using (var cxt = new NuWebContext())
                {
                    var ListDetail = cxt.I_ReceiptNoteDetail.Where(x => x.ReceiptNoteId == ReceiptNoteId).ToList();
                    cxt.I_ReceiptNoteDetail.RemoveRange(ListDetail);
                    cxt.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
