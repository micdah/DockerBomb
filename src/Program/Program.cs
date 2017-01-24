using System;

namespace Program
{
    public class Program
    {
        private Program()
        {
            var number = ReadNumber();

            var bombs = new RedisBomb[number];
            for (var i = 0; i < number; i++)
                bombs[i] = new RedisBomb();

            WaitForEnter("drop bombs");
            foreach (var bomb in bombs)
                bomb.Start();

            WaitForEnter("stop");
            foreach (var bomb in bombs)
                bomb.Dispose();

            WaitForEnter("close");
        }

        public static void Main(string[] args)
        {
            new Program();
        }

        private static void WaitForEnter(string message)
        {
            Console.Write($"Press enter to {message}...");
            Console.ReadLine();
        }

        private static int ReadNumber()
        {
            int number;
            while (true)
            {
                Console.Write("Enter number of bombs to fork: ");
                var response = Console.ReadLine();
                if (!int.TryParse(response, out number))
                {
                    Console.WriteLine("Sorry, I couldn't understand that, please try again.");
                    continue;
                }

                if (number <= 0)
                {
                    Console.WriteLine("Sorry, only positive integers are allowed, please try again.");
                    continue;
                }

                // All good
                break;
            }
            return number;
        }
    }
}