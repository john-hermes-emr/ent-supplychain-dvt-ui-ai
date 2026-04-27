---
name: file-operations
description: Use this skill whenever the work involves the local or networked filesystem — creating directories, creating/reading/writing/moving/deleting files, working with file shares (SMB / CIFS / NFS), mounting network drives on Linux or Windows, watching directories for new files, performing atomic writes, handling permissions, or coordinating local file operations with cloud storage. Triggers include any mention of "file share", "SMB", "CIFS", "NFS", "UNC path", "mount", "fstab", "net use", `Directory.CreateDirectory`, `Path.mkdir`, `os.makedirs`, `pathlib`, file watcher / `FileSystemWatcher` / `watchdog`, atomic move, drop folder, inbox/outbox folder pattern, or transferring files between a share and Oracle Object Storage. Use proactively when the user describes a workflow that picks up files from one location and delivers them to another.
---

# File and Directory Operations

Reliable filesystem patterns for application code that picks up files from shares, processes them, and hands them off to cloud storage or downstream systems.

## Core Principles

1. **Operations on a filesystem are not atomic by default.** You have to design for it.
2. **A directory listing is a snapshot, not a stream.** Files can appear, disappear, or change between calls.
3. **Network filesystems lie about latency and consistency.** Treat SMB/NFS calls as fallible network I/O, not memory-speed operations.
4. **Permissions on shared filesystems are different from local.** Test on the actual share, not on `/tmp`.

## Directory Creation

Always idempotent — call without checking first.

**Python:**

```python
from pathlib import Path
Path("/data/inbox/2026/04/24").mkdir(parents=True, exist_ok=True)
```

**.NET:**

```csharp
Directory.CreateDirectory(@"D:\data\inbox\2026\04\24");  // No-op if it already exists
```

**Bash:**

```bash
mkdir -p /data/inbox/2026/04/24
```

`-p` / `parents=True` / no-arg is the right default. The "check then create" pattern (`if not exists; mkdir`) introduces a race condition for no benefit.

## File Creation Idioms

### Write fully then publish — atomic move

The single most useful pattern. Writing in-place is dangerous when other processes (including ODI agents and file watchers) may scan the directory.

**Python:**

```python
import os, tempfile
from pathlib import Path

def atomic_write_bytes(path: Path, data: bytes) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    fd, tmp = tempfile.mkstemp(dir=path.parent, prefix=path.name + ".", suffix=".tmp")
    try:
        with os.fdopen(fd, "wb") as f:
            f.write(data)
            f.flush()
            os.fsync(f.fileno())
        os.replace(tmp, path)        # Atomic on same volume on POSIX & Windows
    except Exception:
        Path(tmp).unlink(missing_ok=True)
        raise
```

**.NET:**

```csharp
public static async Task AtomicWriteAsync(string path, byte[] data, CancellationToken ct = default)
{
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    var tmp = path + ".tmp";
    await using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write,
                                         FileShare.None, 81920, useAsync: true))
    {
        await fs.WriteAsync(data, ct);
        await fs.FlushAsync(ct);
    }
    File.Move(tmp, path, overwrite: true);  // Atomic on same volume
}
```

The atomic `replace` / `Move(overwrite: true)` only works **within the same volume**. Across volumes it's copy + delete, which is *not* atomic. If you need cross-volume, write to a hidden file in the destination volume first, then rename.

### Sentinel files — signal "done"

When the consumer can't tell if a write is complete, write a `.done` file *after* the data file:

```python
data_path = Path("/share/inbox/orders.csv")
done_path = data_path.with_suffix(".csv.done")
atomic_write_bytes(data_path, payload)
done_path.touch()
```

Consumer waits for `*.done`, then reads the matching `*.csv`. This is the standard handshake when you can't control both sides.

## Path Handling

Use `pathlib.Path` (Python) and `Path` / `Path.Combine` (.NET) — never string concatenation. Both libraries handle the slash differences and edge cases:

