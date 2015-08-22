using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CQRSPipeline.DemoAPI.Dispatch
{
    public abstract class DispatchInfo
    {
        public abstract object Invoke(params object[] args);
    }

    public class FuncDispatchInfo<TArg0, TResult> : DispatchInfo
    {
        public FuncDispatchInfo(MethodInfo method)
        {
            action = (Func<TArg0, TResult>)Delegate.CreateDelegate(typeof(Func<TArg0, TResult>), null, method);
        }

        private readonly Func<TArg0, TResult> action;

        public override object Invoke(params object[] args)
        {
            return (TResult)action((TArg0)args[0]);
        }
    }

    public class FuncDispatchInfo<TArg0, TArg1, TResult> : DispatchInfo
    {
        public FuncDispatchInfo(MethodInfo method)
        {
            action = (Func<TArg0, TArg1, TResult>)Delegate.CreateDelegate(typeof(Func<TArg0, TArg1, TResult>), null, method);
        }

        private readonly Func<TArg0, TArg1, TResult> action;

        public override object Invoke(params object[] args)
        {
            return (TResult)action((TArg0)args[0], (TArg1)args[1]);
        }
    }

    public class ActionDispatchInfo<TArg0> : DispatchInfo
    {
        public ActionDispatchInfo(MethodInfo method)
        {
            action = (Action<TArg0>)Delegate.CreateDelegate(typeof(Action<TArg0>), null, method);
        }

        private readonly Action<TArg0> action;

        public override object Invoke(params object[] args)
        {
            action((TArg0)args[0]);
            return null;
        }
    }

    public class ActionDispatchInfo<TArg0, TArg1> : DispatchInfo
    {
        public ActionDispatchInfo(MethodInfo method)
        {
            action = (Action<TArg0, TArg1>)Delegate.CreateDelegate(typeof(Action<TArg0, TArg1>), null, method);
        }

        private readonly Action<TArg0, TArg1> action;

        public override object Invoke(params object[] args)
        {
            action((TArg0)args[0], (TArg1)args[1]);
            return null;
        }
    }
}
