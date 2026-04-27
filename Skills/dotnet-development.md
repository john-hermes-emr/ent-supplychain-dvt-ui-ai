---
name: dotnet-development
description: Use this skill whenever the user is building, modifying, or troubleshooting .NET applications (C#, F#, VB.NET, .NET Core, .NET 6/7/8/9, ASP.NET, ASP.NET Core, Worker Services, console apps). Triggers include any mention of "C#", ".NET", "dotnet", "csproj", "NuGet", "appsettings.json", `IHostBuilder`, `HttpClient`, `async/await`, dependency injection, or requests to integrate .NET with Oracle services (OCI, ODI, Object Storage), file shares, or directory/file operations. Use proactively whenever the user references `.cs` files, project templates (`dotnet new`), or building Windows services / cross-platform daemons that read/write files or call cloud APIs. Do NOT use for pure scripting that's clearly Python territory or for non-.NET languages.
---

# .NET Development

A reference for building production-grade .NET applications, with emphasis on the patterns this project relies on: Oracle integration, cloud calls, long-running workers, and reliable file I/O.

## Decision Quick Reference

| Need | Use |
|------|-----|
| Console / batch job | `dotnet new console` |
| Long-running service | `dotnet new worker` (BackgroundService) |
| HTTP API | `dotnet new webapi` (Minimal APIs preferred for new code) |
| Outbound HTTP | `IHttpClientFactory` (never `new HttpClient()` per-call) |
| Configuration | `IConfiguration` + `IOptions<T>` from `appsettings.json` |
| Logging | `ILogger<T>` (Serilog as sink for structured logs) |
| Oracle DB | `Oracle.ManagedDataAccess.Core` |
| OCI SDK | `OCI.DotNetSDK.*` packages (per-service) |
| Async file I/O | `await File.ReadAllTextAsync` / `FileStream` with `useAsync: true` |
| JSON | `System.Text.Json` (default); `Newtonsoft.Json` only when required |

## Project Setup

Always pin the SDK with `global.json` so builds are reproducible across machines and CI:

```json
{ "sdk": { "version": "8.0.300", "rollForward": "latestFeature" } }
```

Standard project skeleton:

```bash
dotnet new sln -n MyApp
dotnet new worker -n MyApp.Worker -o src/MyApp.Worker
dotnet new classlib -n MyApp.Core -o src/MyApp.Core
dotnet new xunit -n MyApp.Tests -o tests/MyApp.Tests
dotnet sln add src/**/*.csproj tests/**/*.csproj
```

In each `.csproj`, prefer:

```xml
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <LangVersion>latest</LangVersion>
</PropertyGroup>
```

`Nullable=enable` is non-negotiable for new code — it eliminates a whole class of NullReferenceExceptions at compile time.

## Dependency Injection & Hosting

Every non-trivial app should use the Generic Host. It gives you DI, configuration, logging, lifetime management, and graceful shutdown for free.

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<OciOptions>(builder.Configuration.GetSection("Oci"));
builder.Services.AddHttpClient<IOdiClient, OdiClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Odi:BaseUrl"]!);
    c.Timeout = TimeSpan.FromMinutes(5);
});
builder.Services.AddSingleton<IObjectStorageService, ObjectStorageService>();
builder.Services.AddHostedService<IngestionWorker>();

var host = builder.Build();
await host.RunAsync();
```

## Configuration Pattern

Bind strongly-typed options classes — never sprinkle `IConfiguration["Foo:Bar"]` through business logic.

```csharp
public sealed class OciOptions
{
    public required string TenancyOcid { get; init; }
    public required string UserOcid { get; init; }
    public required string Region { get; init; }
    public required string KeyFingerprint { get; init; }
    public required string PrivateKeyPath { get; init; }
}

// Inject as IOptions<OciOptions> or IOptionsMonitor<OciOptions> for hot-reload.
```

Layer configuration sources in this order: `appsettings.json` → `appsettings.{Environment}.json` → environment variables → command-line args. Secrets go in environment variables, OCI Vault, or User Secrets (dev only) — never in committed JSON.

## Async & Cancellation

Async correctness is the #1 source of production bugs in .NET. Rules:

1. **Async all the way down.** Don't mix `.Result` / `.Wait()` with async code — it deadlocks under load.
2. **Always accept and pass `CancellationToken`.** Workers, HTTP handlers, and DB calls should all flow it through.
3. **`ConfigureAwait(false)` in libraries**, not in app code (ASP.NET Core / Generic Host have no sync context).
4. **Don't `async void`** except for event handlers — exceptions become process crashes.

```csharp
public async Task<IReadOnlyList<Record>> FetchAsync(CancellationToken ct)
{
    using var response = await _http.GetAsync("/records", ct);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<List<Record>>(ct)
           ?? throw new InvalidOperationException("Empty response");
}
```

## HTTP Client Pattern

`IHttpClientFactory` handles socket exhaustion, DNS refresh, and lets you add resilience handlers in one place.

```csharp
builder.Services.AddHttpClient<IOdiClient, OdiClient>()
    .AddStandardResilienceHandler(); // Retries, timeouts, circuit breaker (Microsoft.Extensions.Http.Resilience)
