using System;
using System.Collections.Generic;
using System.Reflection;
using AnimalAi.Data;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Tool.hbm2ddl;

namespace AnimalAi
{
    public sealed class AnimalRepository : IDisposable
    {
        private readonly ISession _session;
         
        public ISessionFactory SessionFactory { get; set; }

        public AnimalRepository(string connection, bool execute)
        {
            var cfg = new Configuration();
            cfg.DataBaseIntegration(x =>
            {
                x.ConnectionString = connection;
                x.Driver<SqlClientDriver>();
                x.Dialect<MsSql2012Dialect>();
                x.BatchSize = 1000;
                x.LogFormattedSql = false;
                x.LogSqlInConsole = false;
            });

            var executingAssembly = Assembly.GetExecutingAssembly();
            cfg.AddAssembly(executingAssembly);
            //var libAssembly = typeof(Question).Assembly;
            //cfg.AddAssembly(libAssembly);

            new SchemaExport(cfg).SetOutputFile("schema.sql").Execute(execute, execute, false);

            var factory = cfg.BuildSessionFactory();
            _session = factory.OpenSession();
        }

        public void Dispose()
        {
            _session?.Dispose();
        }

        public void SetupDb()
        {
            using (var tx = _session.BeginTransaction())
            {
                var question = new Question { Data = "Does it swim?", Parent = null, Answer = null };
                var animal1 = new Animal { Name = "fish", Parent = question, Answer = true };
                var animal2 = new Animal { Name = "bird", Parent = question, Answer = false };

                _session.Save(question);
                _session.Save(animal1);
                _session.Save(animal2);
                tx.Commit();
            }
        }

        public Question GetFirstQuestion()
        {
            var question = _session.QueryOver<Question>().Where(x => x.Parent == null && x.Answer == null).SingleOrDefault();
            return question;
        }

        public Question GetNextQuestion(Question parent, bool answer)
        {
            var question = _session.QueryOver<Question>().Where(x => x.Parent == parent && x.Answer == answer).SingleOrDefault();
            return question;
        }

        public Animal GetAnimal(Question parent, bool answer)
        {
            var animal = _session.QueryOver<Animal>().Where(x => x.Parent == parent && x.Answer == answer).SingleOrDefault();
            return animal;
        }

        public void SaveNewQuestion(Question question, Animal a1, Animal a2)
        {
            using (var tx = _session.BeginTransaction())
            {
                _session.Save(question);
                _session.Save(a1);
                _session.Update(a2);

                tx.Commit();
            }
        }

        public IList<Animal> DumpAllAnimals()
        {
            var animals = _session.QueryOver<Animal>().OrderBy(x => x.Name).Asc;
            return animals.List();
        }
    }
}