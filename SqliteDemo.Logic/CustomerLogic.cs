using Microsoft.Data.Sqlite;
using SqliteDemo.Logic.Base;
using SqliteDemo.Model;
using SqliteDemo.Repository;
using System.Collections.Concurrent;

namespace SqliteDemo.Logic
{
    public interface ICustomerLogic : IDataLogic
    {
        Task<IEnumerable<Customer>> GetCustomersAsync(string? CustomerID, string? CompanyName, string? Region, string? PostalCode, CancellationToken TaskCancellationToken = default);

        Task<Customer> AddCustomerAsync(Customer NewCustomer, CancellationToken TaskCancellationToken = default);

        Task<Customer> UpdateCustomerAsync(string CustomerID, CustomerForUpdate ExistCustomer, CancellationToken TaskCancellationToken = default);

        Task<CustomerDeleted> DeleteCustomerAsync(string CustomerID, CancellationToken TaskCancellationToken = default);
    }

    internal class CustomerLogic : DataLogic, ICustomerLogic
    {
        private static readonly ConcurrentDictionary<string, string> _operationLock = new();

        public CustomerLogic(IBusinessLogicFactory BusinessLogicFactory, AppSettings? Settings = null, ILogRecorder? LogRecorder = null, IRepositoryFactory? RepositoryFactory = null) : base(BusinessLogicFactory, Settings, LogRecorder, RepositoryFactory)
        {
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync(string? CustomerID, string? CompanyName, string? Region, string? PostalCode, CancellationToken TaskCancellationToken = default)
        {
            try
            {
                var context = CreateSqliteRepository<ICustomerRepository>();
                LogEvent("Log search parameters.", new
                {
                    CustomerID,
                    CompanyName,
                    Region,
                    PostalCode
                });

                return await context.GetCustomersAsync(CustomerID, CompanyName, Region, PostalCode, null, DefaultTimeout, TaskCancellationToken);
            }
            catch (Exception ex)
            {
                throw LogException(ex);
            }
        }

        public async Task<Customer> AddCustomerAsync(Customer NewCustomer, CancellationToken TaskCancellationToken = default)
        {
            var islock = false;
            var id = string.Empty;

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
                var exists = await context.GetCustomersAsync(NewCustomer.CustomerID, null, null, null, null, DefaultTimeout, TaskCancellationToken);

                // 相同ID已存在
                if (exists.Any())
                {
                    throw new InvalidOperationException($"The {nameof(NewCustomer.CustomerID)} {NewCustomer.CustomerID} is exists.");
                }
                // 有另一個執行緒正在操作此ID
                if (!_operationLock.TryAdd(NewCustomer.CustomerID, "add"))
                {
                    throw new InvalidOperationException($"The {nameof(NewCustomer.CustomerID)} {NewCustomer.CustomerID} is locked by someone else.");
                }

                islock = true;
                id = NewCustomer.CustomerID;

                using var transaction = OpenDbTransaction();
                var rows = await context.AddCustomerAsync(NewCustomer, transaction, DefaultTimeout, TaskCancellationToken);

                // 新增失敗
                if (rows == 0)
                {
                    throw new SqliteException("Add customer is failed.", 1);
                }

                transaction.Commit();

                exists = await context.GetCustomersAsync(NewCustomer.CustomerID, null, null, null, null, DefaultTimeout, TaskCancellationToken);

                return exists.Single();
            }
            catch (Exception ex)
            {
                throw LogException(ex);
            }
            finally
            {
                CloseDbConnection();

                if (islock && !string.IsNullOrEmpty(id))
                {
                    _operationLock.TryRemove(id, out _);
                }
            }
        }

        public async Task<Customer> UpdateCustomerAsync(string CustomerID, CustomerForUpdate ExistCustomer, CancellationToken TaskCancellationToken = default)
        {
            var islock = false;

            try
            {
                if (string.IsNullOrEmpty(CustomerID))
                {
                    throw new ArgumentNullException(nameof(CustomerID));
                }
                if (string.IsNullOrEmpty(ExistCustomer.CompanyName))
                {
                    throw new ArgumentException($"{nameof(ExistCustomer.CompanyName)} is null or empty.");
                }

                var context = CreateSqliteRepository<ICustomerRepository>();
                var exists = await context.GetCustomersAsync(CustomerID, null, null, null, null, DefaultTimeout, TaskCancellationToken);

                // 資料不存在
                if (exists.Count() != 1)
                {
                    throw new InvalidOperationException($"Unable to determine {CustomerID} information.");
                }
                // 有另一個執行緒正在操作此ID
                if (!_operationLock.TryAdd(CustomerID, "update"))
                {
                    throw new InvalidOperationException($"The {nameof(CustomerID)} {CustomerID} is locked by someone else.");
                }

                islock = true;

                using var transaction = OpenDbTransaction();
                var rows = await context.UpdateCustomerAsync(CustomerID, ExistCustomer, transaction, DefaultTimeout, TaskCancellationToken);

                // 更新失敗
                if (rows == 0)
                {
                    throw new SqliteException("Update customer is failed.", 1);
                }

                transaction.Commit();

                exists = await context.GetCustomersAsync(CustomerID, null, null, null, null, DefaultTimeout, TaskCancellationToken);

                return exists.Single();
            }
            catch (Exception ex)
            {
                throw LogException(ex);
            }
            finally
            {
                CloseDbConnection();

                if (islock)
                {
                    _operationLock.TryRemove(CustomerID, out _);
                }
            }
        }

        public async Task<CustomerDeleted> DeleteCustomerAsync(string CustomerID, CancellationToken TaskCancellationToken = default)
        {
            var islock = false;

            try
            {
                var context = CreateSqliteRepository<ICustomerRepository>();
                var exists = await context.GetCustomersAsync(CustomerID, null, null, null, null, DefaultTimeout, TaskCancellationToken);

                // 資料不存在
                if (exists.Count() != 1)
                {
                    throw new InvalidOperationException($"Unable to determine {CustomerID} information.");
                }
                // 有另一個執行緒正在操作此ID
                if (!_operationLock.TryAdd(CustomerID, "delete"))
                {
                    throw new InvalidOperationException($"The {nameof(CustomerID)} {CustomerID} is locked by someone else.");
                }

                islock = true;

                using var transaction = OpenDbTransaction();
                var rows = await context.DeleteCustomerAsync(CustomerID, transaction, DefaultTimeout, TaskCancellationToken);

                // 刪除失敗
                if (rows == 0)
                {
                    throw new SqliteException("Delete customer is failed.", 1);
                }

                transaction.Commit();

                return new CustomerDeleted
                {
                    CustomerID = CustomerID,
                    IsDelete = true
                };
            }
            catch (Exception ex)
            {
                throw LogException(ex);
            }
            finally
            {
                CloseDbConnection();

                if (islock)
                {
                    _operationLock.TryRemove(CustomerID, out _);
                }
            }
        }
    }
}
