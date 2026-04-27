---
name: oci-apis
description: Use this skill whenever the user is working with Oracle Cloud Infrastructure (OCI) APIs, SDKs, or the OCI CLI — authentication, request signing, OCIDs, compartments, regions, tenancy, and the catalog of OCI services (Identity/IAM, Compute, Networking, Vault, Resource Manager, Logging, Monitoring, Functions, Streaming, Events, Notifications, etc.). Triggers include any mention of "OCI", "Oracle Cloud", "OCID", "tenancy", "compartment", `~/.oci/config`, `oci-cli`, instance principal, resource principal, security token, API key, request signing, or any OCI service by name. Use proactively when the user wants to call OCI from .NET or Python, or when troubleshooting `NotAuthenticated` / `NotAuthorized` / signature errors. For Object-Storage-specific work (buckets, objects, multipart, PARs), pair this with the **oracle-object-storage** skill.
---

# Oracle Cloud Infrastructure (OCI) APIs

How to authenticate, sign, and call OCI from code. The signing and auth model is the part that trips people up — get it right once and the rest is service-specific.

## Mental Model

OCI is built around four concepts:

1. **Tenancy** — the root account. One per organization. Identified by an OCID.
2. **Compartment** — a logical container for resources, used for access control and billing. Every resource lives in exactly one compartment. Compartments nest.
3. **Region / Availability Domain** — geographic placement. Region codes look like `us-ashburn-1`, `uk-london-1`, `eu-frankfurt-1`.
4. **OCID** — globally unique resource ID. Format: `ocid1.<resource-type>.<realm>.<region-or-empty>.<unique-id>`.

Everything you do is "in a compartment, in a region, as a principal."

## Authentication: Pick the Right Principal

OCI supports several **authentication principals**. Pick by *where the code runs*:

| Principal | When to use | How it works |
|-----------|-------------|--------------|
| **API Key (user)** | Local dev, scripts on laptops | RSA key pair in `~/.oci/config` |
| **Instance Principal** | Code on an OCI Compute VM | VM gets identity from a Dynamic Group + Policy |
| **Resource Principal** | OCI Functions, Data Science notebooks, ODA, etc. | Service injects identity at runtime |
| **Workload Identity** | OKE (Kubernetes) pods | Per-pod identity via service account |
| **Security Token (session)** | Federated users, short-lived MFA | `oci session authenticate` |

Rule of thumb: **never bake API keys into a server-side workload**. If it's running on OCI, use instance/resource principal.

### `~/.oci/config` (API Key)

```ini
[DEFAULT]
user=ocid1.user.oc1..aaaa...
fingerprint=aa:bb:cc:dd:...
key_file=~/.oci/oci_api_key.pem
tenancy=ocid1.tenancy.oc1..aaaa...
region=us-ashburn-1
```

Permissions on the key file matter. On Linux/macOS:

```bash
chmod 600 ~/.oci/oci_api_key.pem
oci setup repair-file-permissions --file ~/.oci/config
```

### Instance Principal (Python)

```python
import oci
signer = oci.auth.signers.InstancePrincipalsSecurityTokenSigner()
identity = oci.identity.IdentityClient(config={}, signer=signer)
```

### Instance Principal (.NET)

```csharp
using Oci.Common.Auth;
using Oci.IdentityService;

var provider = new InstancePrincipalsAuthenticationDetailsProvider();
using var client = new IdentityClient(provider);
```

### Resource Principal (works in Functions, Data Science, etc.)

Python:

```python
signer = oci.auth.signers.get_resource_principals_signer()
```

.NET:

```csharp
var provider = ResourcePrincipalAuthenticationDetailsProvider.GetProvider();
```

## Request Signing

Every OCI REST request must be signed with HTTP Signatures (RFC 9421-style, OCI-flavored). **Do not implement this yourself** — every official SDK handles it. If you must call OCI without an SDK (e.g., from a tool that only exposes raw HTTP), use Oracle's signing reference implementations or a small wrapper.

What gets signed (typical):
- `(request-target)`, `host`, `date`, `x-content-sha256` (for bodies), `content-length`, `content-type`
- The signing key is your RSA private key; the `keyId` is `<tenancy>/<user>/<fingerprint>`.

If you see `NotAuthenticated` errors, the suspects in order are: clock skew (`date` header > 5 min off), wrong fingerprint, missing `x-content-sha256` on a PUT/POST, or signing the wrong canonical headers.

## SDKs

| Language | Package | Notes |
|----------|---------|-------|
| Python | `oci` | Most mature; mirrors REST 1:1. Used in `python-development` skill. |
| .NET | `OCI.DotNetSDK.<Service>` (e.g. `OCI.DotNetSDK.Objectstorage`) | Per-service NuGets. |
| Java | `com.oracle.oci.sdk:oci-java-sdk-*` | Standard for ODI/Java work. |
| Go | `github.com/oracle/oci-go-sdk/v65` | |
| TypeScript | `oci-sdk` (oci-typescript-sdk) | |
| CLI | `oci` (Python under the hood) | Great for ad-hoc + CI. |

## Calling a Service: The Common Shape

Every SDK call has the same skeleton:

1. Build an auth provider / signer.
2. Construct a service client (region usually inferred from config or set explicitly).
3. Build a request object with required + optional fields.
4. Call the operation, handle pagination.

**Python — list compartments:**

