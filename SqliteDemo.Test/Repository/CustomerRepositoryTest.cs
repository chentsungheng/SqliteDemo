﻿using AutoFixture;
using SqliteDemo.Logic.Base;
using SqliteDemo.Model;
using SqliteDemo.Repository;

namespace SqliteDemo.Test.Repository
{
    internal class CustomerRepositoryTest
    {
        private AppSettings _settings;
        private Fixture _fixture;

        [OneTimeSetUp]
        public void Init()
        {
            _settings = BusinessLogic.GetAppSettings();
            _fixture = new Fixture();
        }

        [Test(Description = "成功取得Customer資料列"), Ignore("避免存取DB")]
        public void GetCustomers_Success()
        {
            using var factory = new RepositoryFactory(_settings);
            var context = factory.GetSqliteRepository<ICustomerRepository>();
            var actual = context.GetCustomersAsync("FISSA", null, null, "28034").Result;

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.Count(), Is.GreaterThan(0));
        }

        [Test(Description = "成功新增資料"), Ignore("避免存取DB")]
        public void AddCustomerAsync_Success()
        {
            var data = _fixture.Build<Customer>().With(c => c.CustomerID, "TEST123").Create();

            using var factory = new RepositoryFactory(_settings);
            var context = factory.GetSqliteRepository<ICustomerRepository>();
            var actual = context.AddCustomerAsync(data).Result;

            Assert.That(actual, Is.GreaterThan(0));
        }

        [Test(Description = "成功更新資料"), Ignore("避免存取DB")]
        public void UpdateCustomerAsync_Success()
        {
            var data = new Customer
            {
                CustomerID = string.Empty,
                CompanyName = "CompanyName123",
                ContactName = "ContactName123",
                ContactTitle = "ContactTitle123",
                Address = null,
                City = null,
                Region = null,
                PostalCode = null,
                Country = null,
                Phone = null,
                Fax = null
            };

            using var factory = new RepositoryFactory(_settings);
            var context = factory.GetSqliteRepository<ICustomerRepository>();
            var actual = context.UpdateCustomerAsync("TEST123", data).Result;

            Assert.That(actual, Is.GreaterThan(0));
        }

        [Test(Description = "成功刪除資料"), Ignore("避免存取DB")]
        public void DeleteCustomerAsync_Success()
        {
            using var factory = new RepositoryFactory(_settings);
            var context = factory.GetSqliteRepository<ICustomerRepository>();
            var actual = context.DeleteCustomerAsync("TEST123").Result;

            Assert.That(actual, Is.GreaterThan(0));
        }
    }
}
