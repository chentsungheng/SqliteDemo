using Dapper;
using SqliteDemo.Model;
using SqliteDemo.Repository.Base;
using System.Data;
using System.Text;

namespace SqliteDemo.Repository
{
    public interface ICustomerRepository : ISqliteRepository
    {
        Task<IEnumerable<Customer>> GetCustomersAsync(string? CustomerID, string? CompanyName, string? Region, string? PostalCode, int Timeout = 30, CancellationToken SqlCancellationToken = default);

        Task<int> AddCustomerAsync(Customer Data, int Timeout = 30, CancellationToken SqlCancellationToken = default);
    }

    internal class CustomerRepository : SqliteRepository, ICustomerRepository
    {
        public CustomerRepository(IDbConnection Connection) : base(Connection)
        {
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync(string? CustomerID, string? CompanyName, string? Region, string? PostalCode, int Timeout = 30, CancellationToken SqlCancellationToken = default)
        {
            var columns = ConvertToColumnString<Customer>();
            var command = new StringBuilder($"SELECT {columns} FROM Customers WHERE 1 = 1");
            var parameters = new
            {
                CustomerID,
                CompanyName,
                Region,
                PostalCode
            };

            if (!string.IsNullOrEmpty(CustomerID))
            {
                command.Append($" AND {nameof(CustomerID)} = @{nameof(CustomerID)}");
            }
            if (!string.IsNullOrEmpty(CompanyName))
            {
                command.Append($" AND {nameof(CompanyName)} = @{nameof(CompanyName)}");
            }
            if (!string.IsNullOrEmpty(Region))
            {
                command.Append($" AND {nameof(Region)} = @{nameof(Region)}");
            }
            if (!string.IsNullOrEmpty(PostalCode))
            {
                command.Append($" AND {nameof(PostalCode)} = @{nameof(PostalCode)}");
            }

            return await Connection.QueryAsync<Customer>(GetCommand(CommandType.Text, command.ToString(), parameters, null, Timeout, SqlCancellationToken));
        }

        public async Task<int> AddCustomerAsync(Customer Data, int Timeout = 30, CancellationToken SqlCancellationToken = default)
        {
            var columns = ConvertToColumnString<Customer>();
            var command = new StringBuilder($"INSERT INTO Customers ({columns}) VALUES (@{nameof(Data.CustomerID)}, @{nameof(Data.CompanyName)}, @{nameof(Data.ContactName)}, @{nameof(Data.ContactTitle)}, @{nameof(Data.Address)}, @{nameof(Data.City)}, @{nameof(Data.Region)}, @{nameof(Data.PostalCode)}, @{nameof(Data.Country)}, @{nameof(Data.Phone)}, @{nameof(Data.Fax)})");
            var parameters = new
            {
                Data.CustomerID,
                Data.CompanyName,
                Data.ContactName,
                Data.ContactTitle,
                Data.Address,
                Data.City,
                Data.Region,
                Data.PostalCode,
                Data.Country,
                Data.Phone,
                Data.Fax
            };

            return await Connection.ExecuteAsync(GetCommand(CommandType.Text, command.ToString(), parameters, null, Timeout, SqlCancellationToken));
        }
    }
}