```python
inbox = Path("/srv") / "share" / "inbox"
for csv in inbox.glob("*.csv"):
    print(csv.name, csv.stat().st_size)
```

```csharp
var inbox = Path.Combine("D:\\srv", "share", "inbox");
foreach (var csv in Directory.EnumerateFiles(inbox, "*.csv"))
    Console.WriteLine($"{Path.GetFileName(csv)} {new FileInfo(csv).Length}");
```

### UNC Paths (.NET / Windows)

UNC paths look like `\\server\share\path`. In code:

```csharp
var path = @"\\fileserver\departments\finance\inbox\file.csv";
File.Exists(path);
```

Long UNC paths require the `\\?\UNC\` prefix on older frameworks (`\\?\UNC\server\share\...`). On modern .NET (Core 3.1+), long paths work transparently if the OS has long-path support enabled.

## File Shares — SMB / CIFS

### Linux: mount an SMB share

```bash
sudo apt-get install -y cifs-utils
sudo mkdir -p /mnt/finance
sudo mount -t cifs //fileserver/finance /mnt/finance \
    -o username=svc_app,uid=1000,gid=1000,vers=3.0,iocharset=utf8,credentials=/etc/cifs.cred
```

Where `/etc/cifs.cred` contains:

```
username=svc_app
password=...
domain=CORP
```

Permissions on the credentials file should be `600`. For persistent mounts, add to `/etc/fstab`:

```
//fileserver/finance  /mnt/finance  cifs  credentials=/etc/cifs.cred,uid=1000,gid=1000,vers=3.0,_netdev,nofail  0  0
```

`_netdev,nofail` keeps a slow share from blocking boot.

### Windows: map a network drive

```powershell
# Persistent
New-PSDrive -Name "Z" -PSProvider FileSystem -Root "\\fileserver\finance" `
    -Credential (Get-Credential) -Persist

# Or for service accounts
net use Z: \\fileserver\finance /user:CORP\svc_app *
```

Inside a Windows service running as a domain account, **do not map drive letters**. Drive mappings are per-session and may not be visible to the service. Use UNC paths directly.

### Permissions Gotchas

- SMB mounts on Linux project ownership through the `uid`/`gid` mount options. The remote ACL still applies on top.
- A file you can read with `cat` may not be readable by your service account if SELinux is enforcing — check `audit.log`.
- On Windows, the *service account* needs the share permission. "It works when I RDP in" is not a useful test.

## File Shares — NFS

### Linux: mount NFS

```bash
sudo apt-get install -y nfs-common
sudo mkdir -p /mnt/data
sudo mount -t nfs -o vers=4.1,sec=sys,hard,timeo=600 nfs.example.com:/export/data /mnt/data
```

Persistent (`/etc/fstab`):

```
nfs.example.com:/export/data  /mnt/data  nfs  vers=4.1,hard,timeo=600,_netdev,nofail  0  0
```

`hard` (vs `soft`) means a hung server will block I/O instead of returning errors. For data integrity on writes, `hard` is almost always correct. Pair with timeouts at the application layer.

### OCI File Storage Service (FSS)

OCI's managed NFS. From an OCI VM in the same VCN:

```bash
sudo mount -t nfs -o nconnect=16 10.0.0.5:/myfs /mnt/myfs
```

The mount target lives on a private IP in your VCN. Open NFS ports (`2048-2050`, `111`) in the security list/NSG. `nconnect=16` is the standard performance bump on modern kernels.

## Watching for New Files

### Python — `watchdog`

```python
from watchdog.observers import Observer
from watchdog.events import PatternMatchingEventHandler
from pathlib import Path

class InboxHandler(PatternMatchingEventHandler):
    def __init__(self):
        super().__init__(patterns=["*.csv"], ignore_directories=True)

    def on_closed(self, event):              # Fired when the writer closes the file
        process(Path(event.src_path))

obs = Observer()
obs.schedule(InboxHandler(), "/mnt/finance/inbox", recursive=False)
obs.start()
```

