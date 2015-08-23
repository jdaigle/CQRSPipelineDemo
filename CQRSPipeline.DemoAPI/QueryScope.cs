using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;

namespace CQRSPipeline.DemoAPI
{
    public class QueryScope : IDisposable
    {
        public QueryScope(Client client, Func<DbConnection> dbConnectionFactory)
        {
            this.Client = client;
            this.scopedConnection = dbConnectionFactory();
            this.scopedConnection.Open();
            this.scopedTransaction = this.CurrentConnection.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public Client Client { get; private set; }

        private DbConnection scopedConnection;
        private DbTransaction scopedTransaction;

        public DbConnection CurrentConnection
        {
            get
            {
                if (isDisposed)
                {
                    throw new ObjectDisposedException("QueryScope");
                }
                return scopedConnection;
            }
        }
        public DbTransaction CurrentTransaction
        {
            get
            {
                if (isDisposed)
                {
                    throw new ObjectDisposedException("QueryScope");
                }
                return scopedTransaction;
            }
        }

        private bool isDisposed = false;
        private int disposeSignaled = 0;

        public void Commit()
        {
            Dispose();
        }

        public void Rollback()
        {
            try
            {
                if (CurrentTransaction != null)
                {
                    CurrentTransaction.Rollback();
                }
            }
            catch (Exception)
            {
                // do not throw;
                // TODO: logging if necessary
            }
            finally
            {
                if (CurrentTransaction != null)
                {
                    CurrentTransaction.Dispose();
                    scopedTransaction = null;
                }
                Dispose();
            }
        }

        public event EventHandler Disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposeSignaled, 1) != 0)
            {
                return;
            }

            try
            {
                if (CurrentTransaction != null)
                {
                    CurrentTransaction.Commit();
                }
            }
            finally
            {
                if (CurrentTransaction != null)
                {
                    CurrentTransaction.Dispose();
                    scopedTransaction = null;
                }
                if (CurrentConnection != null)
                {
                    CurrentConnection.Dispose();
                    scopedConnection = null;
                }
                if (Disposed != null)
                {
                    Disposed(this, EventArgs.Empty);
                }
            }

            isDisposed = true;
        }
    }
}
