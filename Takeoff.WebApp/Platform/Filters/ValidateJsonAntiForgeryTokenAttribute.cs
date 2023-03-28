using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Takeoff.Controllers;
using System.Net.Mail;
using System.Configuration;
using System.Web.Script.Serialization;
using System.IO;
using System.Web.Routing;
using System.Reflection;
using Newtonsoft.Json;
using System.Collections.Specialized;

namespace Takeoff
{

    /// <summary>
    /// Same as ValidateAntiForgeryTokenAttribute, but allows for validation for ajax json requests
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ValidateJsonAntiForgeryTokenAttribute : FilterAttribute, IAuthorizationFilter
    {
        private string _salt;
        private ValidateAntiForgeryTokenAttribute _validate;
        private readonly AcceptVerbsAttribute _verbs;

        private static PropertyInfo ReadOnlyProperty = typeof(NameValueCollection).GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);

        public string Salt
        {
            get
            {
                return _salt ?? String.Empty;
            }
            set
            {
                _salt = value;
            }
        }

        public ValidateJsonAntiForgeryTokenAttribute(HttpVerbs verbs = HttpVerbs.Post)
            : this(null, verbs)
        {
        }

        public ValidateJsonAntiForgeryTokenAttribute(string salt, HttpVerbs verbs = HttpVerbs.Post)
        {
            this._verbs = new AcceptVerbsAttribute(verbs);
            this._salt = salt;

            this._validate = new ValidateAntiForgeryTokenAttribute();
            if (salt.HasChars())
                this._validate.Salt = salt;
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            var request = filterContext.HttpContext.Request;

            // We only need to validate this if it's a post
            string httpMethodOverride = filterContext.HttpContext.Request.GetHttpMethodOverride();
            if (!this._verbs.Verbs.Contains(httpMethodOverride, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            const string FieldName = "__RequestVerificationToken";//name of the field parameter
            //here's where the magic happens.  if this is an ajax json post, check for the property and add it to the Form parameters
            if (request.Form[FieldName] == null && (filterContext.HttpContext.Request.ContentType ?? string.Empty).Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                var originalPosition = filterContext.HttpContext.Request.InputStream.Position;
                if (originalPosition > 0)
                    filterContext.HttpContext.Request.InputStream.Position = 0;
                var reader = new JsonTextReader(new StreamReader(filterContext.HttpContext.Request.InputStream));
                var serializer = Newtonsoft.Json.JsonSerializer.Create(new JsonSerializerSettings { });
                //read through all the top level (depth 1) properties and deserialize them individually to their proper types then set the parameters
                while (reader.Read())
                {
                    if (reader.Depth == 1 && reader.TokenType == JsonToken.PropertyName)
                    {
                        var property = (string)reader.Value;
                        reader.Read();
                        if (property.EqualsCaseSensitive(FieldName))
                        {
                            //form is read only right now so we use reflection to unlock it
                            var value = (string)serializer.Deserialize(reader, typeof(string));
                            ReadOnlyProperty.SetValue(request.Form, false, null);
                            request.Form.Add(FieldName, value);
                            ReadOnlyProperty.SetValue(request.Form, true, null);
                            break;
                        }
                    }
                }

                filterContext.HttpContext.Request.InputStream.Position = originalPosition;
            }
            _validate.OnAuthorization(filterContext);
        }
    }
}
