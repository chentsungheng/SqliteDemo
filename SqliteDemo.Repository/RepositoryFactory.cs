using Microsoft.Data.Sqlite;
using Ninject;
using Ninject.Parameters;
using SqliteDemo.Model;
using System.Data;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SqliteDemo.Test")]
namespace SqliteDemo.Repository
{
    public interface IRepositoryFactory : IDisposable
    {
        TRepository GetSqliteRepository<TRepository>();
    }

    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly StandardKernel _kernel;
        private readonly AppSettings _appSettings;

        private readonly string Connection = nameof(Connection);
        protected virtual IDbConnection? DbConnection { get; private set; }

        public RepositoryFactory(AppSettings Settings)
        {
            _kernel = new StandardKernel();
            _appSettings = Settings ?? throw new ArgumentNullException(typeof(AppSettings).Name);

            InitialConnection();
            InitialContainer();
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing && DbConnection != null && DbConnection.State == ConnectionState.Closed)
            {
                DbConnection.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void InitialConnection()
        {
            var builder = new SqliteConnectionStringBuilder() { DataSource = _appSettings.SqliteDatabase[_appSettings.Stage].Path };
            DbConnection = new SqliteConnection(builder.ConnectionString);
        }

        private void InitialContainer()
        {
            _kernel
                .Bind<ICustomerRepository>()
                .To<CustomerRepository>()
                .InSingletonScope()
                .WithConstructorArgument(Connection, context => null);
        }

        public TRepository GetSqliteRepository<TRepository>()
        {
            if (DbConnection == null)
            {
                throw new InvalidOperationException($"{nameof(DbConnection)} is null.");
            }

            var inject = new IParameter[]
            {
                new ConstructorArgument(Connection, DbConnection)
            };

            return _kernel.Get<TRepository>(inject);
        }
    }
}
