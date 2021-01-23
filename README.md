# Serilog.Sinks.BrowserHttp [![Build status](https://ci.appveyor.com/api/projects/status/3cdhiwgd59sfdpg5?svg=true)](https://ci.appveyor.com/project/NicholasBlumhardt/serilog-sinks-browserhttp)

**Note:** this project is currently at the proof-of-concept stage, and may eat your laundry.

A Serilog sink for client-side Blazor that posts batched events using the browser's HTTP stack. These can be sent to any remote HTTP endpoint, including the app's origin server. A companion package, _Serilog.AspNetCore.Ingestion_, is also published from this repository, and can be used to relay events from the client to Serilog running in the ASP.NET Core server process.

### Getting started

In a Blazor client (WASM) app, first install _Serilog.Sinks.BrowserHttp_:

```
dotnet add package Serilog.Sinks.BrowserHttp
```

The sink is enabled using `WriteTo.BrowserHttp`:

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.ControlledBy(levelSwitch)
    .WriteTo.BrowserConsole()
    .WriteTo.BrowserHttp($"{builder.HostEnvironment.BaseAddress}ingest")
    .CreateLogger();

Log.Information("Hello, browser!");
```

Events will be `POST`ed in newline-delimited JSON batches to the given URL. See
 [_Serilog.Formatting.Compact_](https://github.com/serilog/serilog-formatting-compact) for a description of the JSON
schema that is used.

### Server-side relay for ASP.NET Core

To use the server-side relay in an ASP.NET Core app that uses Serilog, install _Serilog.AspNetCore.Ingestion_:

```
dotnet add package Serilog.AspNetCore.Ingestion
```

Then, add `UseSerilogIngestion()` to the app builder in _Startup.cs_:

```csharp
app.UseSerilogIngestion();
```

The client app should be configured with `LoggingLevelSwitch` so that the server can control the client's logging level. An endpoint address is not required. For example:

```csharp
// In a Blazor WASM Program.cs file
var builder = WebAssemblyHostBuilder.CreateDefault(args);
var levelSwitch = new LoggingLevelSwitch();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.ControlledBy(levelSwitch)
    .WriteTo.BrowserHttp($"{builder.HostEnvironment.BaseAddress}ingest", controlLevelSwitch: levelSwitch)
    .CreateLogger();

Log.Information("Hello, browser!");
```

See the `/samples` directory in this repository for a working end-to-end example.
