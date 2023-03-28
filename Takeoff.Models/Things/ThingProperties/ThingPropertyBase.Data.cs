using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Linq.Expressions;


namespace Takeoff.Models
{

    /// <summary>
    /// Provides a higher level of property management than typical CLR properties.  Has the ability to track whether the property has been set, set default values, serialize properly.
    /// When combined with FancyType, this reduces lots of repetitive code.
    /// </summary>
    /// <remarks>
    /// ComponentProperty objects can only be created through the Register functions right now, typically in 
    /// static initializers.
    /// </remarks>
    partial class ThingProperty
    {

        /// <summary>
        /// Indicates the data model type that this thing interfaces with.  This is a linq2sql class.
        /// </summary>
        public virtual Type DataModelType { get; set; }


        /// <summary>
        /// Possibly sets the value of this property from the data model passed.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="dataRecord"></param>
        public virtual void FillWithRecord(IThingPropertyContainer owner, object dataRecord)
        {
            throw new NotImplementedException();
            //if (GetValueFromDataModel == null)
            //{
            //    var property = dataModel.GetType().GetProperty(DataPropertyName);
            //    GetValueFromDataModel = (owningThing) =>
            //        {
            //            return DynamicProperties.GetValue(owningThing, DataPropertyName);
            //        };
            //}
            //object dataObject = GetValueFromDataModel(dataModel);
            //this.SetValueAsObject(owner, dataObject);
//            var dataObject = GetValueFromDataModel(dataModel);
        }
//        private Func<object, object> GetValueFromDataModel;

        /// <summary>
        /// Fills the given data record with this property if it should.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="dataModel"></param>
        public virtual void FillRecord(IThingPropertyContainer owner, object dataRecord)
        {
            throw new NotImplementedException();
        }

        /////
        ///// Creates a dynamic getter for the property
        /////
        //private static Func<object, object> CreateGetMethod(PropertyInfo propertyInfo)
        //{
        //    /*
        //    * If there's no getter return null
        //    */
        //    MethodInfo getMethod = propertyInfo.GetGetMethod();
        //    if (getMethod == null)/
        //        return null;

        //    /*
        //    * Create the dynamic method
        //    */
        //    Type[] arguments = new Type[1];
        //    arguments[0] = typeof(object);

        //    DynamicMethod getter = new DynamicMethod(
        //      String.Concat("_Get", propertyInfo.Name, "_"),
        //      typeof(object), arguments, propertyInfo.DeclaringType);
        //    ILGenerator generator = getter.GetILGenerator();
        //    generator.DeclareLocal(typeof(object));
        //    generator.Emit(OpCodes.Ldarg_0);
        //    generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
        //    generator.EmitCall(OpCodes.Callvirt, getMethod, null);

        //    if (!propertyInfo.PropertyType.IsClass)
        //        generator.Emit(OpCodes.Box, propertyInfo.PropertyType);

        //    generator.Emit(OpCodes.Ret);

        //    /*
        //    * Create the delegate and return it
        //    */
        //    return (Func<object, object>)getter.CreateDelegate(typeof(Func<object, object>));
        //}


        //public delegate void GenericSetter(object target, object value);
        /////
        ///// Creates a dynamic setter for the property
        /////
        //private static GenericSetter CreateSetMethod(PropertyInfo propertyInfo)
        //{
        //    /*
        //    * If there's no setter return null
        //    */
        //    MethodInfo setMethod = propertyInfo.GetSetMethod();
        //    if (setMethod == null)
        //        return null;

        //    /*
        //    * Create the dynamic method
        //    */
        //    Type[] arguments = new Type[2];
        //    arguments[0] = arguments[1] = typeof(object);

        //    DynamicMethod setter = new DynamicMethod(
        //      String.Concat("_Set", propertyInfo.Name, "_"),
        //      typeof(void), arguments, propertyInfo.DeclaringType);
        //    ILGenerator generator = setter.GetILGenerator();
        //    generator.Emit(OpCodes.Ldarg_0);
        //    generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
        //    generator.Emit(OpCodes.Ldarg_1);

        //    if (propertyInfo.PropertyType.IsClass)
        //        generator.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
        //    else
        //        generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);

        //    generator.EmitCall(OpCodes.Callvirt, setMethod, null);
        //    generator.Emit(OpCodes.Ret);

        //    /*
        //    * Create the delegate and return it
        //    */
        //    return (GenericSetter)setter.CreateDelegate(typeof(GenericSetter));
        //}


        //public static Func<object, object> GetMemberGetDelegate(Type objectType, string memberName)
        //{
        //    PropertyInfo pi = objectType.GetProperty(memberName);
        //    FieldInfo fi = objectType.GetField(memberName);
        //    if (pi != null)
        //    {
        //        MethodInfo mi = pi.GetGetMethod();
        //        if (mi != null)
        //        {
        //            // NOTE:  As reader J. Dunlap pointed out...

        //            //  Calling a property's get accessor is faster/cleaner using

        //            //  Delegate.CreateDelegate rather than Reflection.Emit 

