---
name: oracle-object-storage
description: Use this skill whenever the user is reading from, writing to, or otherwise managing Oracle Object Storage — buckets, objects, namespaces, multipart uploads, pre-authenticated requests (PARs), object lifecycle policies, replication, retention rules, or storage tiers (Standard / Infrequent Access / Archive). Triggers include any mention of "Object Storage", "OCI bucket", "namespace", "PAR", "pre-authenticated request", `os://`, `oci os` CLI commands, `ObjectStorageClient`, multipart upload, large file upload to OCI, S3-compatible API on OCI, or moving files between local/share storage and Oracle Cloud. Use proactively when the user wants to upload/download files at scale, generate temporary URLs for partners, or configure lifecycle/archive policies. Pair with the **oci-apis** skill for auth and signing fundamentals.
---

# Oracle Object Storage

Patterns for using OCI Object Storage as the durable blob layer for an application.

## Mental Model

| Concept | What it is |
|---------|-----------|
| **Namespace** | Tenancy-scoped string (e.g. `axxxxxxxxxxx`). Every object URL contains it. Get it once and cache it. |
| **Bucket** | A container in a single compartment, in a single region. Names are unique per namespace+region. |
| **Object** | Key + bytes + metadata. Keys may contain `/` to simulate folders, but folders are not real. |
| **Storage tier** | `Standard`, `InfrequentAccess`, `Archive`. Set at bucket or object level. |
| **PAR** | Pre-Authenticated Request — a time-limited URL that lets a third party read/write without OCI credentials. |

## Endpoints

Two API surfaces sit in front of the same data:

- **Native API** — `https://objectstorage.<region>.oraclecloud.com` — full feature set; use this from SDKs.
- **S3-compatible API** — `https://<namespace>.compat.objectstorage.<region>.oraclecloud.com` — for tools that already speak S3 (e.g., AWS SDKs, `aws s3`, `s3cmd`, rclone). Subset of features.

Use the **native API + official OCI SDK** unless you have a reason not to.

## Get the Namespace

You will always need it. It's a one-line call.

**Python:**

```python
import oci
config = oci.config.from_file()
os_client = oci.object_storage.ObjectStorageClient(config)
namespace = os_client.get_namespace().data
```

**.NET:**

```csharp
using Oci.Common.Auth;
using Oci.ObjectstorageService;
using Oci.ObjectstorageService.Requests;

var provider = new ConfigFileAuthenticationDetailsProvider("DEFAULT");
using var os = new ObjectStorageClient(provider);
var ns = (await os.GetNamespace(new GetNamespaceRequest())).Value;
```

## Buckets — Create, Configure, Inspect

**Python — create a bucket:**

```python
from oci.object_storage.models import CreateBucketDetails

os_client.create_bucket(
    namespace_name=namespace,
    create_bucket_details=CreateBucketDetails(
        name="myapp-ingest",
        compartment_id=compartment_ocid,
        storage_tier="Standard",
        public_access_type="NoPublicAccess",
        versioning="Enabled",
        object_events_enabled=True,  # Emit events to OCI Events for new/changed objects
    ),
)
```

Defaults you almost always want:

- `public_access_type="NoPublicAccess"` — never expose buckets directly.
- `versioning="Enabled"` — cheap insurance against bad writes/deletes.
- `object_events_enabled=True` — lets you wire up Functions / Notifications on object change.

## Uploading Objects

### Small objects (< ~50 MB) — single PUT

**Python:**

```python
with open(path, "rb") as f:
    os_client.put_object(
        namespace_name=namespace,
        bucket_name="myapp-ingest",
        object_name="incoming/2026/04/file.csv",
        put_object_body=f,
        content_type="text/csv",
        opc_meta={"source": "etl-job-42"},  # Custom metadata becomes opc-meta-source header
    )
```

**.NET:**

```csharp
using Oci.ObjectstorageService.Requests;

await using var fs = File.OpenRead(path);
await os.PutObject(new PutObjectRequest
{
    NamespaceName = ns,
    BucketName = "myapp-ingest",
    ObjectName = "incoming/2026/04/file.csv",
    PutObjectBody = fs,
    ContentType = "text/csv",
});
```

