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
        private readonly DispatchHandlers handlers;
        private readonly FastContainer iocContainer;

        public QueryDispatcher(DispatchHandlers handlers, FastContainer iocContainer)
        {
            this.handlers = handlers;
            this.iocContainer = iocContainer;
        }

        public TResult Dispatch<TResult>(IAPIQuery<TResult> query, QueryScope queryScope)
        {
            var sw = Stopwatch.StartNew();
            var queryContext = new QueryContext();
            queryContext.CurrentQuery = query;
            queryContext.QueryScope = queryScope;
            using (var childContainer = iocContainer.Clone())
            {
                childContainer.Register(queryContext);
                try
                {
                    OnDispatching(queryContext);
                    var handler = handlers.FindHandler(query.GetType());
                    var result = (TResult)handler.Execute(ResolveParameters(handler, query, childContainer));
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
        }

        private object[] ResolveParameters(ActionMethodDispatcher handler, object query, FastContainer childContainer)
        {
            var parameters = new object[handler.ParameterTypes.Count];
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameterType = handler.ParameterTypes[i];
                if (parameterType == query.GetType())
                {
                    parameters[i] = query; // shortcut for query, it's a known type
                    continue;
                }
                parameters[i] = childContainer.Resolve(parameterType);
            }
            return parameters;
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
