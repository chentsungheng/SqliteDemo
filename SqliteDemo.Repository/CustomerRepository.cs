using Dapper;
using SqliteDemo.Model;
using SqliteDemo.Repository.Base;
using System.Data;

namespace SqliteDemo.Repository
{
    public interface ICustomerRepository : ISqliteRepository
    {
        Task<IEnumerable<Customer>> GetCustomersAsync(string? CustomerID, string? CompanyName, string? Region, string? PostalCode, int Timeout = 30, CancellationToken SqlCancellationToken = default);
    }

    internal class CustomerRepository : SqliteRepository, ICustomerRepository
    {
        public CustomerRepository(IDbConnection Connection) : base(Connection)
        {
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync(string? CustomerID, string? CompanyName, string? Region, string? PostalCode, int Timeout = 30, CancellationToken SqlCancellationToken = default)
        {
            var columns = ConvertToColumnString<Customer>();
            var command = $"SELECT {columns} FROM Customers WHERE 1 = 1";

            return await Connection.QueryAsync<Customer>(GetCommand(CommandType.Text, command, null, null, Timeout, SqlCancellationToken));
        }
    }
}
