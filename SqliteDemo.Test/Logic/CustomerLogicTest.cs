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

                    // 在第2次呼叫把物件加入清單
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

        [Test(Description = "測試空白參數")]
        public void AddCustomerAsync_ArgumentException()
        {
            var errorData = new Customer
            {
                CustomerID = string.Empty,
                CompanyName = string.Empty
            };
            string? nullString = null;

            var fakeRepository = Substitute.For<ICustomerRepository>();

            var fakeFactory = Substitute.For<IRepositoryFactory>();
            fakeFactory
                .GetSqliteRepository<ICustomerRepository>()
                .Returns(fakeRepository);

            using var logic = new CustomerLogic(Substitute.For<IBusinessLogicFactory>(), _settings, _recorder, fakeFactory);

            Assert.ThrowsAsync<ArgumentException>(() => logic.AddCustomerAsync(errorData), "CustomerID is null or empty.");

            fakeRepository.DidNotReceive().AddCustomerAsync(Arg.Compat.Any<Customer>(), Arg.Compat.Any<int>(), Arg.Compat.Any<CancellationToken>());
            fakeRepository.DidNotReceive().GetCustomersAsync(Arg.Compat.Any<string?>(), Arg.Is(nullString), Arg.Is(nullString), Arg.Is(nullString), Arg.Compat.Any<int>(), Arg.Compat.Any<CancellationToken>());
        }

        [Test(Description = "測試資料已存在")]
        public void AddCustomerAsync_InvalidOperationException()
        {
            string? nullString = null;
            var fakeCustomer = _fixture.Create<Customer>();

            var fakeRepository = Substitute.For<ICustomerRepository>();
            fakeRepository
                .GetCustomersAsync(Arg.Compat.Any<string?>(), Arg.Is(nullString), Arg.Is(nullString), Arg.Is(nullString), Arg.Compat.Any<int>(), Arg.Compat.Any<CancellationToken>())
                .Returns(_fixture.CreateMany<Customer>(1));

            var fakeFactory = Substitute.For<IRepositoryFactory>();
            fakeFactory
                .GetSqliteRepository<ICustomerRepository>()
                .Returns(fakeRepository);

            using var logic = new CustomerLogic(Substitute.For<IBusinessLogicFactory>(), _settings, _recorder, fakeFactory);

            Assert.ThrowsAsync<InvalidOperationException>(() => logic.AddCustomerAsync(fakeCustomer), $"The CustomerID {fakeCustomer.CustomerID} is exists.");

            fakeRepository.DidNotReceive().AddCustomerAsync(Arg.Compat.Any<Customer>(), Arg.Compat.Any<int>(), Arg.Compat.Any<CancellationToken>());
            fakeRepository.Received().GetCustomersAsync(Arg.Compat.Any<string?>(), Arg.Is(nullString), Arg.Is(nullString), Arg.Is(nullString), Arg.Compat.Any<int>(), Arg.Compat.Any<CancellationToken>());
        }

        [Test(Description = "更新customer"), Ignore("避免存取DB")]
        public void UpdateCustomerAsync_Success()
        {
            var data = new Customer
            {
                CustomerID = "TEST12345",
                CompanyName = _fixture.Create<string>()
            };

            using var factory = new BusinessLogicFactory();
            var logic = factory.GetLogic<ICustomerLogic>();
            var actual = logic.UpdateCustomerAsync(data.CustomerID, data).Result;

            Assert.Multiple(() =>
            {
                Assert.That(actual.CustomerID, Is.EqualTo(data.CustomerID));
                Assert.That(actual.CompanyName, Is.EqualTo(data.CompanyName));
            });
        }

        [Test(Description = "刪除customer"), Ignore("避免存取DB")]
        public void DeleteCustomerAsync_Success()
        {
            var customerID = "TEST12345";

            using var factory = new BusinessLogicFactory();
            var logic = factory.GetLogic<ICustomerLogic>();
            var actual = logic.DeleteCustomerAsync(customerID).Result;

            Assert.Multiple(() =>
            {
                Assert.That(actual.CustomerID, Is.EqualTo(customerID));
                Assert.That(actual.IsDelete, Is.True);
            });
        }
    }
}
