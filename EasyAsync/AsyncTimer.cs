using System;
using System.Collections.Generic;
using System.Threading;

namespace EasyAsync
{
    sealed class AsyncTimer : IDisposable        
    {
        private Timer _timer;
        private AsyncCallback _callback;

        internal AsyncTimer()
        {
            _timer = new Timer(TimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        internal IAsyncResult Start(int millis, AsyncCallback callback, object state)
        {
            _callback = callback;
            _timer.Change(millis, Timeout.Infinite); // start the timer
            return null;
        }

        internal bool End(IAsyncResult ar) { return true; }

        private void TimerCallback(object state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            if (_callback != null)
            {
                _callback(null);
            }
        }

        ~AsyncTimer()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
    }
}
