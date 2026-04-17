# System.Net.Http.Handlers&nbsp;[![NuGet Version](https://img.shields.io/nuget/v/RoccoZero.System.Net.Http.Handlers.svg?style=flat)](https://www.nuget.org/packages/RoccoZero.System.Net.Http.Handlers)

Simple progress tracking for `HttpClient` via `DelegatingHandler`.

This package is based on the original `ProgressMessageHandler` from ASP.NET Web Stack (`System.Net.Http.Handlers`) and adapts the same idea for modern .NET. The original source lives in the `aspnet/AspNetWebStack` repository under

## Install

```bash
dotnet add package RoccoZero.System.Net.Http.Handlers
```

```csharp
var progressMessageHandler = new ProgressMessageHandler(new HttpClientHandler());

progressMessageHandler.HttpSendProgress += (_, e) =>
{
    if (e.TotalBytes.HasValue && e.TotalBytes.Value > 0)
    {
        var percent = (double)e.BytesTransferred / e.TotalBytes.Value * 100;
        Console.WriteLine($"Upload: {percent:F2}%");
    }
    else
    {
        Console.WriteLine($"Upload: {e.BytesTransferred} bytes");
    }
};

progressMessageHandler.HttpReceiveProgress += (_, e) =>
{
    if (e.TotalBytes.HasValue && e.TotalBytes.Value > 0)
    {
        var percent = (double)e.BytesTransferred / e.TotalBytes.Value * 100;
        Console.WriteLine($"Download: {percent:F2}%");
    }
    else
    {
        Console.WriteLine($"Download: {e.BytesTransferred} bytes");
    }
};

using var httpClient = new HttpClient(progressMessageHandler);
using var response = await httpClient.GetAsync(
    "https://builds.dotnet.microsoft.com/dotnet/Sdk/10.0.202/dotnet-sdk-10.0.202-win-x64.exe");

response.EnsureSuccessStatusCode();
```

## Why
* No manual stream wrapping
* Upload + download progress
* Works directly with HttpClient
* Clean and reusable