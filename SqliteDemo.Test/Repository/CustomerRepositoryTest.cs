using SqliteDemo.Logic.Base;
using SqliteDemo.Model;
using SqliteDemo.Repository;

namespace SqliteDemo.Test.Repository
{
    internal class CustomerRepositoryTest
    {
        private AppSettings _settings;

        [OneTimeSetUp]
        public void Init()
        {
            _settings = BusinessLogic.GetAppSettings();
        }

        [Test(Description = "成功取得Customer資料列"), Ignore("避免存取DB")]
        public void GetCustomers_Success()
        {
            using var factory = new RepositoryFactory(_settings);
            var context = factory.GetSqliteRepository<ICustomerRepository>();
            var actual = context.GetCustomersAsync(null, null, null, null).Result;

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.Count(), Is.GreaterThan(0));
        }
    }
}
