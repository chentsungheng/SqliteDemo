using SqliteDemo.Logic;
using SqliteDemo.Model;

namespace SqliteDemo.Test.Logic
{
    internal class CustomerLogicTest
    {
        [Test(Description = "讀取customer"), Ignore("避免存取DB")]
        public void GetCustomersAsync_Success()
        {
            using var factory = new BusinessLogicFactory();
            var logic = factory.GetLogic<ICustomerLogic>();
            var actual = logic.GetCustomersAsync(null, null, null, null).Result;

            Assert.That(actual.Count(), Is.GreaterThan(0));
        }

        [Test(Description = "新增customer"), Ignore("避免存取DB")]
        public void AddCustomerAsync_Success()
        {
            var data = new Customer
            {
                CustomerID = "TEST12345",
                CompanyName = "Test"
            };

            using var factory = new BusinessLogicFactory();
            var logic = factory.GetLogic<ICustomerLogic>();
            var actual = logic.AddCustomerAsync(data).Result;

            Assert.That(actual.CustomerID, Is.EqualTo(data.CustomerID));
        }
    }
}
