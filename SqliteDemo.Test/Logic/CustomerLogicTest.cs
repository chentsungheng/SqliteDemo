using AutoFixture;
using NSubstitute;
using SqliteDemo.Logic;
using SqliteDemo.Logic.Base;
using SqliteDemo.Model;
using SqliteDemo.Repository;

namespace SqliteDemo.Test.Logic
{
    internal class CustomerLogicTest
    {
        private AppSettings _settings;
        private Fixture _fixture;
        private ILogRecorder _recorder;

        [OneTimeSetUp]
        public void Init()
        {
            _settings = BusinessLogic.GetAppSettings();
            _fixture = new Fixture();
        }

        [SetUp]
        public void Setup()
        {
            _recorder = Substitute.For<ILogRecorder>();
            _recorder.Write(Arg.Compat.Any<string>());
            _recorder.Write(Arg.Compat.Any<string>(), Arg.Compat.Any<IDictionary<string, string>>());
            _recorder.Write(Arg.Compat.Any<Exception>());
            _recorder.Write(Arg.Compat.Any<Exception>(), Arg.Compat.Any<IDictionary<string, string>>());
        }

        [TearDown]
        public void Uninstall()
        {
            _recorder.Dispose();
        }

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

        [Test(Description = "隔離DB並新增customer")]
        public void AddCustomerAsync_Mock_Success()
        {
            var results = new List<Customer>();
            var fakeCustomer = _fixture.Create<Customer>();
            var count = 0;
            string? nullString = null;

            var fakeRepository = Substitute.For<ICustomerRepository>();
            fakeRepository
                .GetCustomersAsync(Arg.Compat.Any<string?>(), Arg.Is(nullString), Arg.Is(nullString), Arg.Is(nullString), Arg.Compat.Any<int>(), Arg.Compat.Any<CancellationToken>())
                .Returns(results);
            fakeRepository
                .When(calling => calling.GetCustomersAsync(Arg.Compat.Any<string>(), Arg.Is(nullString), Arg.Is(nullString), Arg.Is(nullString), Arg.Compat.Any<int>(), Arg.Compat.Any<CancellationToken>()))
                .Do(check =>
                {
                    Assert.That(fakeCustomer.CustomerID, Is.EqualTo(check.ArgAt<string>(0)));

                    if (count++ == 1)
                    {
                        results.Add(fakeCustomer);
                    }
                });
            fakeRepository
                .AddCustomerAsync(Arg.Compat.Any<Customer>(), Arg.Compat.Any<int>(), Arg.Compat.Any<CancellationToken>())
                .Returns(1);

            var fakeFactory = Substitute.For<IRepositoryFactory>();
            fakeFactory
                .GetSqliteRepository<ICustomerRepository>()
                .Returns(fakeRepository);

            using var logic = new CustomerLogic(Substitute.For<IBusinessLogicFactory>(), _settings, _recorder, fakeFactory);
            var actual = logic.AddCustomerAsync(fakeCustomer).Result;

            Assert.That(actual.CustomerID, Is.EqualTo(fakeCustomer.CustomerID));

            fakeRepository.Received().AddCustomerAsync(Arg.Compat.Any<Customer>(), Arg.Compat.Any<int>(), Arg.Compat.Any<CancellationToken>());
            fakeRepository.Received().GetCustomersAsync(Arg.Compat.Any<string?>(), Arg.Is(nullString), Arg.Is(nullString), Arg.Is(nullString), Arg.Compat.Any<int>(), Arg.Compat.Any<CancellationToken>());
        }
    }
}
