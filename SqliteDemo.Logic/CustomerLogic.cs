using Microsoft.Data.Sqlite;
using SqliteDemo.Logic.Base;
using SqliteDemo.Model;
using SqliteDemo.Repository;

namespace SqliteDemo.Logic
{
    public interface ICustomerLogic : IDataLogic
    {
        Task<IEnumerable<Customer>> GetCustomersAsync(string? CustomerID, string? CompanyName, string? Region, string? PostalCode, CancellationToken TaskCancellationToken = default);

        Task<Customer> AddCustomerAsync(Customer NewCustomer, CancellationToken TaskCancellationToken = default);
    }

    internal class CustomerLogic : DataLogic, ICustomerLogic
    {
        public CustomerLogic(IBusinessLogicFactory BusinessLogicFactory, AppSettings? Settings = null, ILogRecorder? LogRecorder = null, IRepositoryFactory? RepositoryFactory = null) : base(BusinessLogicFactory, Settings, LogRecorder, RepositoryFactory)
        {
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync(string? CustomerID, string? CompanyName, string? Region, string? PostalCode, CancellationToken TaskCancellationToken = default)
        {
            try
            {
                var context = CreateSqliteRepository<ICustomerRepository>();

                return await context.GetCustomersAsync(CustomerID, CompanyName, Region, PostalCode, DefaultTimeout, TaskCancellationToken);
            }
            catch (Exception ex)
            {
                throw LogException(ex);
            }
        }

        public async Task<Customer> AddCustomerAsync(Customer NewCustomer, CancellationToken TaskCancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(NewCustomer.CustomerID))
                {
                    throw new ArgumentException($"{nameof(NewCustomer.CustomerID)} is null or empty.");
                }
                if (string.IsNullOrEmpty(NewCustomer.CompanyName))
                {
                    throw new ArgumentException($"{nameof(NewCustomer.CompanyName)} is null or empty.");
                }

                var context = CreateSqliteRepository<ICustomerRepository>();
                var exists = await context.GetCustomersAsync(NewCustomer.CustomerID, null, null, null, DefaultTimeout, TaskCancellationToken);

                // 相同ID已存在
                if (exists.Any())
                {
                    throw new InvalidOperationException($"The {nameof(NewCustomer.CustomerID)} {NewCustomer.CustomerID} is exists.");
                }

                using var transaction = OpenTransactionScope(DefaultTimeout);
                var rows = await context.AddCustomerAsync(NewCustomer, DefaultTimeout, TaskCancellationToken);

                // 新增失敗
                if (rows == 0)
                {
                    throw new SqliteException("Add customer is failed.", 1);
                }

                transaction.Complete();

                exists = await context.GetCustomersAsync(NewCustomer.CustomerID, null, null, null, DefaultTimeout, TaskCancellationToken);

                return exists.Single();
            }
            catch (Exception ex)
            {
                throw LogException(ex);
            }
        }
    }
}
