using System;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Takeoff.App_Start
{
    public class XDocumentBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var request = controllerContext.HttpContext.Request.InputStream;
            var originalPosition = request.Position;
            if (originalPosition > 0)
                request.Position = 0;
            XDocument xml = null;
            try
            {
                xml = XDocument.Load(request);
            }
            catch (Exception)
            {
                bindingContext.ModelState.AddModelError("model","Invalid Input");
            }
            request.Position = originalPosition;
            return xml;
        }
    }
}