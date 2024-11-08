using Dapper;
using System.Data;

namespace SqliteDemo.Repository.Base
{
    public interface ISqliteRepository : IDisposable
    {
        /// <summary>
        /// 資料庫連線
        /// </summary>
        IDbConnection Connection { get; }
    }

    internal abstract class SqliteRepository : ISqliteRepository
    {
        public IDbConnection Connection { get; private set; }

        protected SqliteRepository(IDbConnection Connection)
        {
            this.Connection = Connection ?? throw new ArgumentNullException(nameof(Connection));
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Connection != null && Connection.State == ConnectionState.Closed && Disposing)
            {
                Connection.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 取得執行命令
        /// </summary>
        /// <param name="SqlCommandType">命令類型</param>
        /// <param name="SqlCommandText">命令文字</param>
        /// <param name="Parameters">執行參數</param>
        /// <param name="Transaction">交易</param>
        /// <param name="Timeout">逾時秒數</param>
        /// <returns></returns>
        protected virtual CommandDefinition GetCommand(CommandType SqlCommandType, string SqlCommandText, object? Parameters = null, IDbTransaction? Transaction = null, int Timeout = 30, CancellationToken SqlCancellationToken = default) => new CommandDefinition(SqlCommandText, Parameters, commandType: SqlCommandType, transaction: Transaction, commandTimeout: Timeout, cancellationToken: SqlCancellationToken);

        /// <summary>
        /// 轉換欄位字串
        /// </summary>
        /// <typeparam name="TModel">資料表模型</typeparam>
        /// <returns></returns>
        protected virtual string ConvertToColumnString<TModel>() where TModel : class
        {
            var properties = new List<string>();

            foreach (var property in typeof(TModel).GetProperties().Select(p => new { p.Name }))
            {
                properties.Add(property.Name);
            }

            return string.Join(", ", properties);
        }
    }
}
