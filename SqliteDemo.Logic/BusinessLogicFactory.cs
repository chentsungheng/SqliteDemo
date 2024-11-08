using Ninject;
using Ninject.Parameters;
using SqliteDemo.Logic.Base;
using SqliteDemo.Model;
using SqliteDemo.Repository;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SqliteDemo.Test")]
namespace SqliteDemo.Logic
{
    public interface IBusinessLogicFactory : IDisposable
    {
        AppSettings Settings { get; }

        TLogic GetLogic<TLogic>();

        TLogic GetLogic<TLogic>(AppSettings Settings, ILogRecorder LogRecorder);

        TLogic GetLogic<TLogic>(AppSettings Settings, ILogRecorder LogRecorder, IRepositoryFactory? RepositoryFactory);
    }

    public class BusinessLogicFactory : IBusinessLogicFactory
    {
        private readonly string RepositoryFactory = typeof(RepositoryFactory).Name;
        private readonly string LogRecorder = typeof(LogRecorder).Name;
        private readonly StandardKernel _kernel;

        public AppSettings Settings { get; private set; }

        public BusinessLogicFactory()
        {
            Settings = BusinessLogic.GetAppSettings();

            _kernel = new StandardKernel();

            RegisterLogic();
        }

        private void RegisterLogic()
        {
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                _kernel?.Dispose();

                Settings.ApplicationInsights.Clear();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public TLogic GetLogic<TLogic>()
        {
            return _kernel.Get<TLogic>();
        }

        public TLogic GetLogic<TLogic>(AppSettings Settings, ILogRecorder LogRecorder)
        {
            var inject = new IParameter[]
            {
                new ConstructorArgument(GetType().Name, this),
                new ConstructorArgument(nameof(Settings), Settings),
                new ConstructorArgument(nameof(LogRecorder), LogRecorder)
            };

            return _kernel.Get<TLogic>(inject);
        }

        public TLogic GetLogic<TLogic>(AppSettings Settings, ILogRecorder LogRecorder, IRepositoryFactory? RepositoryFactory)
        {
            var inject = new IParameter[]
            {
                new ConstructorArgument(GetType().Name, this),
                new ConstructorArgument(nameof(Settings), Settings),
                new ConstructorArgument(nameof(LogRecorder), LogRecorder),
                new ConstructorArgument(nameof(RepositoryFactory), RepositoryFactory)
            };

            return _kernel.Get<TLogic>(inject);
        }
    }
}
