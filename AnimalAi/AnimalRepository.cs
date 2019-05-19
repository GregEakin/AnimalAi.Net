﻿using AnimalAi.Data;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AnimalAi
{
    public sealed class AnimalRepository : IDisposable
    {
        private readonly ISession _session;

        // public ISessionFactory SessionFactory { get; set; }

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

        public AnimalRepository(ISession session)
        {
            _session = session;
        }

        public void Dispose()
        {
            _session?.Dispose();
        }

        public void SetupDb()
        {
            using (var tx = _session.BeginTransaction())
            {
                _session.CreateQuery("delete Animal a").ExecuteUpdate();
                _session.CreateQuery("delete Question q").ExecuteUpdate();

                var question = new Question {Data = "Does it swim?", Parent = null, Answer = null};
                var animal1 = new Animal {Name = "fish", Parent = question, Answer = true};
                var animal2 = new Animal {Name = "bird", Parent = question, Answer = false};

                _session.Save(question);
                _session.Save(animal1);
                _session.Save(animal2);
                tx.Commit();
            }
        }

        public Question GetQuestion()
        {
            using (_session.BeginTransaction())
            {
                var question = _session.QueryOver<Question>().Where(x => x.Parent == null && x.Answer == null)
                    .SingleOrDefault();
                return question;
            }
        }

        public Question GetQuestion(Question parent, bool answer)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            using (_session.BeginTransaction())
            {
                var question = _session.QueryOver<Question>().Where(x => x.Parent == parent && x.Answer == answer)
                    .SingleOrDefault();
                return question;
            }
        }

        public Animal GetAnimal(Question parent, bool answer)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            using (_session.BeginTransaction())
            {
                var animal = _session.QueryOver<Animal>().Where(x => x.Parent == parent && x.Answer == answer)
                    .SingleOrDefault();
                return animal;
            }
        }

        public Animal GetAnimal(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            using (_session.BeginTransaction())
            {
                var animal = _session.QueryOver<Animal>().Where(x => x.Name == name).SingleOrDefault();
                return animal;
            }
        }

        private void SaveNewQuestion(Question newQuestion, Animal newAnimal, Animal existingAnimal)
        {
            if (newQuestion == null)
                throw new ArgumentNullException(nameof(newQuestion));
            if (newAnimal == null)
                throw new ArgumentNullException(nameof(newAnimal));
            if (existingAnimal == null)
                throw new ArgumentNullException(nameof(existingAnimal));

            using (var tx = _session.BeginTransaction())
            {
                _session.Save(newQuestion);
                _session.Save(newAnimal);
                _session.Update(existingAnimal);

                tx.Commit();
            }
        }

        public IList<Animal> FindAllAnimals()
        {
            using (_session.BeginTransaction())
            {
                var animals = _session.QueryOver<Animal>().OrderBy(x => x.Name).Asc;
                return animals.List();
            }
        }

        public Tuple<Question, Animal> AddAnimal(Question parent, bool answer, ref Animal existingAnimal,
            string newAnimal, string newQuestion, bool newAnswer)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (string.IsNullOrWhiteSpace(newAnimal))
                throw new ArgumentNullException(nameof(newAnimal));
            if (string.IsNullOrWhiteSpace(newQuestion))
                throw new ArgumentNullException(nameof(newQuestion));

            var question = new Question {Data = newQuestion, Answer = answer, Parent = parent};
            var animal = new Animal {Name = newAnimal, Parent = question, Answer = newAnswer};
            existingAnimal.Parent = question;
            existingAnimal.Answer = !newAnswer;
            SaveNewQuestion(question, animal, existingAnimal);
            return new Tuple<Question, Animal>(question, animal);
        }
    }
}