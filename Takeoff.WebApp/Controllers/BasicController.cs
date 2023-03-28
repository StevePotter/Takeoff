using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Mediascend.Web;
using System.Configuration;
using System.Security;
using System.Linq.Expressions;
using MvcContrib;
using Ninject;
using MvcContrib.ActionResults;
using System.Web.WebPages.Scope;

namespace Takeoff.Controllers
{
    public abstract class BasicController : Controller
    {
        /// <summary>
        /// Used to bypass normal controller logic to test views.  Evenutally this can be replaced by a better testing framework.
        /// </summary>
        /// <param name="actn"></param>
        /// <param name="view"></param>
        /// <param name="model"></param>
        /// <param name="scenario"></param>
        /// <returns></returns>
        [SpecialRestriction(SpecialRestriction.Staff| SpecialRestriction.Local)]
        public virtual ActionResult RenderView(string actn, string view, object model, string scenario)
        {
            if ( model != null && model.GetType().Equals(typeof(object)))
                model = null;
            if (ViewData["ControllerName"] == null)
                ViewData["ControllerName"] = GetType().Name.EndWithout("Controller");
            ;//this an actionname are needed for proper body css classes 
            if (ViewData["ActionName"] == null)
                ViewData["ActionName"] = actn;//this an actionname are needed for proper body css classes 

            return View(view.CharsOr(actn), model);
        }

        protected override IActionInvoker CreateActionInvoker()
        {
            return IoC.Current.Get<IActionInvoker>();
        }


    }


}
