using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Text;
using System.Windows.Forms;

using EasyAsync;

namespace EasyAsync.Samples
{
    public partial class Gui : Form
    {
        delegate int Func();

        public Gui()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Task dbTask = TaskManager.AddTask(DatabaseTask());
            TaskManager.AddTask(UpdateProgressTask(dbTask));
            TaskManager.RunTasks();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TaskManager.AddTask(Task2());
            TaskManager.RunTasks();
        }

        IEnumerator<IAsyncCall> Task2()
        {
            textBox1.Text = string.Empty;

            for (int i = 0; i < 10; ++i)
            {
                textBox1.Text += "task2\r\n";
                yield return Task.Sleep(100);
            }
        }

        IEnumerator<IAsyncCall> UpdateProgressTask(Task dbTask)
        {
            while (dbTask.TaskState != TaskState.Terminated)
            {
                if (progressBar1.Value == progressBar1.Maximum)
                    progressBar1.Value = 0;
                else
                    progressBar1.Value += 1;

                textBox1.Text += "this database call is taking way too long!\r\n";
                yield return Task.Sleep(200);
            }

            progressBar1.Value = 0;
        }

        IEnumerator<IAsyncCall> DatabaseTask()
        {
            button1.Enabled = false;

            AsyncCall<int> call = new AsyncCall<int>();

            Func slow = new Func(DarnSlowDatabaseCall);

            yield return call.WaitOn(cb => slow.BeginInvoke(cb, null)) & slow.EndInvoke;

            textBox1.Text = "db task returned " + call.Result + "\r\n";
            button1.Enabled = true;
        }

        int DarnSlowDatabaseCall()
        {
            Thread.Sleep(15000);
            return new Random().Next();
        }

        public void Demo()
        {
            Application.Run(new Gui());
        }
    }
}
