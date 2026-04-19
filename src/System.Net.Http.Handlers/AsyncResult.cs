// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Web.Http;

namespace System.Net.Http.Internal;

internal abstract class AsyncResult(AsyncCallback? callback, object? state) : IAsyncResult
{
    private readonly AsyncCallback? _callback = callback;
    private readonly object? _state = state;

    private bool _isCompleted;
    private bool _completedSynchronously;
    private bool _endCalled;

    private Exception? _exception;

    public object? AsyncState => _state;

    public WaitHandle AsyncWaitHandle => throw new NotSupportedException("AsyncWaitHandle is not supported. Use callbacks instead.");

    public bool CompletedSynchronously => _completedSynchronously;

    public bool HasCallback => _callback != null;

    public bool IsCompleted => _isCompleted;

    protected void Complete(bool completedSynchronously)
    {
        if (_isCompleted)
        {
            throw Error.InvalidOperation(Handlers.Properties.Resources.AsyncResult_MultipleCompletes, GetType().Name);
        }

        _completedSynchronously = completedSynchronously;
        _isCompleted = true;

        if (_callback != null)
        {
            try
            {
                _callback(this);
            }
            catch (Exception e)
            {
                throw Error.InvalidOperation(e, Handlers.Properties.Resources.AsyncResult_CallbackThrewException);
            }
        }
    }

    protected void Complete(bool completedSynchronously, Exception exception)
    {
        _exception = exception;
        Complete(completedSynchronously);
    }

    protected static TAsyncResult End<TAsyncResult>(IAsyncResult result) where TAsyncResult : AsyncResult
    {
        if (result == null)
        {
            throw Error.ArgumentNull("result");
        }

        if (result is not TAsyncResult thisPtr)
        {
            throw Error.Argument("result", Handlers.Properties.Resources.AsyncResult_ResultMismatch);
        }

        if (!thisPtr._isCompleted)
        {
            thisPtr.AsyncWaitHandle.WaitOne();
        }

        if (thisPtr._endCalled)
        {
            throw Error.InvalidOperation(Handlers.Properties.Resources.AsyncResult_MultipleEnds);
        }

        thisPtr._endCalled = true;

        if (thisPtr._exception != null)
        {
            throw thisPtr._exception;
        }

        return thisPtr;
    }
}