# AGENTS.md

## Scope
These instructions apply to this repository.

## Git Commit And Push Workflow

- After implementation and verification, stage only task-related files, create a local commit, and push the current branch to its configured upstream remote without waiting for an extra prompt.
- If no upstream/remote exists, or push fails, report it clearly.
- Never push secrets, ignored runtime files, or unrelated dirty changes.

## Primary Knowledge Sources
Before making changes, reviewing code, or answering project-specific questions, read relevant files in this order:
1. `memory-bank/00-index.md`
2. `memory-bank/PROJECT_DOCUMENTATION.md`
3. `memory-bank/pr-review.md` and `memory-bank/20-pr-review-standards.md` (for reviews)
4. `memory-bank/workflows.md` (for workflow guidance)
5. Related source files for the task

## Working Rules
- Before generating or modifying code, confirm `memory-bank/PROJECT_DOCUMENTATION.md` first.
- Prefer repository facts and project docs over generic assumptions.
- If project information is missing or unclear, state the gap explicitly before continuing.
- Do not modify unrelated files.
- When behavior, routes, data flow, or operations change, update docs in `memory-bank/`.
- Record important architectural or workflow decisions in `memory-bank/09-decisions/`.

## PR Review Expectations
When asked to review code, prioritize:
1. Correctness and regression risk
2. Security / authorization / sensitive data handling
3. Data access / SQL / transaction risks
4. Null handling and edge cases
5. Concurrency / race conditions / caching side effects
6. Test coverage gaps

Review output should list findings first with severity (P0/P1/P2 when applicable), then open questions/assumptions, then a short summary.

## Project Context Notes
- This repository contains `HQ` and `HQBackSite` (ASP.NET MVC / Web API components).
- `memory-bank/PROJECT_DOCUMENTATION.md` currently contains routes, auth flow, logging notes, tests, and DDL references.

## Documentation Maintenance
If you create new repeatable workflows or important debugging notes, add a focused markdown file under `memory-bank/` and reference it from `memory-bank/00-index.md`.
