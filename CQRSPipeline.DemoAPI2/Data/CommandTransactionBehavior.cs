using System;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using CQRSPipeline.Framework;

namespace CQRSPipeline.DemoAPI.Data
{
    public class CommandTransactionBehavior : PipelinedBehavior
    {

        public override void Invoke(CommandContext context, Action<CommandContext> next)
        {
            using (var dbContext = context.SingleInstanceFactory(typeof(DbContext)) as DbContext)
            {
                // commands, by default, will be RepeatableRead
                using (var transaction = dbContext.Database.BeginTransaction(IsolationLevel.RepeatableRead))
                {
                    next(context);
                    dbContext.SaveChanges();
                    transaction.Commit();
                }
            }
        }
    }
}
