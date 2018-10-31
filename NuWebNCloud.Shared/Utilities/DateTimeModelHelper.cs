using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Web.ModelBinding;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Utilities
{
    public class DateTimeModelHelper : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var displayFormat = bindingContext.ModelMetadata.DisplayFormatString;
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (!string.IsNullOrEmpty(displayFormat) && value != null)
            {
                DateTime date;
                displayFormat = displayFormat.Replace("{0:", string.Empty).Replace("}", string.Empty);
                
                //var a = CultureInfo.GetCultureInfo("en-GB");
                try
                {

                    date = DateTime.ParseExact(value.AttemptedValue, displayFormat, null);

                    return date;
                }
                catch (Exception ex)
                {
                    bindingContext.ModelState.AddModelError(
                       bindingContext.ModelName,
                       string.Format("{0} is an invalid date format", value.AttemptedValue)
                   );

                }
            }

            return base.BindModel(controllerContext, bindingContext);
        }
    }
}
