using System;
using System.Collections.Generic;

namespace EasyAsync
{
    public delegate void ProcDelegate();
    public delegate IAsyncResult BeginDelegate(AsyncCallbackWrapper cb);
    public delegate void EndDelegate(IAsyncResult ar);
    public delegate R EndDelegate<R>(IAsyncResult ar);
    public delegate bool TimerTaskCallback();
}