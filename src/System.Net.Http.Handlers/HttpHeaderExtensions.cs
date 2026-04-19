// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Headers;

namespace System.Net.Http;

internal static class HttpHeaderExtensions
{
    extension(HttpContentHeaders fromHeaders)
    {
        public void CopyTo(HttpContentHeaders toHeaders)
        {
            ArgumentNullException.ThrowIfNull(fromHeaders);
            ArgumentNullException.ThrowIfNull(toHeaders);

            foreach (var header in fromHeaders)
            {
                toHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }
}