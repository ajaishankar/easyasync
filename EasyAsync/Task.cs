using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading;

namespace EasyAsync
{
    public enum TaskState {
        Unstarted, Ready, Running, Wait, Terminated 
    };

    public class Task : IDisposable
    {
        [ThreadStatic]
        private static Task _currentTask;
        private TaskState _taskState;
        private IEnumerator<IAsyncCall> _asyncCallIterator;
        private TaskManager _taskManager;
        private object _taskContext;
        private bool _abortTask;
        private AsyncCall<bool> _waitCall;
        private AsyncTimer _timer;

        internal Task(TaskManager taskManager, IEnumerator<IAsyncCall> callIterator)
        {
            this._waitCall = new AsyncCall<bool>();
            this._taskManager = taskManager;
            this._taskState = TaskState.Unstarted;
            this._asyncCallIterator = callIterator;
        }

        ~Task()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal AsyncOperation AsyncOperation
        {
            get;
            set;  // set by TaskManager.OnBeginCall for Windows forms model
        }

        protected virtual void Dispose(bool disposing)
        {
            TaskState = TaskState.Terminated;

            if (_asyncCallIterator != null)
            {
                _asyncCallIterator.Dispose();
                _asyncCallIterator = null;
            }

            if (_timer != null)
            {
                ((IDisposable)_timer).Dispose();
                _timer = null;
            }
        }

        public static Task CurrentTask
        {
            get { return _currentTask; }
            internal set { _currentTask = value; }
        }

        public TaskManager TaskManager
        {
            get { return _taskManager; }
        }

        public object TaskContext
        {
            get { return _taskContext; }
            set { _taskContext = value; }
        }

        public TaskState TaskState
        {
            get { return _taskState; }
            internal set { _taskState = value; }
        }

        public void Abort() { _abortTask = true; }

        internal IAsyncCall Start()
        {
            if (TaskState != TaskState.Unstarted)
                throw new InvalidOperationException();

            TaskState = TaskState.Ready;

            return Resume();
        }

        internal IAsyncCall Resume()
        {
            if (TaskState != TaskState.Ready)
                throw new InvalidOperationException();

            Task.CurrentTask = this;

            TaskState = TaskState.Running;

            if (!_abortTask && _asyncCallIterator.MoveNext())
            {
                return _asyncCallIterator.Current;
            }
            else
            {
                TaskState = TaskState.Terminated;
                return null;
            }
        }

        internal void OnBeginCall()
        {
            _taskManager.OnBeginCall(this);
        }

        internal void OnEndCall()
        {
            _taskManager.OnEndCall(this);
        }

        public static IAsyncCall Sleep(int millis)
        {
            return Task.CurrentTask.GotoSleep(millis);
        }

        private IAsyncCall GotoSleep(int millis)
        {
            if (millis <= 0)
                throw new ArgumentException("Argument must be greater than zero");

            if (_timer == null)
                _timer = new AsyncTimer();

            return _waitCall
                .WaitOn(cb => _timer.Start(millis, cb, null)) & _timer.End;
        }

        public bool WaitSucceded
        {
            get { return _waitCall.Result; }
        }

        public IAsyncCall WaitOn(Monitor monitor)
        {
            return WaitOn(monitor, Timeout.Infinite);
        }

        public IAsyncCall WaitOn(Monitor monitor, int millis)
        {
            if (millis <= 0)
                millis = Timeout.Infinite;

            return _waitCall
                .WaitOn(cb => monitor.BeginWait(millis, cb, null)) & monitor.EndWait;
        }
    }
}
