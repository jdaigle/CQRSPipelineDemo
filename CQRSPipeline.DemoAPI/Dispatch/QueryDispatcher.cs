using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRSPipeline.DemoAPI.Dispatch
{
    public class QueryDispatcher
    {
        private readonly Client client;
        private readonly DispatchHandlers handlers;
        private readonly Func<DbConnection> dbConnectionFactory;

        public QueryDispatcher(Client client, DispatchHandlers handlers, Func<DbConnection> dbConnectionFactory)
        {
            this.client = client;
            this.handlers = handlers;
            this.dbConnectionFactory = dbConnectionFactory;
        }

        public TResult Dispatch<TResult>(IAPIQuery<TResult> query)
        {
            var sw = Stopwatch.StartNew();
            var queryContext = new QueryContext();
            queryContext.CurrentQuery = query;
            try
            {
                OnDispatching(queryContext);
                var handler = handlers.FindHandler(query.GetType());
                var result = (TResult)handler.Invoke(query, queryContext);
                OnDispatched(queryContext);
                // TODO: logging and other instrumentation
                sw.Stop();
                return result;
            }
            catch (Exception e)
            {
                OnDispatched(queryContext, e);
                sw.Stop();
                // TODO: logging
                throw;
            }
        }

        private void OnDispatching(QueryContext queryContext)
        {
            // TODO: open connection, other pipeline operations here
            queryContext.CurrentConnection = dbConnectionFactory();
            queryContext.CurrentConnection.Open();
            queryContext.CurrentTransaction = queryContext.CurrentConnection.BeginTransaction();
        }

        private void OnDispatched(QueryContext queryContext, Exception exception = null)
        {
            try
            {
                if (exception == null)
                {
                    queryContext.CurrentTransaction.Commit();
                }
                else
                {
                    try
                    {
                        queryContext.CurrentTransaction.Rollback();
                    }
                    catch (Exception)
                    {
                        // do not throw;
                        // TODO: logging if necessary
                    }
                }
            }
            finally
            {
                if (queryContext.CurrentTransaction != null)
                {
                    queryContext.CurrentTransaction = null;
                }
                if (queryContext.CurrentConnection != null)
                {
                    queryContext.CurrentConnection = null;
                }
            }
        }
    }
}
