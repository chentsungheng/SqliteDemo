using SqliteDemo.Model;
using SqliteDemo.Repository;
using System.Transactions;

namespace SqliteDemo.Logic.Base
{
    /// <summary>
    /// 資料導向邏輯介面
    /// </summary>
    public interface IDataLogic : IBusinessLogic
    {
        /// <summary>
        /// 資料儲存庫
        /// </summary>
        IRepositoryFactory RepositoryFactory { get; }

        /// <summary>
        /// 是否自動釋放Repository
        /// </summary>
        bool IsAutoDisposeRepository { get; }
    }

    public abstract class DataLogic : BusinessLogic, IDataLogic
    {
        public IRepositoryFactory RepositoryFactory { get; private set; }

        public bool IsAutoDisposeRepository { get; private set; }

        protected int DefaultTimeout { get; set; }

        protected DataLogic(IBusinessLogicFactory BusinessLogicFactory, AppSettings? Settings = null, ILogRecorder? LogRecorder = null, IRepositoryFactory? RepositoryFactory = null) : base(BusinessLogicFactory, Settings, LogRecorder)
        {
            IsAutoDisposeRepository = RepositoryFactory == null;
            this.RepositoryFactory = RepositoryFactory ?? new RepositoryFactory(this.Settings);

            DefaultTimeout = 20;
        }

        protected override void Dispose(bool Disposing)
        {
            if (RepositoryFactory != null && Disposing && IsAutoDisposeRepository)
            {
                RepositoryFactory.Dispose();
            }

            base.Dispose(Disposing);
        }

        /// <summary>
        /// 取得資料導向邏輯
        /// </summary>
        /// <typeparam name="TLogic">邏輯型別</typeparam>
        /// <returns></returns>
        protected override TLogic CreateDataLogic<TLogic>()
        {
            return BusinessLogicFactory.GetLogic<TLogic>(Settings, LogRecorder, RepositoryFactory);
        }

        /// <summary>
        /// 開啟資料交易
        /// </summary>
        /// <returns></returns>
        protected virtual TransactionScope OpenTransactionScope(double TransactionTimeout, IsolationLevel TransactionIsolation = IsolationLevel.ReadCommitted)
        {
            if (Transaction.Current == null)
            {
                return new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = TransactionIsolation, Timeout = TimeSpan.FromSeconds(TransactionTimeout) }, TransactionScopeAsyncFlowOption.Enabled);
            }
            else
            {
                return new TransactionScope(Transaction.Current, TransactionScopeAsyncFlowOption.Enabled);
            }
        }

        /// <summary>
        /// 建立Sqlite儲存體物件
        /// </summary>
        /// <typeparam name="TRepository">儲存體型別</typeparam>
        /// <returns></returns>
        protected virtual TRepository CreateSqliteRepository<TRepository>()
        {
            return RepositoryFactory.GetSqliteRepository<TRepository>();
        }
    }
}
