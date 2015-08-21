using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRSPipeline.DemoAPI
{
    public class QueryContext
    {
        public DbConnection CurrentConnection { get; set; }
        public DbTransaction CurrentTransaction { get; set; }
        public object CurrentQuery { get; set; }
    }
}
