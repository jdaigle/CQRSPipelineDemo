using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQRSPipeline.DemoAPI.Dispatch
{
    public class FastContainerRegistration : IDisposable
    {
        private readonly Func<FastContainer, object> resolve;
        private object instance;
        private bool instancePerCall;

        private FastContainerRegistration(FastContainerRegistration other)
        {
            this.resolve = other.resolve;
            this.instancePerCall = other.instancePerCall;
            this.instance = other.instance;
        }

        public FastContainerRegistration(Func<FastContainer, object> resolve)
        {
            this.resolve = resolve;
        }

        public FastContainerRegistration(object instance)
        {
            this.instance = instance;
        }

        public virtual FastContainerRegistration InstancePerCall()
        {
            AssertNotDisposed();
            this.instancePerCall = true;
            return this;
        }

        public virtual object Resolve(FastContainer container)
        {
            AssertNotDisposed();
            if (this.instancePerCall)
            {
                return this.resolve(container);
            }
            if (this.instance != null)
            {
                return this.instance;
            }
            return this.instance = this.resolve(container);
        }

        public virtual FastContainerRegistration Clone()
        {
            AssertNotDisposed();
            return new FastContainerRegistration(this);
        }

        private void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("TinyContainerRegistration");
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                try
                {
                    if (instance != null && instance is IDisposable)
                    {
                        ((IDisposable)instance).Dispose();
                    }
                }
                finally
                {
                    _disposed = true;
                }
            }
        }
    }
}
