using System;
using System.Threading;

namespace Program
{
    public class Program
    {
        private const int StatusWidth = 50;
        private const int _statusInterval = 250;
        private readonly RedisBomb[] _bombs;
        private bool _tickTock;

        private Program()
        {
            var number = ReadNumber();

            Console.WriteLine();
            var top = Console.CursorTop;

            _bombs = new RedisBomb[number];
            for (var i = 0; i < number; i++)
            {
                _bombs[i] = new RedisBomb();
            }

            // Slowly start dropping bombs
            var waitPerBomb = 50;
            var updateStatusInterval = _statusInterval / waitPerBomb;

            for (var i = 0; i < number; i++)
            {
                _bombs[i].StartDropping();

                Thread.Sleep(waitPerBomb);

                if ((i + 1) % updateStatusInterval == 0)
                    ShowStatus(top);
            }

            while (true)
            {
                Thread.Sleep(_statusInterval);
                ShowStatus(top);
            }
        }

        private void ShowStatus(int top)
        {
            Console.SetCursorPosition(0, top);

            var roller = (_tickTock = !_tickTock) ? '/' : '\\';
            Console.WriteLine($"Status {roller} (? : connecting, . : running, x : dead)");
            for (var i = 0; i < _bombs.Length; i++)
            {
                var c = ' ';
                switch (_bombs[i].State)
                {
                    case State.Connecting:
                        c = '?';
                        break;
                    case State.Running:
                        c = '.';
                        break;
                    case State.Dead:
                        c = 'x';
                        break;
                }
                Console.Write(c);

                if ((i + 1) % StatusWidth == 0)
                    Console.WriteLine();
            }
        }

        public static void Main(string[] args)
        {
            new Program();
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