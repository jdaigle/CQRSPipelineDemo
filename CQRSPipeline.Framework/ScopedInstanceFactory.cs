using System;

namespace CQRSPipeline.Framework
{
    public class ScopedInstanceFactory : IDisposable
    {
        public SingleInstanceFactory SingleInstanceFactory { get; set; }

        public virtual void Dispose()
        {
        }
    }
}
