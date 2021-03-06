﻿/*
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
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AnimalAi
{
    public sealed class AnimalRepository : IDisposable
    {
        private readonly ISession _session;

        public AnimalRepository(string connection, bool execute)
        {
            var cfg = new Configuration();
            cfg.DataBaseIntegration(p =>
            {
                p.ConnectionString = connection;
                p.Driver<SqlClientDriver>();
                p.Dialect<MsSql2012Dialect>();
                p.BatchSize = 1000;
                p.LogFormattedSql = false;
                p.LogSqlInConsole = false;
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
                return _session.QueryOver<Question>().Where(q => q.Parent == null && q.Answer == null).SingleOrDefault();
            }
        }

        public Question GetQuestion(Question parent, bool answer)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            using (_session.BeginTransaction())
            {
                return _session.QueryOver<Question>().Where(q => q.Parent == parent && q.Answer == answer).SingleOrDefault();
            }
        }

        public Animal GetAnimal(Question parent, bool answer)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            using (_session.BeginTransaction())
            {
                return _session.QueryOver<Animal>().Where(a => a.Parent == parent && a.Answer == answer).SingleOrDefault();
            }
        }

        public Animal GetAnimal(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            using (_session.BeginTransaction())
            {
                return _session.QueryOver<Animal>().Where(a => a.Name == name).SingleOrDefault();
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
                return _session.QueryOver<Animal>().OrderBy(a => a.Name).Asc.List();
            }
        }

        public (Question question, Animal animal) AddAnimal(Question parent, bool answer, ref Animal existingAnimal,
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
            return (question, animal);
        }
    }
}