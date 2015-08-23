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
        public object CurrentQuery { get; set; }
        public QueryScope QueryScope { get; set; }
        public DbConnection CurrentConnection { get { return QueryScope.CurrentConnection; } }
        public DbTransaction CurrentTransaction { get { return QueryScope.CurrentTransaction; } }
    }
}
