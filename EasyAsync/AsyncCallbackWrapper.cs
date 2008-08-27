using System;
using System.Collections.Generic;

namespace EasyAsync
{
    /// <summary>
    /// The library passes a callback to the user code, and expects it to be passed when calling a Begin method
    /// We try to enforce this behaviour using the conversion operator trick in this class
    /// </summary>
    public class AsyncCallbackWrapper
    {
        private AsyncCallback _callback;
        private bool _used;

        internal AsyncCallbackWrapper(AsyncCallback cb)
        {
            _callback = cb;
        }

        public static implicit operator AsyncCallback(AsyncCallbackWrapper cb)
        {
            cb._used = true;
            return cb._callback;
        }

        internal void AssertWasUsed()
        {
            if (_used == false)
            {
                throw new CallbackNotPassedToBeginException();
            }
        }
    }

    public class CallbackNotPassedToBeginException : Exception
    {
        public CallbackNotPassedToBeginException()
            : base("Please check if you passed the callback when calling Begin method")
        {
        }
    }
}
