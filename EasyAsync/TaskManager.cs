using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;

using Timer = System.Threading.Timer;

namespace EasyAsync
{
    public class TaskManager
    {
        [ThreadStatic]
        private static TaskManager _currentTaskManager = new TaskManager();
        private List<Task> _taskList = new List<Task>();
        private AutoResetEvent _aCallCompleted = new AutoResetEvent(false);
        private bool _taskAdded;
        private int _executingCalls;
        private List<Task> _terminatedList = new List<Task>();

        private TaskManager() { }

        internal static TaskManager Current
        {
            get { return _currentTaskManager; }
        }

        internal bool IsRunningInMessageLoop
        {
            get
            {
                return SynchronizationContext.Current is WindowsFormsSynchronizationContext;
            }
        }

        public static void RunTasks()
        {
            Current.RunTasksBasedOnSynchronizationModel();
        }

        private void RunTasksBasedOnSynchronizationModel()
        {
            if (Task.CurrentTask != null && Task.CurrentTask.TaskState == TaskState.Running)
            {
                return; // already in scheduler loop
            }

            if (IsRunningInMessageLoop)
            {
                ScheduleTasks();
            }
            else // block till all calls complete
            {
                while (true)
                {
                    ScheduleTasks();

                    if (_taskList.Count == 0)
                        break;

                    if (Interlocked.CompareExchange(ref _executingCalls, 0, 0) > 0)
                    {
                        _aCallCompleted.WaitOne();
                    }
                }
            }
        }

        protected void ScheduleTasks()
        {
            do
            {
                _taskAdded = false;

                foreach (Task task in _taskList)
                {
                    if (task.TaskState == TaskState.Terminated)
                    {
                        _terminatedList.Add(task);
                    }
                    else
                    {
                        IAsyncCall call = null;

                        if (task.TaskState == TaskState.Unstarted)
                            call = task.Start();
                        else if (task.TaskState == TaskState.Ready)
                            call = task.Resume();
                        else
                            continue;

                        if (call != null)
                            call.Invoke();
                        else
                            _terminatedList.Add(task);

                        if (_taskAdded)
                            break;
                    }
                }

                foreach (Task task in _terminatedList)
                {
                    _taskList.Remove(task);
                    ((IDisposable) task).Dispose();
                }

                _terminatedList.Clear();

            } while (_taskAdded);
        }


        protected internal virtual void OnBeginCall(Task task)
        {
            task.TaskState = TaskState.Wait;
            Interlocked.Increment(ref _executingCalls);

            if (IsRunningInMessageLoop)
            {
                task.AsyncOperation = AsyncOperationManager.CreateOperation(task);
            }
        }


        // this is called from a worker thread
        protected internal virtual void OnEndCall(Task task)
        {
            task.TaskState = TaskState.Ready;
            Interlocked.Decrement(ref _executingCalls);

            AsyncOperation op = task.AsyncOperation;

            if (op != null) // running in message loop
            {
                task.AsyncOperation = null;
                op.PostOperationCompleted(state => ScheduleTasks(), null); // run ScheduleTasks on UI thread
            }
            else
            {
                _aCallCompleted.Set(); // unblock scheduler wait in RunTasks
            }
        }

        private void AddTask(Task task)
        {
            _taskList.Add(task);
            _taskAdded = true;
        }

        public static Task AddTask(IEnumerator<IAsyncCall> callIterator)
        {
            Task task = new Task(TaskManager.Current, callIterator);
            TaskManager.Current.AddTask(task);
            return task;
        }

        public static Task AddTimerTask(int period, TimerTaskCallback callback)
        {
            if (period <= 0)
                throw new ArgumentException("Argument must be greater than zero");

            return AddTask(TimerTask(period, callback));
        }

        private static IEnumerator<IAsyncCall> TimerTask(int period, TimerTaskCallback callback)
        {
            do
            {
                yield return Task.Sleep(period);
            } while (callback());
        }
    }
}
