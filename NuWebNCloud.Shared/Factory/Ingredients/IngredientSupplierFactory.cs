using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory.Product;
using NuWebNCloud.Shared.Models.Ingredients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class IngredientSupplierFactory
    {
        private BaseFactory _baseFactory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private ProductFactory _productFactory = null;

        public IngredientSupplierFactory()
        {
            _productFactory = new ProductFactory();
            _baseFactory = new BaseFactory();
        }

        //Get Data 
        public List<Ingredients_SupplierModel> _GetData(string supplier)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from IS in cxt.I_Ingredient_Supplier
                                     from I in cxt.I_Ingredient
                                     where IS.SupplierId == supplier && I.Id == IS.IngredientId
                                     orderby IS.CreatedDate descending
                                     select new Ingredients_SupplierModel()
                                     {
                                         Id = IS.Id,
                                         IngredientName = I.Name,
                                         IngredientId = IS.IngredientId,
                                         SupplierId = IS.SupplierId,
                                         CreatedBy = IS.CreatedBy,
                                         CreatedDate = IS.CreatedDate,
                                         ModifierBy = IS.ModifierBy,
                                         ModifierDate = IS.ModifierDate,
                                         IsActived = IS.IsActived,
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

        public List<string> GetDataForIngredient(string IngredientId, string CompanyId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from IS in cxt.I_Ingredient_Supplier
                                     from S in cxt.I_Supplier
                                     where IS.IngredientId.Equals(IngredientId) && IS.IsActived
                                     && S.IsActived && S.Id.Equals(IS.SupplierId) && S.CompanyId.Equals(CompanyId)
                                     select IS.SupplierId).ToList();
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        public List<string> GetDataForSupplier(string SupplierId, string CompanyId)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from IS in cxt.I_Ingredient_Supplier
                                     from I in cxt.I_Ingredient
                                     where IS.SupplierId.Equals(SupplierId)
                                     && I.IsActive && I.Id.Equals(IS.IngredientId) && I.CompanyId.Equals(CompanyId) && IS.IsActived == true
                                     select IS.IngredientId).ToList();
                    return lstResult;
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
