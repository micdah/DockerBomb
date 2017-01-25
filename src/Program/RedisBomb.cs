using System;
using System.Threading;
using StackExchange.Redis;

namespace Program
{
    public class RedisBomb : IDisposable
    {
        private const int MaxRetryCount = 5;
        private readonly string _counterKey;
        private readonly string _name = $"Bomb{Guid.NewGuid()}";
        private readonly ManualResetEvent _stopEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent _stoppedEvent = new ManualResetEvent(false);
        private readonly Thread _thread;
        private readonly string _valueKey;
        private int _currentRetryCount = 0;

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
            _currentRetryCount = 0;

            try {
                while (!_stopEvent.WaitOne(0) && _currentRetryCount++<MaxRetryCount) {
                    if (!TryConnectAndRun()) {
                        State = State.Connecting;
                        _stopEvent.WaitOne(1000);
                    }
                }
            } catch (Exception e) {
                Console.WriteLine($"Unexpected error: {e.Message}");
            } finally {
                State = State.Dead;
                _stoppedEvent.Set();
            }
        }

        private bool TryConnectAndRun() {
            try {
                State = State.Connecting;

                using (var redis = ConnectionMultiplexer.Connect("127.0.0.1:6379"))
                {
                    var db = redis.GetDatabase();

                    // Set running
                    _currentRetryCount = 0;
                    State = State.Running;

                    while (!_stopEvent.WaitOne(0))
                    {
                        db.StringIncrement(_counterKey);
                        db.StringSet(_valueKey, Guid.NewGuid().ToString());
                    }

                    return true;
                }
            } catch (Exception) {
                return false;
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