using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CQRSPipeline.DemoAPI.Data;

namespace CQRSPipeline.DemoAPI.Dispatch
{
    public class CommandDispatcher
    {
        private readonly DispatchHandlers handlers;
        private readonly Func<AdventureWorksDbContext> dbContextFactory;
        private readonly FastContainer iocContainer;

        public CommandDispatcher(DispatchHandlers handlers, FastContainer iocContainer, Func<AdventureWorksDbContext> dbContextFactory)
        {
            this.handlers = handlers;
            this.iocContainer = iocContainer;
            this.dbContextFactory = dbContextFactory;
        }

        public TResult Dispatch<TResult>(IAPICommand command)
        {
            var sw = Stopwatch.StartNew();
            var commandContext = new CommandContext();
            commandContext.CurrentCommand = command;
            using (var childContainer = iocContainer.Clone())
            {
                childContainer.Register(commandContext);
                try
                {
                    OnDispatching(commandContext);
                    var handler = handlers.FindHandler(command.GetType());
                    var result = (TResult)handler.Execute(ResolveParameters(handler, command, childContainer));
                    OnDispatched(commandContext);
                    // TODO: logging and other instrumentation
                    sw.Stop();
                    return result;
                }
                catch (Exception e)
                {
                    OnDispatched(commandContext, e);
                    sw.Stop();
                    // TODO: logging
                    throw;
                }
            }
        }

        private object[] ResolveParameters(ActionMethodDispatcher handler, object command, FastContainer childContainer)
        {
            var parameters = new object[handler.ParameterTypes.Count];
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameterType = handler.ParameterTypes[i];
                if (parameterType == command.GetType())
                {
                    parameters[i] = command; // shortcut for command, it's a known type
                    continue;
                }
                parameters[i] = childContainer.Resolve(parameterType);
            }
            return parameters;
        }

        private void OnDispatching(CommandContext commandContext)
        {
            // TODO: begin unit of work, other pipeline operations here
            commandContext.DbContext = dbContextFactory();
            commandContext.DbContext.Database.BeginTransaction(IsolationLevel.RepeatableRead);
        }

        private void OnDispatched(CommandContext commandContext, Exception exception = null)
        {
            try
            {
                if (exception == null)
                {
                    // TODO: commit unit of work
                    commandContext.DbContext.SaveChanges();
                    commandContext.DbContext.Database.CurrentTransaction.Commit();
                }
                else
                {
                    try
                    {
                        if (commandContext.DbContext != null && commandContext.DbContext.Database.CurrentTransaction != null)
                        {
                            commandContext.DbContext.Database.CurrentTransaction.Rollback();
                        }
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
                if (commandContext.DbContext != null)
                {
                    commandContext.DbContext.Dispose();
                    commandContext.DbContext = null;
                }
            }
        }
    }
}