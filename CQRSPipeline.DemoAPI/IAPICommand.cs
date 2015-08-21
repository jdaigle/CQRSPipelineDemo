using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRSPipeline.DemoAPI
{
    public interface IAPICommand
    {
    }

    public interface IAPICommand<TResult> : IAPICommand
    {
    }
}
