# Decision: HQ Boundary, RAG, and Skill Docs

Date: 2026-09-03

## Context

The workspace contains two ASP.NET MVC sites plus a test project:

- `HQ/`: employee portal, old Backside, and Web API.
- `HQBackSite/`: newer back-office MVC site.
- `tests/HQBackSite.Tests/`: net472 MSTest project.

The user asked for backend/frontend separation, file-by-file analysis, overall specs, and corresponding skills/RAG artifacts.

## Decision

Create a documentation and AI-context set under `memory-bank/`:

- `memory-bank/10-modules/hq-frontend-backend-boundary.md`
- `memory-bank/10-modules/hq-file-analysis.md`
- `memory-bank/11-rag/hq-rag-index.md`
- `memory-bank/11-rag/hq-code-index.jsonl`
- `memory-bank/13-skills/skm-hq-master-boundary/SKILL.md`

Also install the same skill to the local Codex skills directory:

- `C:\Users\Zack Ou\.codex\skills\skm-hq-master-boundary\SKILL.md`

## Rationale

The project is not a clean frontend/backend split. `HQ` mixes Razor pages, old back-office pages, and Web API controllers; `HQBackSite` is a separate MVC back-office site. RAG chunks should therefore be cut by feature ownership and data flow instead of by repository folder alone.

First-party business code is analyzed one file at a time. Third-party/scaffold/generated areas are explicitly classified as vendor/generated to keep future code changes and retrieval scoped.

## Consequences

- Future agents should start from `memory-bank/00-index.md`, then use the boundary and file-analysis docs before changing code.
- RAG pipelines can use `memory-bank/11-rag/hq-code-index.jsonl` as seed metadata.
- The local skill routes future work to more specific HQ skills such as auth/cookie/JWT, SQL/para-table, Menu CRUD, Word HTML, and net472 test runner.
- Secrets must remain excluded from docs and embeddings.
