---
name: python-development
description: Use this skill whenever the user is building, modifying, or troubleshooting Python applications, scripts, or automation — especially when the work involves Oracle Cloud Infrastructure, Oracle Data Integrator, Oracle Object Storage, file shares, or batch/ETL-style file processing. Triggers include any mention of `.py` files, `pip`, `poetry`, `uv`, `requirements.txt`, `pyproject.toml`, virtual environments (`venv`, `virtualenv`), the `oci` SDK, `requests`, `httpx`, `pathlib`, `pydantic`, `asyncio`, FastAPI, Flask, Typer/Click CLIs, or any Python automation that reads/writes files or calls cloud APIs. Use proactively whenever the user asks to "write a script," automate a workflow, or build a CLI/worker that touches Oracle services. Do NOT use for pure C#/.NET work.
---

# Python Development

Production patterns for Python applications in this project: OCI/ODI integration, file-share processing, robust scripts and CLIs.

## Decision Quick Reference

| Need | Use |
|------|-----|
| Project + dep management | `uv` (fast) or `poetry`; fallback `pip` + `requirements.txt` |
| Virtual env | `python -m venv .venv` (or `uv venv`) |
| HTTP client | `httpx` (sync + async); `requests` for simple sync-only |
| OCI SDK | `oci` (the official Oracle SDK) |
| Oracle DB | `oracledb` (the modern replacement for `cx_Oracle`) |
| Config | `pydantic-settings` (typed) or `python-dotenv` (simple) |
| Paths/files | `pathlib.Path` — never `os.path` string-mashing in new code |
| CLI | `typer` (Click-based, type-hint-driven) |
| Async | `asyncio` + `httpx.AsyncClient`; avoid mixing sync/async libs |
| Logging | stdlib `logging` with `logging.config.dictConfig` |
| Testing | `pytest` + `pytest-asyncio` + `respx` (httpx mocking) |

## Project Setup

Pin the Python version with `.python-version` (pyenv) and use `pyproject.toml` as the source of truth.

Minimal `pyproject.toml`:

```toml
[project]
name = "myapp"
version = "0.1.0"
requires-python = ">=3.11"
dependencies = [
  "oci>=2.130",
  "httpx>=0.27",
  "pydantic-settings>=2.4",
  "typer>=0.12",
  "oracledb>=2.4",
]

[project.optional-dependencies]
dev = ["pytest>=8", "pytest-asyncio>=0.23", "respx>=0.21", "ruff>=0.6", "mypy>=1.11"]

[tool.ruff]
line-length = 100
target-version = "py311"

[tool.ruff.lint]
select = ["E", "F", "I", "B", "UP", "SIM", "RUF"]
```

Standard layout (src layout — keeps tests honest about importability):

```
myapp/
├── pyproject.toml
├── src/myapp/__init__.py
├── src/myapp/cli.py
├── src/myapp/config.py
└── tests/test_cli.py
```

## Configuration

Use `pydantic-settings` so config is typed, validated, and self-documenting.

```python
from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict

class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", env_prefix="MYAPP_")

    oci_config_path: str = Field(default="~/.oci/config")
    oci_profile: str = "DEFAULT"
    object_storage_bucket: str
    odi_base_url: str
    odi_username: str
    odi_password: str  # In prod, source from OCI Vault, not env

settings = Settings()  # Fails fast at import if required vars are missing
```

Secrets: prefer OCI Vault or instance principal. Do not commit `.env` files; do commit `.env.example`.

## Logging

Use `logging.config.dictConfig` so every entry point gets the same setup. Use `extra={...}` for structured fields:

```python
import logging
import logging.config

logging.config.dictConfig({
    "version": 1,
    "formatters": {"json": {"format": "%(asctime)s %(levelname)s %(name)s %(message)s"}},
    "handlers": {"stdout": {"class": "logging.StreamHandler", "formatter": "json"}},
    "root": {"level": "INFO", "handlers": ["stdout"]},
})

log = logging.getLogger(__name__)
log.info("Loaded records", extra={"count": len(records), "tenant": tenant_id})
```

Never use `print()` in production code paths — it bypasses log levels and aggregation.

## HTTP — `httpx` Patterns

`httpx` works for both sync and async. Reuse the client; never create one per call.

```python
import httpx

with httpx.Client(base_url=settings.odi_base_url, timeout=30.0) as client:
    r = client.post("/oracle/odi/v1/scenarios/MY_SCEN/Version/-1/execute",
                    auth=(settings.odi_username, settings.odi_password),
                    json={"OdiUser": settings.odi_username})
    r.raise_for_status()
    payload = r.json()
```

Async variant — same shape, `await` everywhere:

```python
async with httpx.AsyncClient(base_url=base, timeout=30.0) as client:
    r = await client.get("/v1/things")
    r.raise_for_status()
```

