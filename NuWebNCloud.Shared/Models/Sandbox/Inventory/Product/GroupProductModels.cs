using NuWebNCloud.Shared.Utilities;
using System.Collections.Generic;
using System.Web.Mvc;
namespace NuWebNCloud.Shared.Models.Sandbox.Inventory.Product
{
    public class GroupProductModels
    {
        public string ID { get; set; }

        [_AttributeForLanguageRange(0, int.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public int Sequence { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int Minimum { get; set; }

        [_AttributeForLanguageRange(0, int.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public int Maximum { get; set; }
        public int Type { get; set; }
        public List<ProductOnGroupModels> ListProductOnGroup { get; set; }

        public int OffSet { get; set; }
        public string SetMenuID { get; set; }
        public byte Status { get; set; }

        //public int _GroupOffSet { get; set; }
        public List<SelectListItem> ListModifierType { get; set; }

        public int currentgroupOffSet { get; set; }
        public int currentOffset { get; set; }

        public GroupProductModels()
        {
            Maximum = 1;
            //======
            Type = (byte)Commons.EModifierType.Forced;
            ListModifierType = new List<SelectListItem>()
            {
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ModifierForced), Value = Commons.EModifierType.Forced.ToString("d")},
                new SelectListItem() { Text = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ModifierOptional), Value = Commons.EModifierType.Optional.ToString("d")},
            };
        }
    }
}