        //            return (Func<object, object>)Delegate.CreateDelegate(typeof(Func<object, object>), mi);
        //        }
        //        else
        //        {
        //            throw new Exception(String.Format("Property: '{0}' of Type: '{1}' does" + " not have a Public Get accessor", memberName, objectType.Name));
        //        }
        //    }
        //    //else if (fi != null)
        //    //{
        //    //    // Member is a Field...


        //    //    DynamicMethod dm = new DynamicMethod("Get" + memberName,
        //    //        typeof(MemberType), new Type[] { objectType }, objectType);
        //    //    ILGenerator il = dm.GetILGenerator();
        //    //    // Load the instance of the object (argument 0) onto the stack

        //    //    il.Emit(OpCodes.Ldarg_0);
        //    //    // Load the value of the object's field (fi) onto the stack

        //    //    il.Emit(OpCodes.Ldfld, fi);
        //    //    // return the value on the top of the stack

        //    //    il.Emit(OpCodes.Ret);

        //    //    return (MemberGetDelegate<MemberType>)
        //    //        dm.CreateDelegate(typeof(MemberGetDelegate<MemberType>));
        //    //}
        //    //else
        //        throw new Exception(String.Format(
        //            "Member: '{0}' is not a Public Property or Field of Type: '{1}'",
        //            memberName, objectType.Name));
        //}

    }



    //public class TypeUtility<ObjectType>
    //{
    //    public delegate MemberType
    //           MemberGetDelegate<MemberType>(ObjectType obj);

    //    public static MemberGetDelegate<MemberType>
    //        GetMemberGetDelegate<MemberType>(string memberName)
    //    {
    //        Type objectType = typeof(ObjectType);

    //        PropertyInfo pi = objectType.GetProperty(memberName);
    //        FieldInfo fi = objectType.GetField(memberName);
    //        if (pi != null)
    //        {
    //            // Member is a Property...


    //            MethodInfo mi = pi.GetGetMethod();
    //            if (mi != null)
    //            {
    //                // NOTE:  As reader J. Dunlap pointed out...

    //                //  Calling a property's get accessor is faster/cleaner using

    //                //  Delegate.CreateDelegate rather than Reflection.Emit 

    //                return (MemberGetDelegate<MemberType>)
    //                    Delegate.CreateDelegate(typeof(
    //                          MemberGetDelegate<MemberType>), mi);
    //            }
    //            else
    //                throw new Exception(String.Format(
    //                    "Property: '{0}' of Type: '{1}' does" +
    //                    " not have a Public Get accessor",
    //                    memberName, objectType.Name));
    //        }
    //        else if (fi != null)
    //        {
    //            // Member is a Field...


    //            DynamicMethod dm = new DynamicMethod("Get" + memberName,
    //                typeof(MemberType), new Type[] { objectType }, objectType);
    //            ILGenerator il = dm.GetILGenerator();
    //            // Load the instance of the object (argument 0) onto the stack

    //            il.Emit(OpCodes.Ldarg_0);
    //            // Load the value of the object's field (fi) onto the stack

    //            il.Emit(OpCodes.Ldfld, fi);
    //            // return the value on the top of the stack

    //            il.Emit(OpCodes.Ret);

    //            return (MemberGetDelegate<MemberType>)
    //                dm.CreateDelegate(typeof(MemberGetDelegate<MemberType>));
    //        }
    //        else
    //            throw new Exception(String.Format(
    //                "Member: '{0}' is not a Public Property or Field of Type: '{1}'",
    //                memberName, objectType.Name));
    //    }
    //}


    /// <summary>
    /// Allows fast runtime setting of properties.
    /// </summary>
    public static class DynamicProperties
    {
        public static void SetValue(object container, string propertyName, object value)
        {
            GetSetter(propertyName, container.GetType())(container, value);
        }

        public static GenericSetter GetSetter(string propertyName, Type containerType)
        {
            GenericSetter setter;
            string key = PropertyKey(containerType, propertyName);
            if (!setters.TryGetValue(key, out setter))
            {
                lock (setters)
                {
                    if (!setters.TryGetValue(key, out setter))
                    {
                        var property = containerType.GetProperty(propertyName);
                        setter = CreateSetMethod(property);
                        setters.Add(key, setter);
                    }
                }
            }
            return setter;
        }

        public static object GetValue(object container, string propertyName)
        {
            return GetGetter(container.GetType(), propertyName)(container);
        }

        public static Func<object, object> GetGetter(Type containerType, string propertyName)
        {
            Func<object, object> getter;
            string key = PropertyKey(containerType, propertyName);
            if (!getters.TryGetValue(key, out getter))
            {
                lock (getters)
                {
                    if (!getters.TryGetValue(key, out getter))
                    {
                        var property = containerType.GetProperty(propertyName);
                        getter = CreateGetMethod(property);
                        getters.Add(key, getter);
                    }
                }
            }
            return getter;
        }


        private static string PropertyKey(Type containerType, string propertyName)
        {
            return containerType.FullName + "." + propertyName;
        }

        private static readonly Dictionary<string, Func<object, object>> getters = new Dictionary<string, Func<object, object>>();
        private static readonly Dictionary<string, GenericSetter> setters = new Dictionary<string, GenericSetter>();

