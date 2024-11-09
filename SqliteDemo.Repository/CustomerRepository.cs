using Dapper;
using SqliteDemo.Model;
using SqliteDemo.Repository.Base;
using System.Data;
using System.Text;

namespace SqliteDemo.Repository
{
    public interface ICustomerRepository : ISqliteRepository
    {
        Task<IEnumerable<Customer>> GetCustomersAsync(string? CustomerID, string? CompanyName, string? Region, string? PostalCode, IDbTransaction? Transaction = null, int Timeout = 30, CancellationToken SqlCancellationToken = default);

        Task<int> AddCustomerAsync(Customer Data, IDbTransaction? Transaction = null, int Timeout = 30, CancellationToken SqlCancellationToken = default);

        Task<int> UpdateCustomerAsync(string CustomerID, CustomerForUpdate Data, IDbTransaction? Transaction = null, int Timeout = 30, CancellationToken SqlCancellationToken = default);

        Task<int> DeleteCustomerAsync(string CustomerID, IDbTransaction? Transaction = null, int Timeout = 30, CancellationToken SqlCancellationToken = default);
    }

    internal class CustomerRepository : SqliteRepository, ICustomerRepository
    {
        public CustomerRepository(IDbConnection Connection) : base(Connection)
        {
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync(string? CustomerID, string? CompanyName, string? Region, string? PostalCode, IDbTransaction? Transaction = null, int Timeout = 30, CancellationToken SqlCancellationToken = default)
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

            return await Connection.QueryAsync<Customer>(GetCommand(CommandType.Text, command.ToString(), parameters, Transaction, Timeout, SqlCancellationToken));
        }

        public async Task<int> AddCustomerAsync(Customer Data, IDbTransaction? Transaction = null, int Timeout = 30, CancellationToken SqlCancellationToken = default)
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

            return await Connection.ExecuteAsync(GetCommand(CommandType.Text, command.ToString(), parameters, Transaction, Timeout, SqlCancellationToken));
        }

        public async Task<int> UpdateCustomerAsync(string CustomerID, CustomerForUpdate Data, IDbTransaction? Transaction = null, int Timeout = 30, CancellationToken SqlCancellationToken = default)
        {
            var values = new List<string>();
            var parameters = new
            {
                CustomerID,
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

            foreach (var property in Data.GetType().GetProperties().Select(p => new { p.Name }))
            {
                values.Add($" {property.Name} = @{property.Name}");
            }

            var command = new StringBuilder("UPDATE Customers SET");
            command.Append(string.Join(',', values));
            command.Append($" WHERE {nameof(CustomerID)} = @{nameof(CustomerID)}");

            return await Connection.ExecuteAsync(GetCommand(CommandType.Text, command.ToString(), parameters, Transaction, Timeout, SqlCancellationToken));
        }

        public async Task<int> DeleteCustomerAsync(string CustomerID, IDbTransaction? Transaction = null, int Timeout = 30, CancellationToken SqlCancellationToken = default)
        {
            var command = $"DELETE FROM Customers WHERE {nameof(CustomerID)} = @{nameof(CustomerID)}";
            var parameters = new
            {
                CustomerID
            };

            return await Connection.ExecuteAsync(GetCommand(CommandType.Text, command, parameters, Transaction, Timeout, SqlCancellationToken));
        }
    }
}
