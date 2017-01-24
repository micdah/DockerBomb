using System;
using System.Threading;
using StackExchange.Redis;

namespace Program
{
    public class RedisBomb : IDisposable
    {
        private readonly string _name = $"Bomb{Guid.NewGuid()}";
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly ManualResetEvent _stopEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent _stoppedEvent = new ManualResetEvent(false);
        private readonly Thread _thread;
        private readonly string _counterKey;
        private readonly string _valueKey;

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

        public void Start()
        {
            if (!_thread.IsAlive)
                _thread.Start();
        }

        public void Dispose()
        {
            // Stop thread
            _stopEvent.Set();
            _stoppedEvent.WaitOne();

            // Get final values
            string counter = _db.StringGet(_counterKey);
            string value = _db.StringGet(_valueKey);

            Console.WriteLine($"{_name}: {counter} iterations, final value {value}");

            _redis?.Dispose();
        }

        private void ThreadStart()
        {
            while (!_stopEvent.WaitOne(0))
            {
                _db.StringIncrement(_counterKey);
                _db.StringSet(_valueKey, Guid.NewGuid().ToString());
            }

            _stoppedEvent.Set();
        }
    }
}