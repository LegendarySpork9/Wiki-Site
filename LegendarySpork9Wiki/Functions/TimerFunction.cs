namespace LegendarySpork9Wiki.Functions
{
    public class TimerFunction : IDisposable
    {
        private Timer? _timer;
        private readonly Action _callback;
        private readonly int _intervalMs;

        public TimerFunction(Action callback, int intervalMs)
        {
            _callback = callback;
            _intervalMs = intervalMs;
        }

        public void Start()
        {
            _timer = new Timer(_ => _callback(), null, 0, _intervalMs);
        }

        public void Stop()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
