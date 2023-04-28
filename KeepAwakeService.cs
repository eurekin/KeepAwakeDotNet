namespace KeepAwakeServer
{
    public class KeepAwakeService
    {
        
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private Task? _task;
        private bool _isRunning;
        private TimeSpan _timeLeft;

        public bool IsRunning()
        {
            return _isRunning;
        }

        public TimeSpan TimeLeft()
        {
            return _timeLeft;
        }

        public void Start(TimeSpan duration)
        {
            if (!_isRunning)
            {
                _cts = new CancellationTokenSource();
                _task = Task.Run(() => KeepAwake(duration, _cts.Token));
                _isRunning = true;
            }
        }

        public void Stop()
        {
            if (_isRunning)
            {
                _cts.Cancel();
                if(_task != null) {
                    _task.Wait();
                }
                _isRunning = false;
            }
        }

        private void KeepAwake(TimeSpan duration, CancellationToken cancellationToken)
        {
            Console.WriteLine("Starting KeepAwake for {0}", duration);
            
            Program.EXECUTION_STATE state = Program.EXECUTION_STATE.ES_CONTINUOUS | Program.EXECUTION_STATE.ES_SYSTEM_REQUIRED;

            DateTime endTime = DateTime.Now + duration;

            while (!cancellationToken.IsCancellationRequested && DateTime.Now < endTime)
            {
                Console.WriteLine("KeepAwake - still running");
                Program.SetThreadExecutionState(state);

                _timeLeft = endTime - DateTime.Now;
                Thread.Sleep(1000);
            }

            Console.WriteLine("KeepAwake - finished");
            // Restore default execution state
            Program.SetThreadExecutionState(Program.EXECUTION_STATE.ES_CONTINUOUS);
            _timeLeft = endTime - DateTime.Now;
            _isRunning = false;
        }

        public void Extend(TimeSpan duration)
        {
            if (_cts != null)
            {
                // Cancel the existing cancellation token source
                _cts.Cancel();

                // Wait for the background task to finish
                if(_task != null) {
                    _task.Wait();
                }
            }

            // Create a new cancellation token source
            _cts = new CancellationTokenSource();

            // Start a new background task to keep the computer awake
            _task = Task.Run(() => KeepAwake(duration, _cts.Token));
        }
    }
}