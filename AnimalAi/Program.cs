using System;
using AnimalAi.Data;

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

            using (var animalRepository = new AnimalRepository(Connection, false))
            {
                // animalRepository.SetupDb();

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

                    var guess = animalRepository.GetAnimal(parent, answer);
                    if (AskTrueFalseQuestion($"Is it a {guess.Name}?"))
                    {
                        Console.WriteLine("Why not try another animal?");
                        continue;
                    }

                    Console.WriteLine("What animal were you thinking of?");
                    var v1 = Console.ReadLine();

                    Console.WriteLine("Please type a question that would distinguish a {0} from a {1}.", v1, guess.Name);
                    var q1 = Console.ReadLine();

                    var a1 = AskTrueFalseQuestion($"For a {v1}, the answer would be?");

                    var q2 = new Question {Data = q1, Answer = answer, Parent = parent};
                    var a2 = new Animal {Name = v1, Parent = q2, Answer = a1};
                    guess.Parent = q2;
                    guess.Answer = !a1;
                    animalRepository.SaveNewQuestion(q2, a2, guess);
                } while (true);

                Console.WriteLine();
                Console.WriteLine("Here's all the animals the computer knows:");
                var animals = animalRepository.DumpAllAnimals();
                foreach (var animal in animals)
                    Console.WriteLine(animal.Name);

            }
        }
    }
}