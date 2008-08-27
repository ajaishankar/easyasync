using System;
using System.Collections.Generic;

namespace EasyAsync
{
    public class AsyncCall<R>
    {
        private BeginDelegate _begin;
        private EndDelegate<R> _end;
        private R _result;
        private Exception _exception;

        public R Result
        {
            get { return _result; }
        }

        public Exception Exception
        {
            get { return _exception; }
        }

        public bool Succeeded
        {
            get { return _exception == null; }
        }

        public Wait WaitOn(BeginDelegate begin)
        {
            _begin = begin;
            return new Wait(this);
        }

        public void Invoke()
        {
            Reset();
            Task task = Task.CurrentTask;

            try
            {
                task.OnBeginCall();
       
                AsyncCallbackWrapper cb = new AsyncCallbackWrapper(delegate(IAsyncResult ar) {
                    try
                    {
                        this._result = _end(ar);
                    }
                    catch (Exception x)
                    {
                        this._exception = x;
                    }

                    task.OnEndCall();
                });

                _begin(cb);

                cb.AssertWasUsed();

            }
            catch (Exception x)
            {
                if (x is CallbackNotPassedToBeginException) // user forgot to pass our callback to Begin method
                {
                    throw;
                }

                this._exception = x;
                task.OnEndCall();
            }
        }

        private IAsyncCall EndWait(EndDelegate<R> end)
        {
            _end = end;
            return new IAsyncCall(this.Invoke);
        }

        private void Reset()
        {
            _result = default(R);
            _exception = null;
        }

        public struct Wait
        {
            private AsyncCall<R> _call;

            internal Wait(AsyncCall<R> call)
            {
                _call = call;
            }

            public IAsyncCall And(EndDelegate<R> end)
            {
                return _call.EndWait(end);
            }

            public static IAsyncCall operator &(Wait wait, EndDelegate<R> end)
            {
                return wait.And(end);
            }
        }
    }

    public class AsyncCall
    {
        private AsyncCall<Void> _call = new AsyncCall<Void>();

        public Exception Exception
        {
            get { return _call.Exception; }
        }

        public bool Succeeded
        {
            get { return _call.Succeeded; }
        }

        public Wait WaitOn(BeginDelegate begin)
        {
            return new Wait(_call.WaitOn(begin));
        }

        public struct Wait
        {
            private AsyncCall<Void>.Wait _wait;

            internal Wait(AsyncCall<Void>.Wait wait)
            {
                _wait = wait;
            }

            public IAsyncCall And(EndDelegate end)
            {
                return _wait.And(ar => { end(ar); return Void.Value; });
            }

            public static IAsyncCall operator &(Wait wait, EndDelegate end)
            {
                return wait.And(end);
            }
        }

        public class Void
        {
            private Void() { }
            public static readonly Void Value = new Void();
        }
    }

    public sealed class IAsyncCall
    {
        private ProcDelegate _invoke;

        internal IAsyncCall(ProcDelegate invoke)
        {
            _invoke = invoke;
        }

        internal void Invoke() { _invoke(); }
    }
}
