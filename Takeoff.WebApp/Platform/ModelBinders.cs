using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Globalization;

namespace Takeoff.Platform
{

    /// <summary>
    /// ModelBinder for FileSize and FileSize?
    /// </summary>
    public class FileSizeModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var strValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).AttemptedValue;
            if (strValue.HasChars())
                return new FileSize(strValue);

            if (bindingContext.ModelType.Equals(typeof(FileSize?)))
            {
                return new FileSize?();
            }
            return new FileSize();
        }
    }



    /// <summary>
    /// Binds a value like $45.99 to a double or double?.
    /// </summary>
    public class CurrencyModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var strValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).AttemptedValue;
            if (strValue.HasChars())
            {
                double value;
                if ( double.TryParse(strValue, NumberStyles.Currency, NumberFormatInfo.CurrentInfo, out value) )
                {
                    return value;
                }
            }

            if (bindingContext.ModelType.Equals(typeof(double?)))
            {
                return new double?();
            }
            return new double();
        }
    }

}