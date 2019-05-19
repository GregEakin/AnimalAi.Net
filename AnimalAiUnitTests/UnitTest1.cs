using System;
using AnimalAi;
using AnimalAi.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate;
using NHibernate.Tool.hbm2ddl;

namespace AnimalAiUnitTests
{
    [TestClass]
    public class UnitTest1 : IDisposable
    {
        // 855 5owning
        // 3.75%
        // owing.com

        private readonly ISession _session;
        private readonly AnimalRepository _repository;

        public UnitTest1()
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
        public void TestMethod1()
        {
            _repository.SetupDb();
        }

        [TestMethod]
        public void TestMethod2()
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

            var newQuestion = new Question { Data = questionData, Answer = false, Parent = q1 };
            var newAnimal = new Animal { Name = animalName, Parent = newQuestion, Answer = questionAnswer };
            a1.Parent = newQuestion;
            a1.Answer = !questionAnswer;
            _repository.SaveNewQuestion(newQuestion, newAnimal, a1);
        }
    }
}