### Large objects — multipart upload

**Always** use multipart for files larger than ~100 MB. It enables parallelism and resumability.

The Python SDK has an `UploadManager` that handles split + parallel + retry for you:

```python
from oci.object_storage import UploadManager
from oci.object_storage.transfer.constants import MEBIBYTE

upload_manager = UploadManager(os_client, allow_parallel_uploads=True, parallel_process_count=8)
upload_manager.upload_file(
    namespace_name=namespace,
    bucket_name="myapp-ingest",
    object_name="bulk/big.parquet",
    file_path="/data/big.parquet",
    part_size=64 * MEBIBYTE,
)
```

The .NET SDK exposes `UploadManager` similarly:

```csharp
using Oci.ObjectstorageService.Transfer;
using Oci.ObjectstorageService.Transfer.Requests;

var manager = new UploadManager(os, new UploadConfiguration
{
    AllowMultipartUploads = true,
    AllowParallelUploads = true,
});

var req = new UploadManager.UploadRequest
{
    NamespaceName = ns,
    BucketName = "myapp-ingest",
    ObjectName = "bulk/big.parquet",
    ContentType = "application/octet-stream",
};
await manager.UploadFile(req, "/data/big.parquet");
```

If you implement multipart manually:

1. `CreateMultipartUpload` → returns an `uploadId`.
2. `UploadPart` for each part (parts numbered 1..10000, each ≥ 5 MiB except the last).
3. `CommitMultipartUpload` with the list of part numbers + ETags.
4. On failure, `AbortMultipartUpload` — or buckets accumulate orphan parts you'll pay for.

A bucket lifecycle rule should auto-abort multipart uploads older than N days as a safety net.

## Downloading Objects

**Python — stream to file:**

```python
resp = os_client.get_object(namespace, "myapp-ingest", "incoming/2026/04/file.csv")
with open("/tmp/file.csv", "wb") as f:
    for chunk in resp.data.raw.stream(1024 * 1024, decode_content=False):
        f.write(chunk)
```

**.NET:**

```csharp
var resp = await os.GetObject(new GetObjectRequest
{
    NamespaceName = ns,
    BucketName = "myapp-ingest",
    ObjectName = "incoming/2026/04/file.csv",
});
await using var dest = File.Create("/tmp/file.csv");
await resp.InputStream.CopyToAsync(dest);
```

## Listing Objects

Always paginate. Use prefix to scope:

```python
import oci

pages = oci.pagination.list_call_get_all_results(
    os_client.list_objects,
    namespace_name=namespace,
    bucket_name="myapp-ingest",
    prefix="incoming/2026/04/",
    fields="name,size,timeModified,etag",
)
for obj in pages.data.objects:
    print(obj.name, obj.size, obj.time_modified)
```

Object listing is lexicographic by key. Designing keys with date prefixes (`incoming/YYYY/MM/DD/...`) makes prefix scans cheap and lifecycle rules easy.

## Pre-Authenticated Requests (PARs)

A PAR is a signed URL — the only way to give an external party access without giving them OCI credentials. PARs are scoped to:

- A specific object **or** a prefix.
- A specific permission: `ObjectRead`, `ObjectWrite`, `ObjectReadWrite`, `AnyObjectWrite`, etc.
- An expiration time.

**Python — create an upload PAR good for 24 hours:**

```python
from datetime import datetime, timedelta, timezone
from oci.object_storage.models import CreatePreauthenticatedRequestDetails

details = CreatePreauthenticatedRequestDetails(
    name="partner-upload-2026-04-24",
    access_type="AnyObjectWrite",   # Lets PAR holder PUT any object under bucket_listing_action_allowed prefix
    bucket_listing_action="ListObjects",
    object_name="dropbox/partner-x/",  # Prefix-style for AnyObjectWrite
    time_expires=datetime.now(timezone.utc) + timedelta(days=1),
)
par = os_client.create_preauthenticated_request(namespace, "myapp-ingest", details).data
print(par.full_path)  # The URL to give the partner
```

