using System;
using System.Linq;
using AnimalAi;
using AnimalAi.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate;
using NHibernate.Exceptions;
using NHibernate.Tool.hbm2ddl;

namespace AnimalAiUnitTests
{
    [TestClass]
    public class AnimalDbTests : IDisposable
    {
        // 855 5owning
        // 3.75%
        // owing.com

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

            var questionData = "Does it like peanuts?";
            var questionAnswer = true;

            var newQuestion = new Question {Data = questionData, Answer = false, Parent = q1};
            var newAnimal = new Animal {Name = animalName, Parent = newQuestion, Answer = questionAnswer};
            a1.Parent = newQuestion;
            a1.Answer = !questionAnswer;
            _repository.SaveNewQuestion(newQuestion, newAnimal, a1);

            CollectionAssert.AreEqual(new[] {"bird", "elephant", "fish"},
                _repository.FindAllAnimals().Select(a => a.Name).ToArray());
        }

        [TestMethod]
        [ExpectedException(typeof(GenericADOException))]
        public void DuplicateQuestionTest()
        {
            _repository.SetupDb();
            var q1 = _repository.GetFirstQuestion();
            Assert.AreEqual("Does it swim?", q1.Data);

            var a1 = _repository.GetAnimal(q1, false);
            Assert.AreEqual("bird", a1.Name);

            var newQuestion = new Question {Data = "Does it like peanuts?", Answer = false, Parent = q1};
            var newAnimal = new Animal {Name = "elephant", Parent = newQuestion, Answer = true};
            a1.Parent = newQuestion;
            a1.Answer = false;
            _repository.SaveNewQuestion(newQuestion, newAnimal, a1);

            var duplicateQuestion = new Question {Data = "Does it bark?", Answer = false, Parent = q1};
            var duplicateAnimal = new Animal {Name = "dog", Parent = duplicateQuestion, Answer = true};
            a1.Parent = duplicateQuestion;
            a1.Answer = false;
            _repository.SaveNewQuestion(duplicateQuestion, duplicateAnimal, a1);
        }

        [TestMethod]
        [ExpectedException(typeof(GenericADOException))]
        public void DuplicateAnimalTest()
        {
            _repository.SetupDb();
            var q1 = _repository.GetFirstQuestion();
            Assert.AreEqual("Does it swim?", q1.Data);

            var a1 = _repository.GetAnimal(q1, false);
            Assert.AreEqual("bird", a1.Name);

            var newQuestion = new Question {Data = "Does it like peanuts?", Answer = false, Parent = q1};
            var newAnimal = new Animal {Name = "elephant", Parent = newQuestion, Answer = true};
            a1.Parent = newQuestion;
            a1.Answer = false;
            _repository.SaveNewQuestion(newQuestion, newAnimal, a1);

            var duplicateQuestion = new Question {Data = "Does it bark?", Answer = false, Parent = newQuestion};
            var duplicateAnimal = new Animal {Name = "elephant", Parent = duplicateQuestion, Answer = true};
            a1.Parent = duplicateQuestion;
            a1.Answer = false;
            _repository.SaveNewQuestion(duplicateQuestion, duplicateAnimal, a1);
        }

        [TestMethod]
        public void BirdTest()
        {
            _repository.SetupDb();
            var q1 = _repository.GetFirstQuestion();
            Assert.AreEqual("Does it swim?", q1.Data);

            var a1 = _repository.GetAnimal(q1, false);
            Assert.AreEqual("bird", a1.Name);

            var newQuestion = new Question { Data = "Does it like peanuts?", Answer = false, Parent = q1 };
            var newAnimal = new Animal { Name = "elephant", Parent = newQuestion, Answer = true };
            a1.Parent = newQuestion;
            a1.Answer = false;
            _repository.SaveNewQuestion(newQuestion, newAnimal, a1);

            var duplicateQuestion = new Question { Data = "Does it bark?", Answer = false, Parent = newQuestion };
            var duplicateAnimal = new Animal { Name = "dog", Parent = duplicateQuestion, Answer = true };
            a1.Parent = duplicateQuestion;
            a1.Answer = false;
            _repository.SaveNewQuestion(duplicateQuestion, duplicateAnimal, a1);

            CollectionAssert.AreEqual(new[] { "bird", "dog", "elephant", "fish" },
                _repository.FindAllAnimals().Select(a => a.Name).ToArray());
        }
    }
}