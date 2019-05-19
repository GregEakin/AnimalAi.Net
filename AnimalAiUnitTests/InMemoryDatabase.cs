/*
 * Copyright 2019 Greg Eakin
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at:
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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