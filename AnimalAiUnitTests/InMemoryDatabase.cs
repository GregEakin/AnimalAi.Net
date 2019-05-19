using AnimalAi.Data;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;

namespace AnimalAiUnitTests
{
    public static class InMemoryDatabase
    {
        public static Configuration SetupConfiguration()
        {
            var cfg = new Configuration();
            cfg.DataBaseIntegration(x =>
            {
                x.ConnectionReleaseMode = ConnectionReleaseMode.OnClose;
                x.Dialect<SQLiteDialect>();
                x.Driver<SQLite20Driver>();
                x.ConnectionString = "data source=:memory:";
                x.BatchSize = 1000;
                x.LogFormattedSql = false;
                x.LogSqlInConsole = false;
            });

            // cfg.SetProperty(Environment.ProxyFactoryFactoryClass, typeof(ProxyFactoryFactory).AssemblyQualifiedName);
            cfg.AddAssembly(typeof(Question).Assembly);

            return cfg;
        }
    }
}