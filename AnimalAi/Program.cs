using AnimalAi.Data;
using System;

namespace AnimalAi
{
    class Program
    {
        // https://www.atariarchives.org/basicgames/showpage.php?page=4

        public const string Connection =
            "Data Source=(localdb)\\ProjectsV13;" +
            "Initial Catalog=AnimalDb;" +
            "Integrated Security=True;" +
            "Connect Timeout=30;" +
            "Encrypt=False;" +
            "TrustServerCertificate=False;" +
            "ApplicationIntent=ReadWrite;" +
            "MultiSubnetFailover=False";

        static bool AskTrueFalseQuestion(string question)
        {
            Console.WriteLine(question);
            var answer = Console.ReadLine();
            return !string.IsNullOrWhiteSpace(answer) && answer.Trim().ToLower().StartsWith("y");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Animal");
            Console.WriteLine("Play guess the animal.");
            Console.WriteLine("Think of an animal, and the computer will try to guess it.");
            Console.WriteLine();

            var setup = args.Length > 0 && args[0] == "-setup";
            var demo = args.Length > 1 && args[1] == "-demo";
            using (var animalRepository = new AnimalRepository(Connection, setup))
            {
                if (setup)
                    animalRepository.SetupDb();

                if (demo)
                {
                    var swim = animalRepository.GetFirstQuestion();
                    var bird = animalRepository.GetAnimal(swim, false);
                    var (peanuts, elephant) =
                        animalRepository.AddAnimal(swim, false, ref bird, "elephant", "Does it like peanuts?", true);
                    var fish = animalRepository.GetAnimal(swim, true);
                    var (scales, seal) =
                        animalRepository.AddAnimal(swim, true, ref fish, "seal", "Does it have scales?", false);
                    var (roar, lion) =
                        animalRepository.AddAnimal(peanuts, false, ref bird, "lion", "Does it roar?", true);
                    var (tentacles, octopus) = animalRepository.AddAnimal(scales, false, ref seal, "octopus",
                        "Does it have eight tentacles?", true);
                    var (yob, wumpus) =
                        animalRepository.AddAnimal(roar, false, ref bird, "wumpus", "Is its last name Yob?", true);
                }

                do
                {
                    if (!AskTrueFalseQuestion("Are you thinking of an animal?"))
                        break;

                    Question parent = null;
                    var question = animalRepository.GetFirstQuestion();
                    var answer = false;

                    while (question != null)
                    {
                        answer = AskTrueFalseQuestion(question.Data);
                        parent = question;
                        question = animalRepository.GetNextQuestion(question, answer);
                    }

                    if (parent == null)
                        throw new Exception("The question DB is empty.");

                    var animal = animalRepository.GetAnimal(parent, answer);
                    if (animal == null)
                        throw new Exception($"Missing leaf node in DB for {parent.Data} and {answer}.");

                    if (AskTrueFalseQuestion($"Is it a {animal.Name}?"))
                    {
                        Console.WriteLine("Why not try another animal?");
                        continue;
                    }

                    Console.WriteLine("What animal were you thinking of?");
                    var newAnimal = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(newAnimal))
                        throw new Exception("Can't have a blank animal.");

                    Console.WriteLine("Please type a question that would distinguish a {0} from a {1}.", newAnimal,
                        animal.Name);
                    var newQuestion = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(newQuestion))
                        throw new Exception("Can't have a blank question.");

                    var newAnswer = AskTrueFalseQuestion($"For a {newAnimal}, the answer would be?");

                    animalRepository.AddAnimal(parent, answer, ref animal, newAnimal, newQuestion, newAnswer);
                } while (true);

                Console.WriteLine();
                Console.WriteLine("Here's all the animals the computer knows:");
                var animals = animalRepository.FindAllAnimals();
                foreach (var animal in animals)
                    Console.WriteLine(animal.Name);
            }
        }
    }
}