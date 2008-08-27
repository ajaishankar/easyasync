using System;
using System.Collections.Generic;
using System.Threading;

namespace EasyAsync
{
    /// <summary>
    /// A monitor that works on a _single_ thread, and waited and pulsed by different "tasks"!
    /// </summary>
    public sealed class Monitor
    {
        private TaskManager _taskManager = TaskManager.Current;
        private List<AsyncCallback> _waiters;

        public Monitor()
        {
            _waiters = new List<AsyncCallback>();
        }

        public void Pulse()
        {
            VerifyTaskManager();

            AsyncCallback callback = null;

            lock (_waiters)
            {
                if (_waiters.Count == 0)
                {
                    return;
                }

                callback = _waiters[0];
                _waiters.RemoveAt(0);               
            }

            callback(BoolResult.TrueValue);
        }

        public void PulseAll()
        {
            VerifyTaskManager();

            List<AsyncCallback> copy;

            lock (_waiters)
            {
                copy = new List<AsyncCallback>(_waiters);
                _waiters.Clear();
            }

            foreach (AsyncCallback callback in copy)
            {
                callback(BoolResult.TrueValue);
            }
        }

        internal IAsyncResult BeginWait(int millis, AsyncCallback callback, object state)
        {
            VerifyTaskManager();

            lock (_waiters)
            {
                _waiters.Add(callback);
            }

            if (millis == Timeout.Infinite) // task will wait forever till pulsed
                return null;
            
            if (millis <= 0)
                throw new ArgumentException("Argument must be greater than zero");

            AsyncTimer timer = new AsyncTimer();

            timer.Start(millis, delegate(IAsyncResult ar) {
                ((IDisposable)timer).Dispose();

                bool timedOut = false;
                lock(_waiters) {
                    if (_waiters.Contains(callback)) // timed out
                    {
                        timedOut = true;
                        _waiters.Remove(callback);
                    }
                }
                if (timedOut)
                    callback(BoolResult.FalseValue);
            }, null);

            return null;
        }

        internal bool EndWait(IAsyncResult ar)
        {
            return ar == BoolResult.TrueValue;
        }

        private void VerifyTaskManager()
        {
            if (_taskManager != Task.CurrentTask.TaskManager)
            {
                throw new Exception(
                    "Cannot wait or pulse across tasks running on different task managers");
            }            
        }

        private class BoolResult : IAsyncResult
        {
            internal static readonly BoolResult TrueValue = new BoolResult();
            internal static readonly BoolResult FalseValue = new BoolResult();

            private BoolResult() { }

            public object AsyncState 
            { 
                get { return null; } 
            }
            public WaitHandle AsyncWaitHandle
            {
                get { return null; } 
            }
            public bool CompletedSynchronously 
            {
                get { return false; } 
            }
            public bool IsCompleted 
            { 
                get { return false; } 
            }
        }
    }
}