```python
import oci
config = oci.config.from_file(profile_name="DEFAULT")
identity = oci.identity.IdentityClient(config)

resp = oci.pagination.list_call_get_all_results(
    identity.list_compartments,
    compartment_id=config["tenancy"],
    compartment_id_in_subtree=True,
)
for c in resp.data:
    print(c.id, c.name)
```

**.NET — list compartments:**

```csharp
using Oci.Common.Auth;
using Oci.IdentityService;
using Oci.IdentityService.Requests;

var provider = new ConfigFileAuthenticationDetailsProvider("DEFAULT");
using var client = new IdentityClient(provider);

var req = new ListCompartmentsRequest
{
    CompartmentId = provider.TenantId,
    CompartmentIdInSubtree = true,
};
var resp = await client.ListCompartments(req);
foreach (var c in resp.Items) Console.WriteLine($"{c.Id} {c.Name}");
```

## Pagination

Every "list" operation paginates. Two forms:

- **Use the helper** (Python: `oci.pagination.list_call_get_all_results`; .NET: `client.NewListCompartmentsPaginator(...)`).
- **Manual**: pass the `OpcNextPage` from one response as the `Page` parameter on the next request, until `OpcNextPage` is null.

Don't assume one page is enough — even small tenancies can paginate.

## Retries & Rate Limiting

OCI throttles per-service, per-tenancy. Treat `429` as expected and retry with backoff. Both SDKs ship with retry strategies:

**Python:**

```python
from oci.retry import DEFAULT_RETRY_STRATEGY
client.list_compartments(..., retry_strategy=DEFAULT_RETRY_STRATEGY)
```

**.NET:**

```csharp
var req = new ListCompartmentsRequest { /* ... */ };
var resp = await client.ListCompartments(req,
    retryConfiguration: new RetryConfiguration { MaxAttempts = 5 });
```

For raw HTTP, retry on `429`, `500`, `502`, `503`, `504` with exponential backoff and jitter. Don't retry `400`, `401`, `403`, `404`, `409` — those mean *your* request is wrong.

## Regions and Endpoints

Endpoints follow `https://<service>.<region>.oci.<realm>.com`. Examples:

- `https://identity.us-ashburn-1.oci.oraclecloud.com`
- `https://objectstorage.eu-frankfurt-1.oraclecloud.com`

The realm differs for government/sovereign regions (`oraclegovcloud.com`, etc.). SDKs handle this if you set `region` correctly in config.

## Common Services & Their Use Cases

| Service | What it's for |
|---------|---------------|
| Identity (IAM) | Users, groups, dynamic groups, policies, compartments |
| Object Storage | Bulk file/blob storage — see **oracle-object-storage** skill |
| Vault | Secrets, encryption keys |
| Resource Manager | Terraform-as-a-service for OCI |
| Logging | Centralized log ingestion (custom + service logs) |
| Monitoring | Metrics, alarms |
| Notifications (ONS) | Pub/sub topics, email/SMS/Slack/PagerDuty |
| Events | React to resource state changes (e.g., "object created") |
| Functions | Serverless (FaaS) — Fn-based |
| Streaming | Kafka-compatible event stream |
| File Storage (FSS) | Managed NFS — see **file-operations** skill |
| Database | ADB, Base DB, Exadata; Oracle DB connection from app code |

## Policies (Authorization)

After auth comes authz. Policies are written in a domain-specific language at the tenancy or compartment level:

```
Allow group MyAppDevs to manage object-family in compartment AppData
Allow dynamic-group MyAppInstances to read secret-bundle in compartment AppData
```

If a call returns `NotAuthorizedOrNotFound`, OCI is being deliberately vague to avoid leaking resource existence. Check both: does the resource exist *and* does the principal have policy granting the verb on the resource type in that compartment?

## Troubleshooting Decision Tree

| Error | Most likely cause |
|-------|------------------|
| `NotAuthenticated` | Clock skew, wrong fingerprint/key, malformed signature |
| `NotAuthorizedOrNotFound` | Missing policy grant *or* wrong compartment |
| `LimitExceeded` | Tenancy/service quota — request a service-limit increase |
| `TooManyRequests` (429) | Throttling — back off and retry |
| `InvalidParameter` | Required field missing, OCID format wrong |
| `Conflict` (409) | Resource already exists / state transition not allowed |
| Connection refused / TLS | Wrong region in endpoint, or VCN/security-list rules |

## Cross-Skill References

- Object Storage operations → **oracle-object-storage** skill
- ODI execution and APIs → **odi-automation** skill
- Calling OCI from .NET → **dotnet-development** skill
- Calling OCI from Python → **python-development** skill
- File Storage Service mounts → **file-operations** skill

## Common Pitfalls

1. **Embedding API keys in containers** instead of using instance/resource principal.
2. **Calling production from a laptop with a stale config** — add the region to your config and your client config explicitly; don't rely on environment defaults silently flipping.
3. **Not paginating** — first page returns 100 items, you act like that's everything.
4. **Catching all exceptions and retrying 4xx errors** — wastes time, masks bugs. Retry only transient classes.
5. **Hardcoding endpoint URLs** — let the SDK resolve from `region`. New realms break hardcoded URLs.
6. **Confusing "compartment" with "tenancy"** in OCID parameters. They're both compartments structurally (tenancy is the root), but most APIs want a *non-root* compartment OCID.
