using System;
using System.Web.Mvc;

namespace Takeoff.App_Start
{
    public class NullableDoubleModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult == null)
                return base.BindModel(controllerContext, bindingContext);
            if (valueProviderResult.RawValue != null)
                return Convert.ToDouble(valueProviderResult.RawValue);
            if (valueProviderResult.AttemptedValue.HasChars())
            {
                double value;
                if (double.TryParse(valueProviderResult.AttemptedValue, out value))
                    return new double?(value);
            }
            return new double?();
        }
    }
}