using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Takeoff.Controllers
{
    public class ErrorController : Controller
    {
        /// <summary>
        /// Called by asp.net when an error (NOT from a controller exception, more stuff like bad urls) occurs.
        /// </summary>
        public ActionResult Index(string id)
        {
            HttpStatusCode code;
            ViewData.Model = Enum.TryParse<HttpStatusCode>(id, out code) ? new HttpStatusCode?(code) : new HttpStatusCode?();
            return View("Error.HttpCode");
        }

        public ActionResult PageNotFound()
        {
            return View();
        }

    }
}
