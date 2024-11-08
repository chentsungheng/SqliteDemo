using SqliteDemo.Logic;

namespace SqliteDemo.Test.Logic
{
    internal class CustomerLogicTest
    {
        [Test, Ignore("避免存取DB")]
        public void GetCustomersAsync_Success()
        {
            using var factory = new BusinessLogicFactory();
            var logic = factory.GetLogic<ICustomerLogic>();
            var actual = logic.GetCustomersAsync(null, null, null, null).Result;

            Assert.That(actual.Count(), Is.GreaterThan(0));
        }
    }
}
