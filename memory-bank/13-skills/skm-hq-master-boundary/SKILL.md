---
name: skm-hq-master-boundary
description: Use when working in the SKM HQ master workspace to classify HQ vs HQBackSite frontend/backend ownership, choose the right local docs/RAG chunks, and route to the existing HQ skills.
metadata:
  short-description: SKM HQ repo boundary and RAG router
---

# SKM HQ Master Boundary

Use this skill for work inside:

```text
D:\work_space\新光三越\20260203 新版HQ 接手剩下頁面\master
```

It helps decide whether a request belongs to `HQ`, `HQBackSite`, tests, docs/RAG, or vendor/generated assets before making changes.

## Required Context

Before changing code in this workspace, read:

1. `AGENTS.md`
2. `memory-bank/00-index.md`
3. `memory-bank/10-modules/hq-frontend-backend-boundary.md`
4. `memory-bank/10-modules/hq-file-analysis.md`

For retrieval-heavy questions, also read:

- `memory-bank/11-rag/hq-rag-index.md`
- `memory-bank/11-rag/hq-code-index.jsonl`

## Boundary Rules

- `HQ/` is a mixed legacy site: employee portal frontend, old Backside backend UI, and Web API backend.
- `HQBackSite/` is the newer back-office MVC site.
- `tests/HQBackSite.Tests/` covers HQBackSite regression tests, especially MenuController, ICS, Content, and Word HTML.
- `HQ/Areas/HelpPage`, `HQ/ueditor`, Bootstrap, jQuery, Modernizr, generated HTML, userfiles, images, and build outputs are vendor/generated unless the user explicitly targets them.
- Do not copy connection strings, passwords, JWT signing keys, cookie values, or tokens into responses or generated documentation.

## Routing

Use the more specific local skills when the request touches these areas:

- `hq-auth-and-cookie-jwt`: HQBackSite login, `Authorization` cookie, JWT, unauthorized handling.
- `hq-sql-and-para-table`: `para`, `content`, `unit`, `news`, `users`, permissions, inline SQL, or data-dependent route behavior.
- `hqbacksite-menu-crud`: `HQBackSite/Controllers/MenuController.cs`, `Views/Menu`, related models, CRUD validation, permission mapping.
- `hqbacksite-word-html`: `WordHtmlTest`, `WordHtmlConvert`, `WordHtmlPreview`, `WordHtmlConverter`, generated HTML, docx conversion tests.
- `hq-test-runner-net472`: building or running `tests/HQBackSite.Tests`.
- `aspnet-mvc-legacy-maintenance`: IIS Express, Global.asax, RouteConfig, legacy MVC startup or route issues.

## Work Pattern

1. Confirm the current directory and git status.
2. Classify the request using the boundary docs and RAG index.
3. Read the smallest relevant code set: frontend caller, backend action, data model/table, and tests.
4. Keep changes scoped to the classified area.
5. Update `memory-bank` docs/RAG/skill source when behavior or boundaries change.
6. Run the narrowest meaningful verification, then report any uncovered gaps.
