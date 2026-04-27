---
name: odi-automation
description: Use this skill whenever the user is automating Oracle Data Integrator (ODI) — invoking scenarios or load plans via REST/SDK, polling sessions, retrieving session logs, managing repositories or agents, or integrating ODI with .NET/Python orchestration code. Triggers include any mention of "ODI", "Oracle Data Integrator", "scenario", "load plan", "ODI Agent" (Standalone, Colocated, JEE), "Master Repository", "Work Repository", "ODI Studio", "ODI SDK", `/oracle/odi/v1/` REST endpoints, `oracledi`, `startscen.sh`, `OdiInvokeRESTfulService`, or running data-integration jobs from an external scheduler. Use proactively when the user wants to trigger an ETL job from an app, monitor its status, or chain ODI executions to file/object-storage events.
---

# Oracle Data Integrator (ODI) Automation

How to drive ODI from outside ODI — kick off scenarios, monitor sessions, parse logs, and integrate with the rest of the application stack.

## Mental Model

| Concept | Description |
|---------|-------------|
| **Master Repository** | Holds topology (technologies, data servers, agents), security (users, profiles), and version-control metadata. One per environment. |
| **Work Repository** | Holds the development objects (mappings, packages, scenarios, load plans) and runtime data (sessions, logs). Can be **Development** or **Execution-only**. |
| **Scenario** | A frozen, executable version of a package, mapping, procedure, or variable. The unit of automation. |
| **Load Plan** | A hierarchical orchestration of scenarios with parallel/serial steps, exception handling, and restart points. |
| **Agent** | The runtime that executes scenarios. Three flavors: **Standalone**, **Standalone Colocated** (managed by WebLogic), **JEE** (deployed in WebLogic). |
| **Session** | One execution instance of a scenario or load plan. Has a numeric ID, status, start/end times, step-level details. |

Rule: from automation code, you talk to an **Agent**. The agent talks to the repositories.

## How to Trigger ODI From Outside

There are five practical paths. Pick by what's available:

| Path | When to use |
|------|-------------|
| **Public REST API** (12.2.1.3+) | First choice for new integrations. Language-neutral. |
| **OdiInvokeWebService tool** | When ODI calls *another* web service from inside a scenario. |
| **`startscen` / `startloadplan` CLI** | Shell-based schedulers (cron, OEM, Autosys). Simple but coarse. |
| **Java SDK** (`odi-sdk`) | When you need fine-grained control or are inside a Java/JEE app. |
| **OdiInvokeRESTfulService** | When ODI itself needs to call an external REST API. |

The rest of this skill focuses on the REST API path because it's how .NET and Python apps talk to ODI.

## ODI Public REST API

### Base URL

JEE Agent: `http://<weblogic-host>:<port>/oracle/odi/agent/`
Standalone Agent (with REST enabled): `http://<host>:<port>/oraclediagent/`

The runtime endpoints live under `/oracle/odi/v1/` on JEE/colocated agents.

### Authentication

Basic Auth with an ODI user that has runtime permission. For production, front the agent with a load balancer enforcing TLS, and rotate the runtime credentials via OCI Vault.

### Execute a Scenario

```
POST /oracle/odi/v1/runtime/scenarios/{scenName}/{scenVersion}/executions
Content-Type: application/json
Authorization: Basic <base64(user:pass)>
```

Body — minimum:

```json
{
  "OdiUser": "SUPERVISOR",
  "Synchronous": false,
  "Context": "GLOBAL",
  "LogLevel": 5,
  "Variables": [
    { "Name": "GLOBAL.RUN_DATE", "Value": "2026-04-24" }
  ]
}
```

Use `"Synchronous": false` for anything non-trivial — keep the request short and poll for completion. The response contains a `Session` object with the session number, which you'll use to poll.

### Execute a Load Plan

```
POST /oracle/odi/v1/runtime/loadplans/{loadPlanName}/executions
```

Same body shape, with `"KeywordList"` and `"LogLevel"` typically set.

### Poll a Session

```
GET /oracle/odi/v1/runtime/sessions/{sessionNumber}
```

Returns a payload with `Status` ∈ `{D=Done, E=Error, R=Running, W=Waiting, Q=Queued, M=Warning}`. Poll with backoff — start at 5 s, cap at 60 s, and bail after a configured timeout.

### Retrieve Session Errors / Steps

```
GET /oracle/odi/v1/runtime/sessions/{sessionNumber}/steps
GET /oracle/odi/v1/runtime/sessions/{sessionNumber}/tasks
```

These return per-step status and the stored error message. Long error texts and SQL get truncated — for full detail, query the work repository's `SNP_SESS_TASK_LOG` view directly via JDBC if you need it.

### Restart a Session

```
POST /oracle/odi/v1/runtime/sessions/{sessionNumber}/restart
```

Restarts from the last failed step (default behavior depends on scenario configuration).

