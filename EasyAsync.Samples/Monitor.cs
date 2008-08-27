using System;
using System.Collections.Generic;

using EasyAsync;

namespace EasyAsync.Samples
{
    class MonitorSample
    {
        private IEnumerator<IAsyncCall> Signal(Monitor monitor)
        {
            for (int i = 0; i < 10; ++i)
            {
                yield return Task.Sleep(2000);
                Console.WriteLine("signal task pulsing monitor @ {0:T}", DateTime.Now);
                monitor.PulseAll();
            }

            Console.WriteLine("signal task exiting");
        }

        private IEnumerator<IAsyncCall> Wait(Monitor monitor, int id)
        {
            const int millis = 1000;

            for (int i = 0; i < 5; ++i)
            {
                Console.WriteLine("task {0} waiting on monitor for {1} seconds @ {2:T}", id, millis / 1000, DateTime.Now);
                yield return Task.CurrentTask.WaitOn(monitor, millis);
                Console.WriteLine("task {0} wait {1} @ {2:T}", id, Task.CurrentTask.WaitSucceded ? "succeeded" : "timed out", DateTime.Now);
            }

            Console.WriteLine("task {0} exiting", id);
        }

        public void Demo()
        {
            Monitor monitor = new Monitor();

            for (int i = 0; i < 1; ++i)
            {
                TaskManager.AddTask(Wait(monitor, i));
            }

            TaskManager.AddTask(Signal(monitor));
            TaskManager.RunTasks();
        }
    }
}
