using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Mediascend.Web;

namespace Takeoff
{

    public class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        public LocalizedDisplayNameAttribute(string resourceName)
            : base()
        {
            ResourceName = resourceName;
        }

        /// <summary>
        /// The key of the string in Resources.Strings table.
        /// </summary>
        public string ResourceName { get; private set; }

        public override string DisplayName
        {
            get
            {
                return Resources.Strings.ResourceManager.GetString(this.ResourceName);
            }
        }
    }

}


