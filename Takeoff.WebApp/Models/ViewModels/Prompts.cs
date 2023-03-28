using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Mediascend.Web;
using Takeoff.Models;
using Takeoff.Data;

namespace Takeoff.ViewModels
{

    public class ViewPromptViewModel
    {
        /// <summary>
        /// The page that was requested, but the prompt's view was put in place of.  This is useful for setting an href on "OK" buttons.
        /// </summary>
        public string OriginalUrl { get; set; }

    }


}


