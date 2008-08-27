using System;
using System.Collections.Generic;

using EasyAsync;

namespace EasyAsync.Samples
{
    class Abort
    {
        private List<Task> _tasks = new List<Task>();

        private IEnumerator<IAsyncCall> AbortAll()
        {
            // let others to run for some time
            yield return Task.Sleep(5000);

            Console.WriteLine("aborting tasks");
            foreach (Task t in _tasks)
            {
                t.Abort();
            }
        }

        private IEnumerator<IAsyncCall> RunForever(int id)
        {
            try
            {
                while (true)
                {
                    Console.WriteLine("task {0} running", id);
                    yield return Task.Sleep(100);
                }
            }
            finally
            {
                Console.WriteLine("task {0} aborted", id);
            }
        }

        public void Demo()
        {
            TaskManager.AddTask(AbortAll());

            for (int i = 0; i < 10; ++i)
            {
                Task task = TaskManager.AddTask(RunForever(i));
                _tasks.Add(task);
            }

            TaskManager.RunTasks();
        }
    }
}
