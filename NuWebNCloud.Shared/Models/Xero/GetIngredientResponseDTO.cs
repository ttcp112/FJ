using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Xero
{
    public class GetIngredientResponseDTO
    {
        public bool Success { get; set; }
        public ErrorDTO Error { get; set; }
        public GetIngredientResponseData Data { get; set; }
    }
    public class ErrorDTO
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }
    public class GetIngredientResponseData
    {
        public List<IngredientItem> Items { get; set; }
    }
    public class IngredientItem
    {
        public string ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsSelect { get; set; }
        public float Usage { get; set; }
    }
    public class IngredientUsageSyncItem
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string GLAccountCode { get; set; }
        public decimal QuantityUsed { get; set; }
    }
    public class IngredientSyncRequestDTO: XeroBaseModel
    {
        public List<IngredientUsageSyncItem> Items { get; set; }

        public IngredientSyncRequestDTO()
        {
            Items = new List<IngredientUsageSyncItem>();
        }
    }
}
