using System;
using System.Collections.Generic;
using System.Threading;

using EasyAsync;

namespace EasyAsync.Samples
{
    class Sleep
    {
        private static IEnumerator<IAsyncCall> Sleepy(int id, int count, int millis)
        {
            for (int i = 0; i < count; ++i)
            {
                Console.WriteLine("task {0} sleeping on thread {1} @ {2:T} for {3} seconds",
                    id, Thread.CurrentThread.GetHashCode(), DateTime.Now, millis / 1000);

                yield return Task.Sleep(millis);

                Console.WriteLine("task {0} wokeup on thread {1} @ {2:T}",
                    id, Thread.CurrentThread.GetHashCode(), DateTime.Now);
            }
        }

        public void Demo()
        {
            Random random = new Random();
            TaskManager.AddTask(Sleepy(1, 4, 2000));
            TaskManager.AddTask(Sleepy(2, 5, 4000));
            TaskManager.AddTask(Sleepy(3, 6, 1000));

            TaskManager.RunTasks();
        }
    }
}
