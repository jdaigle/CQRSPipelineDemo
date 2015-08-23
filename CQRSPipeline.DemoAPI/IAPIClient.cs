using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRSPipeline.DemoAPI
{
    public interface IAPIClient
    {
        /// <summary>
        /// Executes a command in a transaction and returns no value.
        /// </summary>
        void Execute(IAPICommand command);

        /// <summary>
        /// Executes a command in a transaction and returns a value.
        /// </summary>
        TResult Execute<TResult>(IAPICommand<TResult> command);

        /// <summary>
        /// Executs a query and returns the result.
        /// </summary>
        TResult Query<TResult>(IAPIQuery<TResult> query);

        /// <summary>
        /// Opens a new query scope to execute multiple queries within
        /// the same transaction
        /// </summary>
        QueryScope QueryScope();
    }
}
