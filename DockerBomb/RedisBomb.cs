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

        public RedisBomb()
        {
            _counterKey = $"{_name}_counter";
            _valueKey = $"{_name}_value";

            _thread = new Thread(ThreadStart)
            {
                IsBackground = true,
                Name = _name
            };

            State = State.Ready;
        }

        public State State { get; private set; }

        public void Dispose()
        {
            // Stop thread
            _stopEvent.Set();
            _stoppedEvent.WaitOne();
        }

        public void StartDropping()
        {
            if (!_thread.IsAlive)
                _thread.Start();
        }

        private void ThreadStart()
        {
            try
            {
                State = State.Connecting;

                using (var redis = ConnectionMultiplexer.Connect("localhost:6379"))
                {
                    var db = redis.GetDatabase();

                    State = State.Running;

                    while (!_stopEvent.WaitOne(0))
                    {
                        db.StringIncrement(_counterKey);
                        db.StringSet(_valueKey, Guid.NewGuid().ToString());
                    }
                }
            }
            catch (Exception)
            {
                State = State.Dead;
            }
            finally
            {
                _stoppedEvent.Set();
            }
        }
    }

    public enum State
    {
        Ready,
        Connecting,
        Running,
        Dead
    }
}