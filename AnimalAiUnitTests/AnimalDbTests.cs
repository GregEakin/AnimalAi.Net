using AnimalAi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate;
using NHibernate.Exceptions;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Linq;
using static AnimalAiUnitTests.utilities.ExpectedException;

namespace AnimalAiUnitTests
{
    [TestClass]
    public class AnimalDbTests : IDisposable
    {
        private readonly ISession _session;
        private readonly AnimalRepository _repository;

        public AnimalDbTests()
        {
            var configuration = InMemoryDatabase.SetupConfiguration();
            var sessionFactory = configuration.BuildSessionFactory();
            _session = sessionFactory.OpenSession();
            new SchemaExport(configuration).Execute(false, true, false, _session.Connection, Console.Out);
            _repository = new AnimalRepository(_session);
        }

        public void Dispose()
        {
            _repository?.Dispose();
            _session?.Dispose();
        }

        [TestMethod]
        public void DatabaseSetupTest()
        {
            _repository.SetupDb();
            CollectionAssert.AreEqual(new[] {"bird", "fish"},
                _repository.FindAllAnimals().Select(a => a.Name).ToArray());
        }

        [TestMethod]
        public void NewAnimalTest()
        {
            _repository.SetupDb();
            var q1 = _repository.GetFirstQuestion();
            Assert.AreEqual("Does it swim?", q1.Data);

            var q2 = _repository.GetNextQuestion(q1, false);
            Assert.IsNull(q2);

            var a1 = _repository.GetAnimal(q1, false);
            Assert.AreEqual("bird", a1.Name);

            var animalName = "elephant";
            var aa = _repository.GetAnimal(animalName);
            Assert.IsNull(aa);

            _repository.AddAnimal("Does it like peanuts?", false, q1, "elephant", true, a1);

            CollectionAssert.AreEqual(new[] {"bird", "elephant", "fish"},
                _repository.FindAllAnimals().Select(a => a.Name).ToArray());
        }

        [TestMethod]
        public void DuplicateQuestionTest()
        {
            _repository.SetupDb();
            var q1 = _repository.GetFirstQuestion();
            Assert.AreEqual("Does it swim?", q1.Data);

            var a1 = _repository.GetAnimal(q1, false);
            Assert.AreEqual("bird", a1.Name);

            _repository.AddAnimal("Does it like peanuts?", false, q1, "elephant", true, a1);
            var ex = AssertThrows<GenericADOException>(() =>
                _repository.AddAnimal("Does it bark?", false, q1, "dog", true, a1));
            Assert.AreEqual(
                "could not insert: [AnimalAi.Data.Question][SQL: INSERT INTO questions (Data, ParentId, Answer) VALUES (?, ?, ?); select last_insert_rowid()]",
                ex.Message);
        }

        [TestMethod]
        public void DuplicateAnimalTest()
        {
            _repository.SetupDb();
            var q1 = _repository.GetFirstQuestion();
            Assert.AreEqual("Does it swim?", q1.Data);

            var a1 = _repository.GetAnimal(q1, false);
            Assert.AreEqual("bird", a1.Name);

            var newQuestion = _repository.AddAnimal("Does it like peanuts?", false, q1, "elephant", true, a1);
            var ex = AssertThrows<GenericADOException>(() =>
                _repository.AddAnimal("Does it bark?", false, newQuestion, "elephant", true, a1));
            Assert.AreEqual(
                "could not insert: [AnimalAi.Data.Animal][SQL: INSERT INTO animals (Name, ParentId, Answer) VALUES (?, ?, ?); select last_insert_rowid()]",
                ex.Message);
        }

        [TestMethod]
        public void BirdTest()
        {
            _repository.SetupDb();
            var q1 = _repository.GetFirstQuestion();
            Assert.AreEqual("Does it swim?", q1.Data);

            var a1 = _repository.GetAnimal(q1, false);
            Assert.AreEqual("bird", a1.Name);

            var newQuestion = _repository.AddAnimal("Does it like peanuts?", false, q1, "elephant", true, a1);
            _repository.AddAnimal("Does it bark?", false, newQuestion, "dog", true, a1);

            CollectionAssert.AreEqual(new[] {"bird", "dog", "elephant", "fish"},
                _repository.FindAllAnimals().Select(a => a.Name).ToArray());
        }
    }
}