using System;
using System.Linq;
using System.Text;
using AnimalAi;
using AnimalAi.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate;
using NHibernate.Tool.hbm2ddl;

namespace AnimalAiUnitTests
{
    [TestClass]
    public class TreeTests : IDisposable
    {
        private readonly ISession _session;
        private readonly AnimalRepository _repository;

        public TreeTests()
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


        private string DumpHeap(Question question)
        {
            if (question == null)
                return "\u2205";

            var result = new StringBuilder();
            result.Append("[Q: ");
            result.Append(question.Data);

            result.Append(" true: ");
            var t = _repository.GetQuestion(question, true);
            result.Append(t != null ? DumpHeap(t) : _repository.GetAnimal(question, true).Name);

            result.Append(", false: ");
            var f = _repository.GetQuestion(question, false);
            result.Append(f != null ? DumpHeap(f) : _repository.GetAnimal(question, false).Name);

            result.Append("]");
            return result.ToString();
        }

        [TestMethod]
        public void GivenExampleTest()
        {
            _repository.SetupDb();
            var swim = _repository.GetQuestion();
            var bird = _repository.GetAnimal(swim, false);
            var (peanuts, elephant) =
                _repository.AddAnimal(swim, false, ref bird, "elephant", "Does it like peanuts?", true);
            var fish = _repository.GetAnimal(swim, true);
            var (scales, seal) = _repository.AddAnimal(swim, true, ref fish, "seal", "Does it have scales?", false);
            var (roar, lion) = _repository.AddAnimal(peanuts, false, ref bird, "lion", "Does it roar?", true);
            var (tentacles, octopus) = _repository.AddAnimal(scales, false, ref seal, "octopus",
                "Does it have eight tentacles?", true);
            var (yob, wumpus) = _repository.AddAnimal(roar, false, ref bird, "wumpus", "Is its last name Yob?", true);

            Assert.AreEqual(
                "[Q: Does it swim? true: [Q: Does it have scales? true: fish, false: [Q: Does it have eight tentacles? true: octopus, false: seal]], false: [Q: Does it like peanuts? true: elephant, false: [Q: Does it roar? true: lion, false: [Q: Is its last name Yob? true: wumpus, false: bird]]]]",
                DumpHeap(swim));
        }
    }
}