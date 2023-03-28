using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace Takeoff.WebApp.Ripper
{
    static class Helpers
    {
        /// <summary>
        /// Clones an object simply by copying its properties.  Note that complex properties will be passed by reference.  
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <returns></returns>
        public static T CloneProperties<T>(this T a)
        {
            //allows us to clone an interface by instantiating its underlying type
            var type = a.GetType();
            var copy = (T)Activator.CreateInstance(type);

            foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(type))
            {
                if (!propertyDescriptor.IsReadOnly)
                    propertyDescriptor.SetValue(copy, propertyDescriptor.GetValue(a));
            }
            return copy;
        }


        public static T SetProperty<T, TProp>(this T obj, Expression<Func<T, TProp>> property, TProp value)
        {
            var propertyInfo = ((LambdaExpression)property).Body.CastTo<MemberExpression>().Member.CastTo<PropertyInfo>();
            propertyInfo.SetValue(obj, value, null);
            return obj;
        }
    }
}
