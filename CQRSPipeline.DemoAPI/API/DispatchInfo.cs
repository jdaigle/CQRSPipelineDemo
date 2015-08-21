using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CQRSPipeline.DemoAPI.API
{
    public abstract class DispatchInfo
    {
        public virtual object Invoke(object arg0)
        {
            throw new NotImplementedException();
        }

        public virtual object Invoke(object arg0, object arg1)
        {
            throw new NotImplementedException();
        }
    }

    public class GenericActionDispatchInfo<TArg0, TResult> : DispatchInfo
    {
        public delegate TResult ActionDelegate(TArg0 arg0);

        public GenericActionDispatchInfo(MethodInfo method)
        {
            action = (ActionDelegate)Delegate.CreateDelegate(typeof(ActionDelegate), null, method);
        }

        private ActionDelegate action;

        public override object Invoke(object arg0)
        {
            return (TResult)action((TArg0)arg0);
        }
    }

    public class GenericVoidActionDispatchInfo<TArg0> : DispatchInfo
    {
        public delegate void ActionDelegate(TArg0 arg0);

        public GenericVoidActionDispatchInfo(MethodInfo method)
        {
            action = (ActionDelegate)Delegate.CreateDelegate(typeof(ActionDelegate), null, method);
        }

        private ActionDelegate action;

        public override object Invoke(object arg0)
        {
            action((TArg0)arg0);
            return null;
        }
    }

    public class GenericActionDispatchInfo<TArg0, TArg1, TResult> : DispatchInfo
    {
        public delegate TResult ActionDelegate(TArg0 arg0, TArg1 arg1);

        public GenericActionDispatchInfo(MethodInfo method)
        {
            action = (ActionDelegate)Delegate.CreateDelegate(typeof(ActionDelegate), null, method);
        }

        private ActionDelegate action;

        public override object Invoke(object arg0, object arg1)
        {
            return (TResult)action((TArg0)arg0, (TArg1)arg1);
        }
    }

    public class GenericVoidActionDispatchInfo<TArg0, TArg1> : DispatchInfo
    {
        public delegate void ActionDelegate(TArg0 arg0, TArg1 arg1);

        public GenericVoidActionDispatchInfo(MethodInfo method)
        {
            action = (ActionDelegate)Delegate.CreateDelegate(typeof(ActionDelegate), null, method);
        }

        private ActionDelegate action;

        public override object Invoke(object arg0, object arg1)
        {
            action((TArg0)arg0, (TArg1)arg1);
            return null;
        }
    }
}
