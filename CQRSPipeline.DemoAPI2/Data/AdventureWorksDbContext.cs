using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRSPipeline.DemoAPI.Data
{
    public class AdventureWorksDbContext : DbContext
    {
        static AdventureWorksDbContext()
        {
            Database.SetInitializer<AdventureWorksDbContext>(new NullDatabaseInitializer<AdventureWorksDbContext>());
        }

        public AdventureWorksDbContext(string connectionString)
            : base(connectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.AddFromAssembly(typeof(AdventureWorksDbContext).Assembly);
        }

        public class SqlDbConfiguration : DbConfiguration
        {
            public SqlDbConfiguration()
            {
                SetProviderServices(SqlProviderServices.ProviderInvariantName, SqlProviderServices.Instance);
                SetDefaultConnectionFactory(new SqlConnectionFactory());
            }
        } 
    }

}
