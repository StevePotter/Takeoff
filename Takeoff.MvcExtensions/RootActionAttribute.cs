using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mediascend.Web
{

    /// <summary>
    /// Specifies an action or a controller whose actions can be executed using a root level url.  Example "me.com/Features" as opposed to "me.com/Root/Features"
    /// </summary>
    public class RootUrlAttribute : Attribute
    {
    }

    /// <summary>
    /// Used to identify controller actions that should be accessible via RESTful urls.  By default, 'details|edit|delete' are already RESTful... like /productions/451/delete/.  But if you wanted to add something like 'deleteLogo'
    /// The action needs to have an "id" parameter in order work.
    /// </summary>
    public class RestActionAttribute : Attribute
    {
    }

    /// <summary>
    /// Used to create urls for a controller that is "under" another.  For example, it would allow you to map url: /projects/132/comments/9/edit to a controller.  In this case, you can enter [SubController("/projects/{projectId}",true)].  You would need a controller named CommentsController.  You can also use do [SubController("/projects/{projectId}",false)] and call the controller anything you want.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SubControllerAttribute : Attribute
    {
        public SubControllerAttribute(string baseUrl, bool urlContainsControllerName)
        {
            BaseUrl = baseUrl;
            UrlContainsControllerName = urlContainsControllerName;
        }

        /// <summary>
        /// The url format, such as "/projects/{projectId}/comments".  In this case the projectId will be a route parameter and could be a parameter on an action.
        /// </summary>
        public string BaseUrl
        {
            get;
            private set;
        }

        public bool UrlContainsControllerName
        {
            get;
            private set;
        }
    }


    /// <summary>
    /// Put this on a controller action if you rename the action and/or controller.  This will automatically map the old url to the new place.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple= true)]
    public class OldUrlAttribute : Attribute
    {
        public OldUrlAttribute(string url)
        {
            this.Url = url;
        }


        public string Url
        {
            get;
            private set;
        }

        /// <summary>
        /// Avoids error when setting route url:The route URL cannot start with a '/' or '~' character and it cannot contain a '?' character.
        /// </summary>
        internal string RouteSafeUrl
        {
            get
            {
                return Url.StartWithout(@"/").StartWithout("~");
            }
        }
    }
}
