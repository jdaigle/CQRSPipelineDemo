using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CQRSPipeline.DemoAPI.Data;
using CQRSPipeline.Framework;

namespace CQRSPipeline.DemoAPI
{
    public class DemoAPIScopedInstanceFactory : ScopedInstanceFactory
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["AdventureWorks"].ConnectionString;

        public DemoAPIScopedInstanceFactory()
        {
            this.DbContext = new AdventureWorksDbContext(connectionString);

            this.SingleInstanceFactory = type =>
            {
                if (type == typeof(DbContext))
                    return DbContext;
                return null;
            };
        }

        public AdventureWorksDbContext DbContext { get; private set; }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