## Python Reference Implementation

Drop-in pattern. Uses `httpx` and `tenacity` from the **python-development** skill.

```python
import time, logging, httpx
from typing import Any
from tenacity import retry, stop_after_attempt, wait_exponential, retry_if_exception_type

log = logging.getLogger(__name__)

class OdiClient:
    def __init__(self, base_url: str, user: str, password: str, timeout: float = 30.0):
        self._client = httpx.Client(
            base_url=base_url.rstrip("/"),
            auth=(user, password),
            timeout=timeout,
            headers={"Content-Type": "application/json"},
        )
        self._user = user

    def close(self) -> None:
        self._client.close()

    @retry(stop=stop_after_attempt(3),
           wait=wait_exponential(min=2, max=20),
           retry=retry_if_exception_type(httpx.HTTPError))
    def execute_scenario(self, name: str, version: str = "-1",
                         context: str = "GLOBAL", variables: dict[str, Any] | None = None,
                         log_level: int = 5) -> int:
        body = {
            "OdiUser": self._user,
            "Synchronous": False,
            "Context": context,
            "LogLevel": log_level,
            "Variables": [{"Name": k, "Value": v} for k, v in (variables or {}).items()],
        }
        r = self._client.post(
            f"/oracle/odi/v1/runtime/scenarios/{name}/{version}/executions",
            json=body,
        )
        r.raise_for_status()
        session = r.json()["Session"]
        log.info("ODI scenario started", extra={"scenario": name, "session": session})
        return int(session)

    def get_session(self, session_id: int) -> dict[str, Any]:
        r = self._client.get(f"/oracle/odi/v1/runtime/sessions/{session_id}")
        r.raise_for_status()
        return r.json()

    def wait_for_completion(self, session_id: int,
                            poll_initial: float = 5.0, poll_max: float = 60.0,
                            timeout: float = 3600.0) -> dict[str, Any]:
        start = time.monotonic()
        delay = poll_initial
        while True:
            data = self.get_session(session_id)
            status = data.get("Status")
            if status in {"D", "E", "M"}:           # Done / Error / Warning
                return data
            if time.monotonic() - start > timeout:
                raise TimeoutError(f"ODI session {session_id} did not finish in {timeout}s")
            time.sleep(delay)
            delay = min(delay * 1.5, poll_max)
```

Usage:

```python
with OdiClient("https://odi.example.com", "RUNTIME_USER", secret) as odi:
    sid = odi.execute_scenario("LOAD_CUSTOMERS", variables={"GLOBAL.RUN_DATE": "2026-04-24"})
    final = odi.wait_for_completion(sid, timeout=1800)
    if final["Status"] != "D":
        raise RuntimeError(f"ODI session {sid} ended with status {final['Status']}")
```

## .NET Reference Implementation

```csharp
public sealed class OdiClient
{
    private readonly HttpClient _http;
    private readonly string _user;
    private readonly ILogger<OdiClient> _log;

    public OdiClient(HttpClient http, IOptions<OdiOptions> opts, ILogger<OdiClient> log)
    {
        _http = http;
        _user = opts.Value.Username;
        _log  = log;
        var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_user}:{opts.Value.Password}"));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic);
    }

    public async Task<long> ExecuteScenarioAsync(
        string name, string version = "-1", string context = "GLOBAL",
        IDictionary<string, string>? variables = null, int logLevel = 5,
        CancellationToken ct = default)
    {
        var body = new
        {
            OdiUser     = _user,
            Synchronous = false,
            Context     = context,
            LogLevel    = logLevel,
            Variables   = (variables ?? new Dictionary<string, string>())
                          .Select(kv => new { Name = kv.Key, Value = kv.Value })
        };
        using var resp = await _http.PostAsJsonAsync(
            $"/oracle/odi/v1/runtime/scenarios/{name}/{version}/executions", body, ct);
        resp.EnsureSuccessStatusCode();
        var doc = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        var session = doc.GetProperty("Session").GetInt64();
        _log.LogInformation("ODI scenario {Name} started as session {Session}", name, session);
        return session;
    }

    public async Task<JsonElement> GetSessionAsync(long sessionId, CancellationToken ct = default)
    {
        using var resp = await _http.GetAsync($"/oracle/odi/v1/runtime/sessions/{sessionId}", ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
    }

    public async Task<JsonElement> WaitForCompletionAsync(
        long sessionId, TimeSpan timeout, CancellationToken ct = default)
    {
        var deadline = DateTime.UtcNow + timeout;
        var delay = TimeSpan.FromSeconds(5);
        while (true)
        {
            var data = await GetSessionAsync(sessionId, ct);
            var status = data.GetProperty("Status").GetString();
            if (status is "D" or "E" or "M") return data;
            if (DateTime.UtcNow > deadline)
                throw new TimeoutException($"ODI session {sessionId} did not finish in {timeout}");
            await Task.Delay(delay, ct);
            delay = TimeSpan.FromSeconds(Math.Min(60, delay.TotalSeconds * 1.5));
        }
    }
}
```