        //public static Func<T, object> GetValueGetter<T>(this PropertyInfo propertyInfo)
        //{
        //    if (typeof(T) != propertyInfo.DeclaringType)
        //    {
        //        throw new ArgumentException();
        //    }

        //    var instance = Expression.Parameter(propertyInfo.DeclaringType, "i");
        //    var property = Expression.Property(instance, propertyInfo);
        //    var convert = Expression.TypeAs(property, typeof(object));
        //    return (Func<T, object>)Expression.Lambda(convert, instance).Compile();
        //}

        //public static Action<T, object> GetValueSetter<T>(this PropertyInfo propertyInfo)
        //{
        //    if (typeof(T) != propertyInfo.DeclaringType)
        //    {
        //        throw new ArgumentException();
        //    }

        //    var instance = Expression.Parameter(propertyInfo.DeclaringType, "i");
        //    var argument = Expression.Parameter(typeof(object), "a");
        //    var setterCall = Expression.Call(
        //        instance,
        //        propertyInfo.GetSetMethod(),
        //        Expression.Convert(argument, propertyInfo.PropertyType));

        //    return (Action<T, object>)Expression.Lambda(setterCall, instance, argument).Compile();
        //    Delegate d;
        //    d.D
        //}

        //static Action<T, TValue> BuildSet<T, TValue>(string property)
        //{
        //    string[] props = property.Split('.');
        //    Type type = typeof(T);
        //    ParameterExpression arg = Expression.Parameter(type, "x");
        //    ParameterExpression valArg = Expression.Parameter(typeof(TValue), "val");
        //    Expression expr = arg;
        //    foreach (string prop in props.Take(props.Length - 1))
        //    {
        //        // use reflection (not ComponentModel) to mirror LINQ 
        //        PropertyInfo pi = type.GetProperty(prop);
        //        expr = Expression.Property(expr, pi);
        //        type = pi.PropertyType;
        //    }
        //    // final property set...
        //    PropertyInfo finalProp = type.GetProperty(props.Last());
        //    MethodInfo setter = finalProp.GetSetMethod();
        //    expr = Expression.Call(expr, setter, valArg);
        //    return Expression.Lambda<Action<T, TValue>>(expr, arg, valArg).Compile();

        //}
        //static Func<T, TValue> BuildGet<T, TValue>(string property)
        //{
        //    string[] props = property.Split('.');
        //    Type type = typeof(T);
        //    ParameterExpression arg = Expression.Parameter(type, "x");
        //    Expression expr = arg;
        //    foreach (string prop in props)
        //    {
        //        // use reflection (not ComponentModel) to mirror LINQ 
        //        PropertyInfo pi = type.GetProperty(prop);
        //        expr = Expression.Property(expr, pi);
        //        type = pi.PropertyType;
        //    }
        //    return Expression.Lambda<Func<T, TValue>>(expr, arg).Compile();
        //}
        ///
        /// Creates a dynamic getter for the property
        ///
        private static Func<object, object> CreateGetMethod(PropertyInfo propertyInfo)
        {
            /*
            * If there's no getter return null
            */
            MethodInfo getMethod = propertyInfo.GetGetMethod();
            if (getMethod == null)
                return null;

            /*
            * Create the dynamic method
            */
            Type[] arguments = new Type[1];
            arguments[0] = typeof(object);

            DynamicMethod getter = new DynamicMethod(
              String.Concat("_Get", propertyInfo.Name, "_"),
              typeof(object), arguments, propertyInfo.DeclaringType);
            ILGenerator generator = getter.GetILGenerator();
            generator.DeclareLocal(typeof(object));
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
            generator.EmitCall(OpCodes.Callvirt, getMethod, null);

            if (!propertyInfo.PropertyType.IsClass)
                generator.Emit(OpCodes.Box, propertyInfo.PropertyType);

            generator.Emit(OpCodes.Ret);

            /*
            * Create the delegate and return it
            */
            return (Func<object, object>)getter.CreateDelegate(typeof(Func<object, object>));
        }


        public delegate void GenericSetter(object target, object value);
        ///
        /// Creates a dynamic setter for the property
        ///
        private static GenericSetter CreateSetMethod(PropertyInfo propertyInfo)
        {
            /*
            * If there's no setter return null
            */
            MethodInfo setMethod = propertyInfo.GetSetMethod();
            if (setMethod == null)
                return null;

            /*
            * Create the dynamic method
            */
            Type[] arguments = new Type[2];
            arguments[0] = arguments[1] = typeof(object);

            DynamicMethod setter = new DynamicMethod(
              String.Concat("_Set", propertyInfo.Name, "_"),
              typeof(void), arguments, propertyInfo.DeclaringType);
            ILGenerator generator = setter.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
            generator.Emit(OpCodes.Ldarg_1);

            if (propertyInfo.PropertyType.IsClass)
                generator.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
            else
                generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);

            generator.EmitCall(OpCodes.Callvirt, setMethod, null);
            generator.Emit(OpCodes.Ret);

            /*
            * Create the delegate and return it
            */
            return (GenericSetter)setter.CreateDelegate(typeof(GenericSetter));
        }

    }
}
