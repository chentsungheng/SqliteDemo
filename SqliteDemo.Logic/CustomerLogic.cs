using SqliteDemo.Logic.Base;
using SqliteDemo.Model;
using SqliteDemo.Repository;

namespace SqliteDemo.Logic
{
    public interface ICustomerLogic : IDataLogic
    {
        Task<IEnumerable<Customer>> GetCustomersAsync(string? CustomerID, string? CompanyName, string? Region, string? PostalCode, CancellationToken TaskCancellationToken = default);
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
    }
}
