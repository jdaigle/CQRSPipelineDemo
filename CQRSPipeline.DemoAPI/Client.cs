using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CQRSPipeline.DemoAPI.Data;
using CQRSPipeline.DemoAPI.Dispatch;

namespace CQRSPipeline.DemoAPI
{
    public class Client : IAPIClient
    {
        private static bool intialized = false;
        private static int intializedSignaled = 0;

        /// <summary>
        /// Initializes the API client.
        /// </summary>
        public static void Init()
        {
            if (Interlocked.Exchange(ref intializedSignaled, 1) != 0)
            {
                return;
            }

            handlers = new DispatchHandlers();
            handlers.AutoRegisterHandlers();

            var connectionString = ConfigurationManager.ConnectionStrings["AdventureWorks"].ConnectionString;
            dbContextFactory = () => new AdventureWorksDbContext(connectionString);
            dbConnectionFactory = () =>
            {
                // use this approach instead of creating a straight SqlConnection to enable profiling
                var c = DbProviderFactories.GetFactory("System.Data.SqlClient").CreateConnection();
                c.ConnectionString = ConfigurationManager.ConnectionStrings["AdventureWorks"].ConnectionString;
                return c;
            };


            intialized = true;
        }

        private static DispatchHandlers handlers;
        private static Func<AdventureWorksDbContext> dbContextFactory;
        private static Func<DbConnection> dbConnectionFactory;

        /// <summary>
        /// Returns a new instance of an API client.
        /// </summary>
        /// <returns></returns>
        public static IAPIClient New()
        {
            if (!intialized)
            {
                throw new InvalidOperationException("You must call Init() first.");
            }

            return new Client();
        }

        private Client()
        {
            this.commandDispatcher = new CommandDispatcher(this, handlers, dbContextFactory);
            this.queryDispatcher = new QueryDispatcher(this, handlers);
        }

        private readonly CommandDispatcher commandDispatcher;
        private readonly QueryDispatcher queryDispatcher;

        public void Execute(IAPICommand command)
        {
            commandDispatcher.Dispatch<object>(command);
        }

        public TResult Execute<TResult>(IAPICommand<TResult> command)
        {
            return commandDispatcher.Dispatch<TResult>(command);
        }

        public TResult Query<TResult>(IAPIQuery<TResult> query)
        {
            var disposeQueryScope = false;
            if (currentQueryScope == null)
            {
                currentQueryScope = QueryScope();
                disposeQueryScope = true;
            }
            try
            {
                return queryDispatcher.Dispatch(query, currentQueryScope);
            }
            finally
            {
                if (disposeQueryScope)
                {
                    currentQueryScope.Dispose();
                    currentQueryScope = null;
                }
            }
        }

        private QueryScope currentQueryScope;

        public QueryScope QueryScope()
        {
            if (currentQueryScope != null)
            {
                throw new InvalidOperationException("QueryScope Already Open");
            }
            currentQueryScope = new QueryScope(this, dbConnectionFactory);
            currentQueryScope.Disposed += (object sender, EventArgs args) =>
            {
                currentQueryScope = null;
            };
            return currentQueryScope;
        }
    }
}
