# Memory Bank Index

## Purpose
This folder stores project knowledge for both humans and AI agents.

## Read Order
1. `AGENTS.md`
2. `memory-bank/PROJECT_DOCUMENTATION.md`
3. `memory-bank/10-modules/hq-frontend-backend-boundary.md` (HQ/HQBackSite 前後端與資料邊界)
4. `memory-bank/10-modules/hq-file-analysis.md` (逐檔分析與 ownership map)
5. `memory-bank/10-modules/hq-project-manual.md` (整體說明、流程圖與 ER Model)
6. `memory-bank/10-modules/hq-latest-change-outline.md` (最新版變更大綱)
7. `memory-bank/11-rag/hq-rag-index.md` (RAG 查詢與 chunk 切分)
8. `memory-bank/pr-review.md` (review workflow reference)
9. `memory-bank/20-pr-review-standards.md` (review standards reference)
10. `memory-bank/workflows.md` (general workflow reference)
11. Add module-specific notes under `memory-bank/10-modules/` as needed.

## .NET 10 建置、測試與啟動

- 安裝 .NET 10 SDK，在 repo 根目錄執行 `dotnet build HQ.Modern.slnx -c Release`，建置查詢入口、Word 轉換元件及其測試。
- 執行 `dotnet test tests/HQ.WordHtml.Tests/HQ.WordHtml.Tests.csproj -c Release --no-build`。原後台測試從 Visual Studio 開發人員命令提示字元執行 `MSBuild tests/HQBackSite.Tests/HQBackSite.Tests.csproj /restore /p:RestorePackagesConfig=true /p:Configuration=Debug`，再執行 `vstest.console tests/HQBackSite.Tests/bin/Debug/net472/HQBackSite.Tests.dll`。兩個測試專案共用 Word 案例。
- 執行 `dotnet run --project HQ.Portal/HQ.Portal.csproj -- --urls http://127.0.0.1:5182`。測試 `/Home/Restaurant`、`/Home/FaxView` 及原有 `/Areas/tel/restaurant.htm`、`/Areas/tel/fax.htm`。其餘入口、管理與 API 路由仍由原站提供。
- 執行 `dotnet publish HQ.Portal/HQ.Portal.csproj -c Release` 產出部署檔。餐廳與傳真表沿用員工內網的存取限制，部署時須一併保留傳真表的四個附檔。

## Suggested Next Files
- `memory-bank/03-dev-setup.md`
- `memory-bank/04-coding-standards.md`
- `memory-bank/05-pr-review-checklist.md`
- `memory-bank/08-known-issues.md`
- `memory-bank/12-change-log.md`
- `memory-bank/10-modules/hqbacksite-word-html-test-page.md` (Word 上傳轉靜態 HTML 技術設計)
- `memory-bank/10-modules/hqbacksite-word-html-technical-spec.md` (Word 轉 HTML 現行技術規格書)
- `memory-bank/10-modules/hq-frontend-backend-boundary.md` (HQ/HQBackSite 前後端與資料邊界)
- `memory-bank/10-modules/hq-file-analysis.md` (逐檔分析與 ownership map)
- `memory-bank/10-modules/hq-project-manual.md` (完整專案說明書)
- `memory-bank/10-modules/hq-latest-change-outline.md` (最新版變更大綱)
- `memory-bank/11-rag/hq-rag-index.md` (RAG 查詢與 chunk 切分)
- `memory-bank/11-rag/hq-code-index.jsonl` (RAG-friendly code index)
- `memory-bank/13-skills/skm-hq-master-boundary/SKILL.md` (repo-local Codex skill source)

## Notes
- Prefer documenting current behavior over ideal behavior.
- Record important decisions under `memory-bank/09-decisions/`.

## Imported Cline References
- `memory-bank/cline/pr-review.md` (copied from your Cline Workflows)
- `memory-bank/cline/workflows.md` (copied from your Cline Workflows)
- `memory-bank/cline/20-pr-review-standards.md` (copied from your Cline Rules)
- Keep these as upstream references; put project-specific adaptations in separate `memory-bank/` files.
