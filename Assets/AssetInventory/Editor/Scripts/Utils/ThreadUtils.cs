using System;
using System.Reflection;
using System.Threading;

namespace AssetInventory
{
    public class ThreadUtils
    {
        private static SynchronizationContext _mainThreadContext;

        public static void Initialize()
        {
            if (_mainThreadContext == null)
            {
                _mainThreadContext = SynchronizationContext.Current;
            }
        }

        public static void InvokeOnMainThread(MethodInfo method, object target, object[] parameters)
        {
            if (_mainThreadContext == null)
            {
                throw new InvalidOperationException("MainThreadInvoker not initialized. Call Initialize() from the main thread.");
            }

            _mainThreadContext.Post(_ => method.Invoke(target, parameters), null);
        }
    }
}
