// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Internal;

namespace System.Net.Http.Handlers;

internal sealed class ProgressWriteAsyncResult : AsyncResult
{
    private static readonly AsyncCallback _writeCompletedCallback = WriteCompletedCallback;

    private readonly Stream _innerStream;
    private readonly ProgressStream _progressStream;
    private readonly int _count;

    public ProgressWriteAsyncResult(Stream innerStream, ProgressStream progressStream, byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        : base(callback, state)
    {
        ArgumentNullException.ThrowIfNull(innerStream);
        ArgumentNullException.ThrowIfNull(progressStream);
        ArgumentNullException.ThrowIfNull(buffer);

        _innerStream = innerStream;
        _progressStream = progressStream;
        _count = count;

        try
        {
            IAsyncResult result = innerStream.BeginWrite(buffer, offset, count, _writeCompletedCallback, this);
            if (result.CompletedSynchronously)
            {
                WriteCompleted(result);
            }
        }
        catch (Exception e)
        {
            Complete(true, e);
        }
    }

    private static void WriteCompletedCallback(IAsyncResult result)
    {
        if (result.CompletedSynchronously)
        {
            return;
        }

        if (result.AsyncState is ProgressWriteAsyncResult thisPtr)
        {
            try
            {
                thisPtr.WriteCompleted(result);
            }
            catch (Exception e)
            {
                thisPtr.Complete(false, e);
            }
        }
    }

    private void WriteCompleted(IAsyncResult result)
    {
        _innerStream.EndWrite(result);
        _progressStream.ReportBytesSent(_count, AsyncState);
        Complete(result.CompletedSynchronously);
    }

    public static void End(IAsyncResult result)
    {
        End<ProgressWriteAsyncResult>(result);
    }
}