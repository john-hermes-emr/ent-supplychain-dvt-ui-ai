# AI Skills — App Development Project

A set of focused skill files designed for Claude to consume when working on this project. Each skill follows the standard format: YAML frontmatter (`name` + `description` for triggering) followed by markdown instructions.

## The Skills

| Skill | Folder | Triggers on |
|-------|--------|-------------|
| .NET development | `dotnet-development/` | C#, .NET, dotnet, csproj, NuGet, ASP.NET, Worker Services |
| Python development | `python-development/` | Python, pip, poetry, pyproject.toml, venv, FastAPI, Typer, pydantic |
| OCI APIs | `oci-apis/` | OCI, Oracle Cloud, OCID, tenancy, compartment, instance/resource principal |
| Oracle Object Storage | `oracle-object-storage/` | Object Storage, bucket, namespace, PAR, multipart upload |
| ODI automation | `odi-automation/` | ODI, Oracle Data Integrator, scenario, load plan, ODI Agent |
| File operations | `file-operations/` | File share, SMB, NFS, UNC, mount, atomic move, drop folder |

## How They Compose

The skills are deliberately split by concern but reference each other where workflows cross boundaries. A typical end-to-end flow uses several at once:

```
[ file lands on share ]                   ← file-operations
        ↓
[ .NET or Python worker picks it up ]     ← dotnet-development / python-development
        ↓
[ uploaded to Object Storage ]            ← oracle-object-storage (auth: oci-apis)
        ↓
[ ODI scenario triggered + polled ]       ← odi-automation
        ↓
[ result file written, source archived ]  ← file-operations
```

Cross-references inside each SKILL.md point at the related skills so Claude follows the chain when a task spans concerns.

## Design Choices

These skill files follow Anthropic's skill-authoring conventions:

- **Triggering lives in the description.** Each `description` is verbose and specific — listing concrete keywords, file extensions, and library names — because that's how Claude decides whether to load the skill.
- **Imperative tone in the body.** Instructions read as "do this," "use that," with explanations of *why* rather than soft hedging.
- **Quick-reference tables up front.** Decision tables let Claude land on the right pattern without reading the whole file.
- **Code examples in both languages where relevant.** OCI, Object Storage, and ODI sections show parallel Python and .NET so Claude can pick the matching one.
- **Explicit pitfalls section at the end of each skill.** The mistakes listed are the ones most likely to bite in this technology stack — they're worth a dedicated section because Claude tends to skim error-handling otherwise.
- **Each file is self-contained but cross-linked.** Claude can load one skill and still know which neighbour to consult.

## Installing as Claude Skills

Each folder is a complete skill — drop the folder into a skills directory that Claude has access to, and the description will trigger loading on relevant prompts. Sizes are well under the recommended 500-line ceiling, so they sit comfortably in context when triggered.

## Iteration

These are starting drafts. Refine by:

1. Running real prompts against Claude with the skills loaded.
2. Noting where Claude misses, over-triggers, or produces wrong patterns.
3. Tightening the `description` (for triggering) or the body (for output quality) accordingly.

The pitfalls sections in particular should grow from real bugs hit during the project.
