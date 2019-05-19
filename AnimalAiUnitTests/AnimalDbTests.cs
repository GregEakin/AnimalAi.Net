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
            var swim = _repository.GetFirstQuestion();
            Assert.AreEqual("Does it swim?", swim.Data);

            var q2 = _repository.GetNextQuestion(swim, false);
            Assert.IsNull(q2);

            var bird = _repository.GetAnimal(swim, false);
            Assert.AreEqual("bird", bird.Name);

            var animalName = "elephant";
            var aa = _repository.GetAnimal(animalName);
            Assert.IsNull(aa);

            _repository.AddAnimal(swim, false, ref bird, "elephant", "Does it like peanuts?", true);

            CollectionAssert.AreEqual(new[] {"bird", "elephant", "fish"},
                _repository.FindAllAnimals().Select(a => a.Name).ToArray());
        }

        [TestMethod]
        public void DuplicateQuestionTest()
        {
            _repository.SetupDb();
            var swim = _repository.GetFirstQuestion();
            Assert.AreEqual("Does it swim?", swim.Data);

            var bird = _repository.GetAnimal(swim, false);
            Assert.AreEqual("bird", bird.Name);

            _repository.AddAnimal(swim, false, ref bird, "elephant", "Does it like peanuts?", true);
            var ex = AssertThrows<GenericADOException>(() =>
                _repository.AddAnimal(swim, false, ref bird, "dog", "Does it bark?", true));
            Assert.AreEqual(
                "could not insert: [AnimalAi.Data.Question][SQL: INSERT INTO questions (Data, ParentId, Answer) VALUES (?, ?, ?); select last_insert_rowid()]",
                ex.Message);
        }

        [TestMethod]
        public void DuplicateAnimalTest()
        {
            _repository.SetupDb();
            var swim = _repository.GetFirstQuestion();
            Assert.AreEqual("Does it swim?", swim.Data);

            var bird = _repository.GetAnimal(swim, false);
            Assert.AreEqual("bird", bird.Name);

            var (newQuestion, _) = _repository.AddAnimal(swim, false, ref bird, "elephant", "Does it like peanuts?", true);
            var ex = AssertThrows<GenericADOException>(() =>
                _repository.AddAnimal(newQuestion, false, ref bird, "elephant", "Does it bark?", true));
            Assert.AreEqual(
                "could not insert: [AnimalAi.Data.Animal][SQL: INSERT INTO animals (Name, ParentId, Answer) VALUES (?, ?, ?); select last_insert_rowid()]",
                ex.Message);
        }

        [TestMethod]
        public void BirdTest()
        {
            _repository.SetupDb();
            var swim = _repository.GetFirstQuestion();
            Assert.AreEqual("Does it swim?", swim.Data);

            var bird = _repository.GetAnimal(swim, false);
            Assert.AreEqual("bird", bird.Name);

            var (newQuestion, _) = _repository.AddAnimal(swim, false, ref bird, "elephant", "Does it like peanuts?", true);
            _repository.AddAnimal(newQuestion, false, ref bird, "dog", "Does it bark?", true);

            CollectionAssert.AreEqual(new[] {"bird", "dog", "elephant", "fish"},
                _repository.FindAllAnimals().Select(a => a.Name).ToArray());
        }

        [TestMethod]
        public void GivenExampleTest()
        {
            _repository.SetupDb();
            var swim = _repository.GetFirstQuestion();
            Assert.AreEqual("Does it swim?", swim.Data);

            var bird = _repository.GetAnimal(swim, false);
            Assert.AreEqual("bird", bird.Name);

            var (peanuts, elephant) = _repository.AddAnimal(swim, false, ref bird, "elephant", "Does it like peanuts?", true);

            var fish = _repository.GetAnimal(swim, true);
            Assert.AreEqual("fish", fish.Name);

            var (scales, seal) = _repository.AddAnimal(swim, true, ref fish, "seal", "Does it have scales?", false);

            var (roar, lion) = _repository.AddAnimal(peanuts, false, ref bird, "lion", "Does it roar?", true);
            var (tentacles, octopus) = _repository.AddAnimal(scales, false, ref seal, "octopus", "Does it have eight tentacles?", true);
            var (yob, wumpus) = _repository.AddAnimal(roar, false, ref bird, "wumpus", "Is its last name Yob?", true);

            CollectionAssert.AreEqual(new[] {"bird", "elephant", "fish", "lion", "octopus", "seal", "wumpus"},
                _repository.FindAllAnimals().Select(a => a.Name).ToArray());
        }
    }
}