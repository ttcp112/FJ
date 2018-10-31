using Newtonsoft.Json;
using NLog;
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
    public class ProductTypeFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public ProductTypeFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<ProductTypeApiModels> GetListProductType()
        {
            List<ProductTypeApiModels> listData = new List<ProductTypeApiModels>();
            try
            {
                ProductTypeApiModels paraBody = new ProductTypeApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetProductType, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListProductType"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                listData = JsonConvert.DeserializeObject<List<ProductTypeApiModels>>(lstContent);
                return listData;
            }
            catch (Exception e)
            {
                _logger.Error("ProductType_GetList: " + e);
                return listData;
            }
        }

        public string GetProductTypeName(int Code)
        {
            string name = "";
            switch (Code)
            {
                case 4:
                    name = "Set Menu";
                    break;
                case 2:
                    name = "Modifier";
                    break;
                case 1:
                    name = "Dish";
                    break;
                case 9:
                    name = "Misc";
                    break;
                case 5:
                    name = "Discount";
                    break;
                case 10:
                    name = "SpecialModifier";
                    break;
                case 6:
                    name = "Promotion";
                    break;
                default:
                    name = "None";
                    break;
            }
            return name;
        }
    }

    public enum EProductType
    {
        SetMenu = 4,
        Modifier = 2,
        Dish = 1,
        Misc = 9,
        Discount = 5,
        SpecialModifier = 10
    }
}
