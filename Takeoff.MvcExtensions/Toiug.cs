//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Mediascend.Web
//{
//    public class ThreadUtil
//    {
//        private static AsyncCallback callback = new AsyncCallback(ThreadUtil.EndWrapperInvoke);
//        private static DelegateWrapper wrapperInstance = new DelegateWrapper(ThreadUtil.InvokeWrappedDelegate);

//        private static void EndWrapperInvoke(IAsyncResult ar)
//        {
//            wrapperInstance.EndInvoke(ar);
//            ar.AsyncWaitHandle.Close();
//        }

//        public static void FireAndForget(Delegate d, params object[] args)
//        {
//            wrapperInstance.BeginInvoke(d, args, callback, null);
//        }

//        private static void InvokeWrappedDelegate(Delegate d, object[] args)
//        {
//            d.DynamicInvoke(args);
//        }


//        private delegate void DelegateWrapper(Delegate d, object[] args);
//    }
//}
