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

            using (var animalRepository = new AnimalRepository(Connection, true))
            {
                var setup = animalRepository.GetFirstQuestion();
                if (setup == null)
                    animalRepository.SetupDb();

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
                    var animalName = Console.ReadLine();

                    var aa = animalRepository.GetAnimal(animalName);
                    if (aa != null)
                        throw new Exception($"We already have a {animalName} in the database.");

                    Console.WriteLine("Please type a question that would distinguish a {0} from a {1}.", animalName, animal.Name);
                    var questionData = Console.ReadLine();

                    var questionAnswer = AskTrueFalseQuestion($"For a {animalName}, the answer would be?");

                    var newQuestion = new Question {Data = questionData, Answer = answer, Parent = parent};
                    var newAnimal = new Animal {Name = animalName, Parent = newQuestion, Answer = questionAnswer};
                    animal.Parent = newQuestion;
                    animal.Answer = !questionAnswer;
                    animalRepository.SaveNewQuestion(newQuestion, newAnimal, animal);
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