Add retries with `tenacity` for idempotent calls:

```python
from tenacity import retry, stop_after_attempt, wait_exponential, retry_if_exception_type

@retry(stop=stop_after_attempt(5),
       wait=wait_exponential(multiplier=1, min=2, max=30),
       retry=retry_if_exception_type(httpx.HTTPError))
def fetch(client, path): ...
```

## Async Rules

1. Don't mix sync and async libraries in the same code path. If you call `requests` inside an `async def`, you've lost the benefit.
2. `asyncio.gather` for parallel I/O; `asyncio.Semaphore` to cap concurrency.
3. Use `asyncio.run(main())` as the single entry point — don't manage loops manually.
4. `async def` functions must be awaited or scheduled — a forgotten `await` is a silent no-op.

## Oracle Database Access

Use the modern `oracledb` driver in **thin mode** by default — no Instant Client needed:

```python
import oracledb

with oracledb.connect(user=u, password=p, dsn="host:1521/svc") as conn:
    with conn.cursor() as cur:
        cur.execute("SELECT id, name FROM customers WHERE region = :region", region="EMEA")
        for row in cur:
            ...
```

Always use bind variables (`:name`) — never f-string SQL. SQL injection aside, the database stops sharing cursors and your performance falls off a cliff.

## File I/O

Use `pathlib`. Always.

```python
from pathlib import Path

inbox = Path(settings.inbox_dir)
inbox.mkdir(parents=True, exist_ok=True)

for csv in inbox.glob("*.csv"):
    text = csv.read_text(encoding="utf-8")
    ...
```

For large files, stream:

```python
with path.open("rb") as src, dest.open("wb") as dst:
    while chunk := src.read(1024 * 1024):
        dst.write(chunk)
```

For atomic writes (required when other processes watch the directory):

```python
import os, tempfile

def atomic_write_bytes(path: Path, data: bytes) -> None:
    fd, tmp = tempfile.mkstemp(dir=path.parent, prefix=path.name + ".", suffix=".tmp")
    try:
        with os.fdopen(fd, "wb") as f:
            f.write(data)
            f.flush()
            os.fsync(f.fileno())
        os.replace(tmp, path)  # atomic on POSIX & Windows when on same volume
    except Exception:
        Path(tmp).unlink(missing_ok=True)
        raise
```

See the **file-operations** skill for SMB/NFS share specifics.

## CLIs with Typer

```python
import typer
app = typer.Typer(help="Ingest CSVs into Object Storage.")

@app.command()
def ingest(folder: Path = typer.Argument(..., exists=True, file_okay=False),
           bucket: str = typer.Option(..., envvar="MYAPP_BUCKET"),
           dry_run: bool = False):
    """Upload every CSV in FOLDER to BUCKET."""
    ...

if __name__ == "__main__":
    app()
```

## Error Handling

Catch narrow, log with context, re-raise unless you can actually recover:

```python
try:
    upload(path)
except oci.exceptions.ServiceError as e:
    log.error("OCI rejected upload", extra={"path": str(path), "status": e.status, "code": e.code})
    raise
except OSError as e:
    log.error("Local I/O failed", extra={"path": str(path), "errno": e.errno})
    raise
```

Use `except Exception:` only at the top of a worker loop, where you log and continue to the next item.

## Testing

```python
# tests/test_uploader.py
import respx, httpx, pytest

@respx.mock
def test_uploader_retries_on_503():
    route = respx.put("https://objectstorage.example/o/key").mock(
        side_effect=[httpx.Response(503), httpx.Response(200)]
    )
    upload(...)
    assert route.call_count == 2
```

Use `pytest.fixture` for shared setup, `tmp_path` for filesystem isolation, and `monkeypatch` for env vars.

## Cross-Skill References

- OCI authentication, signing, SDK usage → **oci-apis** skill
- Buckets, objects, multipart uploads, PARs → **oracle-object-storage** skill
- ODI scenarios, load plans, REST endpoints → **odi-automation** skill
- SMB/NFS shares, mount points, atomic moves → **file-operations** skill

## Common Pitfalls

1. **Mutable default arguments** (`def f(x=[]):`) — a classic. Use `None` and assign inside.
2. **Mixing sync `requests` inside async code** — blocks the event loop.
3. **Forgetting `raise_for_status()`** — `httpx`/`requests` don't raise on 4xx/5xx by default.
4. **F-string SQL** — instant SQL injection + cursor cache thrash.
5. **`os.path.join` in new code** — `pathlib` is clearer and cross-platform.
6. **Catching `Exception` and swallowing** — debug nightmares. At least log with `exc_info=True`.
7. **Global state for clients** at module-import time — breaks testing and lifecycle. Build them in a factory.
