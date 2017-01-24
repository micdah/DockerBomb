using System;
using System.Threading;
using StackExchange.Redis;

namespace Program
{
    public class RedisBomb : IDisposable
    {
        private readonly string _counterKey;
        private readonly string _name = $"Bomb{Guid.NewGuid()}";
        private readonly ManualResetEvent _stopEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent _stoppedEvent = new ManualResetEvent(false);
        private readonly Thread _thread;
        private readonly string _valueKey;
        private IDatabase _db;
        private readonly ConnectionMultiplexer _redis;

        public RedisBomb()
        {
            _counterKey = $"{_name}_counter";
            _valueKey = $"{_name}_value";

            _redis = ConnectionMultiplexer.Connect("localhost:6379");
            _db = _redis.GetDatabase();

            _thread = new Thread(ThreadStart)
            {
                IsBackground = true,
                Name = _name
            };
        }

        public void Dispose()
        {
            // Stop thread
            _stopEvent.Set();
            _stoppedEvent.WaitOne();

            // Get final values
            if (_db != null)
                try
                {
                    string counter = _db.StringGet(_counterKey);
                    string value = _db.StringGet(_valueKey);

                    Console.WriteLine($"{_name}: {counter} iterations, final value {value}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Was unable to get final result for {_name}: {e.Message}");
                }

            _redis?.Dispose();
        }

        public void StartDropping()
        {
            if (!_thread.IsAlive)
                _thread.Start();
        }

        private void ThreadStart()
        {
            while (!_stopEvent.WaitOne(0))
                try
                {
                    _db.StringIncrement(_counterKey);
                    _db.StringSet(_valueKey, Guid.NewGuid().ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An error occurred dropping bombs for {_name}: {e.Message}");
                    _db = null;
                    break;
                }

            _stoppedEvent.Set();
        }
    }
}