Important PAR rules:

- Once created, **the URL cannot be retrieved again** — capture `full_path` immediately.
- Deleting the PAR revokes it.
- PARs do not bypass policies — they bypass the *requester's* identity, but the bucket policy still applies.

## Server-Side Encryption

Object Storage encrypts every object at rest by default. By default, OCI manages keys (Oracle-managed). To use customer-managed keys (Vault), set `kms_key_id` on the bucket. To use *customer-supplied* keys (rare), provide them per-request via `opc-sse-customer-*` headers.

For most apps, the right answer is: enable a Vault key on the bucket, and don't think about it again.

## Lifecycle Policies

Configure object aging at the bucket level — don't write a cron job to do this.

```python
from oci.object_storage.models import (
    PutObjectLifecyclePolicyDetails, ObjectLifecycleRule, ObjectNameFilter,
)

rules = [
    ObjectLifecycleRule(
        name="archive-after-90d",
        action="ARCHIVE",
        time_amount=90,
        time_unit="DAYS",
        is_enabled=True,
        object_name_filter=ObjectNameFilter(inclusion_prefixes=["incoming/"]),
    ),
    ObjectLifecycleRule(
        name="abort-multipart-after-7d",
        action="ABORT",
        time_amount=7,
        time_unit="DAYS",
        is_enabled=True,
        target="multipart-uploads",
    ),
]
os_client.put_object_lifecycle_policy(
    namespace, "myapp-ingest", PutObjectLifecyclePolicyDetails(items=rules)
)
```

Always include the multipart-abort rule — it's the standard hedge against orphan parts.

## Archive Tier — Don't Forget the Restore Step

Objects in `Archive` storage tier are not directly readable. To read one:

1. Call `RestoreObjects` (specify `hours=24` for time the restored copy stays warm).
2. Wait — typically up to 1 hour.
3. Read normally during the restore window.

Plan workflows around this latency. Archive is for cold data, not anything in the critical path.

## CLI Cheat Sheet

```bash
# Get namespace
oci os ns get

# Upload one file
oci os object put -bn myapp-ingest --file ./file.csv --name incoming/file.csv

# Bulk upload a directory (parallel + multipart automatic)
oci os object bulk-upload -bn myapp-ingest --src-dir ./out --object-prefix incoming/

# Bulk download
oci os object bulk-download -bn myapp-ingest --download-dir ./local --prefix incoming/2026/

# List with prefix
oci os object list -bn myapp-ingest --prefix incoming/2026/04/ --all

# Create a 1-hour read PAR for a single object
oci os preauth-request create \
  -bn myapp-ingest --name share-once \
  --access-type ObjectRead --object-name reports/q1.pdf \
  --time-expires "$(date -u -d '+1 hour' +%Y-%m-%dT%H:%M:%SZ)"
```

## Cross-Skill References

- Authentication, signing, region/endpoint detail → **oci-apis** skill
- Calling Object Storage from Python — `httpx` or SDK → **python-development** skill
- Calling Object Storage from .NET → **dotnet-development** skill
- Pulling files from a file share before upload, atomic local moves → **file-operations** skill
- Downstream: kicking off ODI scenarios after uploads land → **odi-automation** skill

## Common Pitfalls

1. **Hardcoding the namespace** — call `GetNamespace` once and cache. It differs by tenancy.
2. **Single-PUT for multi-GB files** — slow, fragile, and times out. Always use the upload manager.
3. **Forgetting to abort multipart on failure** — you pay for orphan parts forever. Use a lifecycle rule.
4. **Treating folders as real** — `incoming/2026/04/` is just a key prefix. There's no `mkdir`.
5. **Storing PAR URLs in logs** — they're credentials. Treat them like passwords.
6. **Reading from Archive synchronously** — you must call `RestoreObjects` first.
7. **Putting buckets in the wrong compartment** — buckets cannot be moved between compartments without recreate-and-copy.
8. **Not enabling versioning** — one bad job overwrites months of data with no recovery.
