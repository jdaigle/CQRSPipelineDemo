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

        public QueryDispatcher(Client client, DispatchHandlers handlers)
        {
            this.client = client;
            this.handlers = handlers;
        }

        public TResult Dispatch<TResult>(IAPIQuery<TResult> query, QueryScope queryScope)
        {
            var sw = Stopwatch.StartNew();
            var queryContext = new QueryContext();
            queryContext.CurrentQuery = query;
            queryContext.QueryScope = queryScope;
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
            // TODO: other pipeline operations here
        }

        private void OnDispatched(QueryContext queryContext, Exception exception = null)
        {
            if (exception != null)
            {
                queryContext.QueryScope.Rollback();
            }
            // TODO: other pipeline operations here
        }
    }
}