Prefer `on_closed` over `on_created` — `on_created` fires the moment the file appears, often before the writer has finished. `on_closed` requires Linux and a recent watchdog version. On Windows or older Linux, fall back to `on_created` + a stability check (poll size for N seconds; only proceed when it stops growing) or use a sentinel file.

### .NET — `FileSystemWatcher`

```csharp
var watcher = new FileSystemWatcher(@"\\fileserver\finance\inbox", "*.csv")
{
    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
    EnableRaisingEvents = true,
};
watcher.Created += async (_, e) => await ProcessWhenStableAsync(e.FullPath);
```

`FileSystemWatcher` is unreliable over SMB/NFS — events can be missed under load. Treat it as a hint, not a contract: combine with a periodic full directory scan to catch missed files.

### The "boring" pattern — periodic poll

Often the right answer for batch workflows on shares:

```python
import time
from pathlib import Path

inbox = Path("/mnt/finance/inbox")
processing = inbox / ".processing"
done = inbox / ".done"
processing.mkdir(exist_ok=True); done.mkdir(exist_ok=True)

while True:
    for f in sorted(inbox.glob("*.csv")):
        target = processing / f.name
        try:
            f.rename(target)        # Atomic claim — only one worker wins
        except FileNotFoundError:
            continue                # Another worker grabbed it
        try:
            handle(target)
            target.rename(done / f.name)
        except Exception:
            target.rename(inbox / (f.name + ".error"))
            raise
    time.sleep(10)
```

This scales to multiple workers because `rename` into `processing/` is atomic — exactly one worker succeeds.

## Encoding & Newlines

- Default to UTF-8. Always specify it explicitly: `encoding="utf-8"` (Python), `Encoding.UTF8` (.NET).
- BOM matters for some downstream tools (Excel especially). Use `utf-8-sig` in Python or `new UTF8Encoding(true)` in .NET when generating files for Excel users.
- Newlines: write `\n` (LF) by default; use `\r\n` only when generating files for legacy Windows-only consumers.

## Cleaning Up

Delete temp files in a `try/finally` or use a context manager that does it for you (`tempfile.TemporaryDirectory()` in Python; track-and-delete in .NET). Orphan `.tmp` files on a share are an operational embarrassment.

## Cross-Skill References

- Pushing files to OCI after picking them up → **oracle-object-storage** skill
- Triggering an ODI scenario when a file lands → **odi-automation** skill
- Authenticating to OCI File Storage / Object Storage → **oci-apis** skill
- Filesystem watcher hosted in a .NET worker → **dotnet-development** skill
- Filesystem watcher hosted in a Python service → **python-development** skill

## Common Pitfalls

1. **Reading a file the moment it's created.** The writer probably isn't done. Use `on_closed`, sentinel files, or a stability check.
2. **Cross-volume `Move` assumed atomic.** It's not. Write to a temp file *on the destination volume*, then rename.
3. **Drive letters in Windows services.** Per-session, often invisible. Use UNC paths.
4. **Hardcoded `/` or `\` in paths.** Use `Path` / `pathlib`.
5. **Forgetting `os.fsync` / `FlushAsync` before rename.** The rename is durable but the data may not be — a crash can leave you with an empty file at the target name.
6. **Polling a network share every 100 ms.** The share doesn't appreciate it, and neither does the network. Poll seconds, not milliseconds.
7. **Walking a directory that's actively being written.** Snapshot the list, then iterate.
8. **Ignoring SELinux / AppArmor / NTFS ACLs** when "it works in dev." Permissions on shares are the #1 production support call.
9. **Soft NFS mounts on data paths.** Silent data corruption when the server hiccups. Use `hard`.
10. **Trusting `FileSystemWatcher` as the only mechanism.** Combine with periodic scans.
