using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ninject;
using Ninject.Syntax;

namespace Takeoff
{

    public static class IoC
    {


        public static IKernel Current
        {
            get
            {
                return _current;
            }
            set
            {
                _current = value;
            }
        }
        static IKernel _current = new StandardKernel();

        public static T Get<T>()
        {
            return Current.Get<T>();
        }

        public static T GetOrNull<T>()
        {
            return Current.TryGet<T>();
        }

        public static void Bind<TInterface, TImplementation>(bool inSingletonScope = true) where TImplementation : TInterface
        {
            if (inSingletonScope)
                Current.Bind<TInterface>().To<TImplementation>().InSingletonScope();
            else
                Current.Bind<TInterface>().To<TImplementation>().InThreadScope();

        }

    }
}