```

For OCI requests, do NOT roll your own request-signing — use the `Oci.Common` SDK (see the **oci-apis** skill).

## Logging

Use `ILogger<T>` everywhere. Use structured properties, never string concatenation:

```csharp
// Good — searchable, filterable
_logger.LogInformation("Loaded {RecordCount} records for tenant {TenantId}", records.Count, tenantId);

// Bad — opaque blob
_logger.LogInformation($"Loaded {records.Count} records for tenant {tenantId}");
```

For production, add Serilog with a JSON formatter so logs ship cleanly to OCI Logging or any aggregator.

## Oracle Database Access

Use `Oracle.ManagedDataAccess.Core`. It's pure managed code, no Instant Client needed.

```csharp
using var conn = new OracleConnection(connectionString);
await conn.OpenAsync(ct);
using var cmd = new OracleCommand("SELECT id, name FROM customers WHERE region = :region", conn);
cmd.BindByName = true;          // CRITICAL: default is by position, which is fragile
cmd.Parameters.Add("region", OracleDbType.Varchar2).Value = region;

using var reader = await cmd.ExecuteReaderAsync(ct);
while (await reader.ReadAsync(ct)) { /* ... */ }
```

`BindByName = true` is the single most important Oracle gotcha — without it, parameter order in your code must match SQL order, and bugs are silent.

## File I/O

For anything more than trivial reads, prefer streaming and async:

```csharp
await using var fs = new FileStream(
    path, FileMode.Open, FileAccess.Read, FileShare.Read,
    bufferSize: 81920, useAsync: true);
using var reader = new StreamReader(fs);
string? line;
while ((line = await reader.ReadLineAsync(ct)) is not null) { /* ... */ }
```

For atomic writes (critical when other processes watch the directory):

```csharp
var tmp = path + ".tmp";
await File.WriteAllBytesAsync(tmp, data, ct);
File.Move(tmp, path, overwrite: true);  // Atomic on same volume
```

See the **file-operations** skill for SMB/NFS share patterns and directory creation.

## Error Handling

- Catch the narrowest exception type that's actionable.
- Never `catch (Exception)` and swallow — at minimum log with the exception object (not `.Message`) so the stack trace survives.
- Wrap external calls (DB, HTTP, file I/O) in retry logic via Polly or `Microsoft.Extensions.Http.Resilience`.
- Throw `OperationCanceledException` up — don't convert it into a logged error.

```csharp
try { await DoWorkAsync(ct); }
catch (OperationCanceledException) { throw; }   // Honor cancellation
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "Upstream call failed for {Resource}", resource);
    throw;
}
```

## Testing

- xUnit + FluentAssertions + NSubstitute is a clean, modern stack.
- Use `WebApplicationFactory<TProgram>` for integration tests against your own API.
- Use Testcontainers (`Testcontainers.Oracle`) for tests that need a real Oracle XE instance.

## Cross-Skill References

- Calling Oracle Cloud APIs → **oci-apis** skill
- Buckets, objects, multipart uploads → **oracle-object-storage** skill
- Triggering ODI scenarios / load plans → **odi-automation** skill
- Mounting / reading SMB shares, atomic file moves → **file-operations** skill

## Common Pitfalls

1. **`new HttpClient()` per request.** Causes socket exhaustion under load — use `IHttpClientFactory`.
2. **Sync-over-async (`.Result`).** Will deadlock or starve thread pool. Make the call site async.
3. **Forgetting `BindByName = true`** in Oracle parameter binding. Silent data corruption.
4. **`async void`** outside event handlers. Unobserved exceptions crash the process.
5. **Building config strings manually.** Use `IConfiguration` + `IOptions<T>`.
6. **Logging `.Message` instead of the exception object.** Loses the stack trace.
7. **Creating directories without `Directory.CreateDirectory`** (it's idempotent — always safe to call).
