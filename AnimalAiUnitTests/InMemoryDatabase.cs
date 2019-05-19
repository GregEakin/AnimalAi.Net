using AnimalAi.Data;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Reflection;

namespace AnimalAiUnitTests
{
    public sealed class InMemoryDatabase : IDisposable
    {
        private static readonly Lazy<Configuration> Configuration = new Lazy<Configuration>(SetupConfiguration);
        private static readonly Lazy<ISessionFactory> SessionFactory = new Lazy<ISessionFactory>(() => Configuration.Value.BuildSessionFactory());
        private readonly ISession _session;

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

        public InMemoryDatabase()
        {
            _session = SessionFactory.Value.OpenSession();
            new SchemaExport(Configuration.Value).SetOutputFile("schema.sql").Execute(true, true, false);
            // new SchemaExport(Configuration.Value).Execute(true, true, false, true, _session.Connection, Console.Out);
        }

        public void Dispose()
        {
            _session.Dispose();
        }
    }
}