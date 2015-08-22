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
        private readonly Client client;
        private readonly DispatchHandlers handlers;
        private readonly Func<AdventureWorksDbContext> dbContextFactory;

        public CommandDispatcher(Client client, DispatchHandlers handlers, Func<AdventureWorksDbContext> dbContextFactory)
        {
            this.client = client;
            this.handlers = handlers;
            this.dbContextFactory = dbContextFactory;
        }

        public TResult Dispatch<TResult>(IAPICommand command)
        {
            var sw = Stopwatch.StartNew();
            var commandContext = new CommandContext();
            commandContext.CurrentCommand = command;
            try
            {
                OnDispatching(commandContext);
                var handler = handlers.FindHandler(command.GetType());
                var result = (TResult)handler.Invoke(command, commandContext);
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