Register in DI:

```csharp
builder.Services.AddHttpClient<OdiClient>(c => c.BaseAddress = new Uri(opts.BaseUrl))
                .AddStandardResilienceHandler();
```

## CLI: `startscen` / `startloadplan`

For schedulers that only run shell commands:

```bash
# Standalone agent script
$ODI_HOME/bin/startscen.sh -NAME=LOAD_CUSTOMERS -VERSION=-1 \
    -CONTEXT=GLOBAL -AGENT_URL=http://odi-agent:20910/oraclediagent \
    "GLOBAL.RUN_DATE=2026-04-24"

$ODI_HOME/bin/startloadplan.sh LP_DAILY GLOBAL \
    -AGENT_URL=http://odi-agent:20910/oraclediagent
```

These return the session number on stdout. Exit code is 0 on submission, not on session success — you must still poll.

## Java SDK (when REST is not enough)

When you're embedding ODI work into a Java/JEE app, the Java SDK gives you direct repository access:

```java
OdiInstance odi = OdiInstance.createInstance(
    new OdiInstanceConfig(masterRepoConfig, workRepoConfig));

ITransactionStatus tx = odi.getTransactionManager().getTransaction(new DefaultTransactionDefinition());
StartupParams params = new StartupParams();
OdiScenario scen = ((IOdiScenarioFinder) odi.getFinder(OdiScenario.class))
                       .findLatestByName("LOAD_CUSTOMERS");
new RuntimeAgent("http://agent:20910/oraclediagent", "user", "pass")
    .startScenario(scen.getName(), scen.getVersion(), params, "GLOBAL", "GLOBAL", 5, true);
odi.close();
```

This is rarely needed from .NET/Python apps and is mentioned for completeness.

## Variables, Contexts, and Scopes

Variables come in three scopes — get the prefix right or your value is silently ignored:

- **`GLOBAL.<NAME>`** — global, declared at project level under Global Variables.
- **`PROJECT.<NAME>`** — project-scoped.
- **`<PROJECT>.<NAME>`** — fully-qualified to a specific project's variable.

Always use the fully-qualified form in API calls. Context (`GLOBAL`, `DEV`, `PROD`, etc.) determines which physical schemas the logical schemas resolve to.

## Logging Levels

The `LogLevel` field accepts `0`–`5`:

- `0` — Errors only.
- `3` — Default for scheduled production. Step + error.
- `5` — Verbose, includes all generated SQL. Use for debugging only — produces large logs.

## Patterns That Bite People

1. **Synchronous scenarios over slow networks** — the HTTP call sits open for the whole run; any proxy/LB timeout kills your client even though the scenario keeps running. Always async + poll.
2. **Polling without backoff** — flooding the agent with `GET /sessions/{id}` calls. Use exponential backoff capped at 30–60 s.
3. **Restarting a session that wasn't designed to restart** — a scenario must be marked `Restart from failed step` in its physical design. Otherwise restart starts from step 1 and may double-load.
4. **Missing context** — submitting without `Context` defaults the agent's startup context, which on shared agents may not be what you wanted.
5. **Embedding repository connection strings in app code** — use the agent. The repository is for ODI Studio, not for application code.
6. **Confusing scenario name with package name** — when you generate a scenario from a package, the scenario name defaults to the package name but they are separate objects.
7. **Authentication 401 with "internal user"** — the user must be a *runtime* user, not a repository-only user, and must have execute permissions on the scenario in that context.

## End-to-End Pattern (Common Workflow)

A typical orchestration in this project looks like:

1. A file lands on a **file share** (or in **Object Storage**).
2. A .NET or Python worker picks it up.
3. The worker uploads it to **Object Storage** (if not already there).
4. The worker calls **ODI** to start the load scenario, passing the object key as a variable.
5. The worker polls until the session completes.
6. The worker writes a result record and moves the source file to an `archive/` prefix.

Cross-skill references for each step:

- File pickup, atomic moves, share mount → **file-operations** skill
- Object Storage upload → **oracle-object-storage** skill
- Authentication to OCI services → **oci-apis** skill
- Worker host process → **dotnet-development** or **python-development** skill

## Common Pitfalls

1. **Treating `200 OK` from `executions` as "the job succeeded"** — it means "ODI accepted the request." Always poll the session.
2. **Hardcoding agent URLs** — they change between environments. Pass via config, with a separate value per environment.
3. **Logging `LogLevel=5` permanently** — fills the work repository fast. Reserve for debugging.
4. **Silent variable injection failures** — wrong prefix means ODI never reads it; the run uses the default and produces wrong output. Echo received variables in the first step of every scenario during dev.
5. **Forgetting `Context`** — defaults are environment-specific.
