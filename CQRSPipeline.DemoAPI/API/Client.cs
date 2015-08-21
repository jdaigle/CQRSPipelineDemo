using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSPipeline.DemoAPI.API
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
            dbConnectionFactory = () => new SqlConnection(connectionString);

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
            this.queryDispatcher = new QueryDispatcher(this, handlers, dbConnectionFactory);
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
            return queryDispatcher.Dispatch(query);
        }
    }
}
