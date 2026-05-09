// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http.Handlers;

/// <summary>
/// The <see cref="ProgressMessageHandler"/> provides a mechanism for getting progress event notifications
/// when sending and receiving data in connection with exchanging HTTP requests and responses.
/// Register event handlers for the events <see cref="HttpSendProgress"/> and <see cref="HttpReceiveProgress"/>
/// to see events for data being sent and received.
/// </summary>
public class ProgressMessageHandler : DelegatingHandler
{
#if NET6_0_OR_GREATER
    public static readonly HttpRequestOptionsKey<IProgress<HttpProgressEventArgs>> HttpSendProgressKey = new(nameof(HttpSendProgressKey));

    public static readonly HttpRequestOptionsKey<IProgress<HttpProgressEventArgs>> HttpReceiveProgressKey = new(nameof(HttpReceiveProgressKey));
#endif
    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressMessageHandler"/> class.
    /// </summary>
    public ProgressMessageHandler()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressMessageHandler"/> class.
    /// </summary>
    /// <param name="innerHandler">The inner handler to which this handler submits requests.</param>
    public ProgressMessageHandler(HttpMessageHandler innerHandler)
        : base(innerHandler)
    {
    }

    /// <summary>
    /// Occurs every time the client sending data is making progress.
    /// </summary>
    public event EventHandler<HttpProgressEventArgs>? HttpSendProgress;

    /// <summary>
    /// Occurs every time the client receiving data is making progress.
    /// </summary>
    public event EventHandler<HttpProgressEventArgs>? HttpReceiveProgress;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (HttpSendProgress is not null
#if NET6_0_OR_GREATER
            || request.Options.TryGetValue(HttpSendProgressKey, out _)
#endif
            )
        {
            AddRequestProgress(request);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (HttpReceiveProgress is not null
#if NET6_0_OR_GREATER
            || request.Options.TryGetValue(HttpReceiveProgressKey, out _)
#endif
            )
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AddResponseProgressAsync(request, response);
        }

        return response;
    }

    /// <summary>
    /// Raises the <see cref="HttpSendProgress"/> event.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="e">The <see cref="HttpProgressEventArgs"/> instance containing the event data.</param>
    protected internal virtual void OnHttpRequestProgress(HttpRequestMessage request, HttpProgressEventArgs e)
    {
        HttpSendProgress?.Invoke(request, e);
#if NET6_0_OR_GREATER
        if (request.Options.TryGetValue(HttpSendProgressKey, out var progress))
        {
            progress.Report(e);
        }
#endif
    }

    /// <summary>
    /// Raises the <see cref="HttpReceiveProgress"/> event.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="e">The <see cref="HttpProgressEventArgs"/> instance containing the event data.</param>
    protected internal virtual void OnHttpResponseProgress(HttpRequestMessage request, HttpProgressEventArgs e)
    {
        HttpReceiveProgress?.Invoke(request, e);
#if NET6_0_OR_GREATER
        if (request.Options.TryGetValue(HttpReceiveProgressKey, out var progress))
        {
            progress.Report(e);
        }
#endif
    }

    private void AddRequestProgress(HttpRequestMessage request)
    {
        if (request is not { Content: var content } || content is null)
        {
            return;
        }

        request.Content = new ProgressContent(content, this, request);
    }

    private async Task<HttpResponseMessage> AddResponseProgressAsync(HttpRequestMessage request, HttpResponseMessage response)
    {
        if (response.Content is not HttpContent content)
        {
            return response;
        }

        var stream = await content.ReadAsStreamAsync();

        var progressContent = new StreamContent(new ProgressStream(stream, this, request, response));

        content.Headers.CopyTo(progressContent.Headers);
        response.Content = progressContent;

        return response;
    }
}