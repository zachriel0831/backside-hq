# HQ / HQBackSite 前後端分界規格

Status: 依據 `D:\work_space\新光三越\20260203 新版HQ 接手剩下頁面\master` 目前程式碼整理，日期為 2026-09-03。

## 目的

這份文件定義新版 HQ 接手專案中，哪些地方屬於前端、後端、資料存取、舊後台、新後台，以及 AI/RAG 後續查找時應如何切分上下文。

本文件描述現況，不描述理想架構。若實作與既有文件衝突，以目前程式碼為準；若程式碼與需求文件衝突，先回報差異再改。

## 系統總覽

| 區塊 | 專案/路徑 | 本機 URL | Framework | 角色 |
| --- | --- | --- | --- | --- |
| HQ | `HQ/` | `http://localhost:14555/` | .NET Framework 4.6.1 | 員工入口前台、舊 Backside 後台、Web API |
| HQBackSite | `HQBackSite/` | `http://localhost:57568/` | .NET Framework 4.7.2 | 新版後台 MVC 管理站 |
| Tests | `tests/HQBackSite.Tests/` | N/A | net472 | HQBackSite 單元測試與回歸測試 |
| Docs | `memory-bank/` | N/A | Markdown | 專案規格、RAG 索引、決策紀錄 |

解法不是標準 SPA + API 分離，而是兩個 legacy ASP.NET MVC 站台共用資料庫與靜態資產。`HQ` 裡同時存在前台 Razor、舊後台 Razor、以及 Web API；`HQBackSite` 則是新版後台，以 Razor 頁面搭配 jQuery/fetch 呼叫同站 MVC action。

## 前端分界

### HQ 前台

HQ 前台的主要入口在：

- `HQ/Views/HQ/Index.cshtml`
- `HQ/Views/HQ/Dept.cshtml`
- `HQ/Views/Home/*.cshtml`
- `HQ/Views/Shared/_Layout.cshtml`
- `HQ/Scripts/Hq/*.js`
- `HQ/Scripts/Home/mm_menu.js`
- `HQ/Scripts/util.js`
- `HQ/Content/css.css`
- `HQ/Content/Site.css`

這一層負責畫面 DOM、按鈕互動、頁籤、跑馬燈、電話搜尋、首頁區塊、部門頁區塊與表格組裝。資料主要透過 `g_cus.CommonConnect` 呼叫 `HQ/Controllers/api/*Controller.cs`。

### HQ 舊 Backside 後台

舊後台的前端在：

- `HQ/Views/Backside/*.cshtml`
- `HQ/Views/Shared/_Backside.cshtml`
- `HQ/Views/Shared/_Login.cshtml`
- `HQ/Scripts/Backside/*.js`
- `HQ/Scripts/Common/Backside_lbtn.js`

這一層用傳統 Razor + jQuery + UEditor 動態產生 CRUD 表格與表單。登入由 `HQ/Controllers/BacksideController.cs` 處理，資料操作仍呼叫 `HQ/Controllers/api/Page|Unit|Content|NewsController.cs`。

### HQBackSite 新版後台

新版後台的前端在：

- `HQBackSite/Views/Account/*.cshtml`
- `HQBackSite/Views/Home/*.cshtml`
- `HQBackSite/Views/Menu/*.cshtml`
- `HQBackSite/Views/Shared/*.cshtml`
- `HQBackSite/Scripts/lib.js`
- `HQBackSite/Scripts/login.js`
- `HQBackSite/Scripts/menu.js`
- `HQBackSite/Scripts/content.js`
- `HQBackSite/Scripts/lib.ckeditor.js`
- `HQBackSite/Content/Site.css`

這一層以 server-rendered Razor 為主，清單與表單常透過 modal 載入 partial view。寫入 action 多回傳統一 JSON 結構，`lib.js` 處理 modal、AJAX submit、loading、API helper、結果提示與 reload。

## 後端分界

### HQ MVC Controller

`HQ/Controllers/HomeController.cs`、`HQ/Controllers/HqController.cs`、`HQ/Controllers/BacksideController.cs` 是 MVC controller。

- `HomeController` 舊靜態或 Excel 匯出頁入口，例如電話/系統/餐廳頁。
- `HqController` 員工入口首頁與部門頁入口。
- `BacksideController` 舊後台登入、登出與頁面入口。

### HQ Web API Controller

`HQ/Controllers/api/*.cs` 是 Web API 後端，路由由 `HQ/App_Start/WebApiConfig.cs` 定義：

```text
api/{controller}/{action}/{id}
```

主要回傳 `DataSet` 或 `int`。這些 API 多數沒有額外授權 attribute，前台與舊後台都會直接呼叫。

主要 API 分工：

| Controller | 主要用途 |
| --- | --- |
| `MainController` | 首頁資料、HR/IT 公文、banner、外部連結、資安/人資公告、瀏覽次數、HTML/PDF 連結擷取、清快取 |
| `DeptController` | 部門/分店頁籤、頁面內容、分店公文/新訊、Newsletter 與舊 webmax6 資料合併 |
| `ValuesController` | 電話簿與員工/部門電話搜尋 |
| `PageController` | 舊後台頁面設定 CRUD |
| `UnitController` | 舊後台版位/單元設定 CRUD |
| `ContentController` | 舊後台內容設定 CRUD |
| `NewsController` | 舊後台訊息/資安/人資公告 CRUD |
| `RoomController` | 教育訓練或會議室相關清單與使用者資訊 CRUD |
| `IcsController` | 國際認證規範公告前台查詢 |
| `MarqueeController` | 首頁跑馬燈查詢 |

### HQBackSite MVC Controller

`HQBackSite/Controllers/*.cs` 是新版後台後端，路由由 `HQBackSite/App_Start/RouteConfig.cs` 定義：

```text
{controller}/{action}/{id}
```

主要分工：

| Controller | 主要用途 |
| --- | --- |
| `AccountController` | 登入、登出、JWT cookie 發放、local debug login |
| `HomeController` | 新後台首頁與錯誤頁 |
| `MenuController` | 選單設定、權限設定、類別、內容、訊息、ICS、Word 轉 HTML、上傳 |
| `BaseController` | 統一 JSON、Dapper DB helper、使用者與權限 helper |

`MenuController` 是新版後台的核心，包含多個功能群：

- 選單設定：`Index*`
- 選單權限：`Permission*`
- 選單類別：`Category*`
- 內容設定：`Content*`
- 訊息設定：`Message*`
- 國際認證規範公告：`ICS*`
- Word 轉 HTML：`WordHtml*`
- 共用上傳：`UploadFileForUrl`
- Layout child action：`GetUserInfo`、`GetMenu`
- 前端權限 API：`GetUserMenuPermissions`

## 資料存取分界

目前沒有 repository/service 分層。

`HQ` 使用 `System.Data.SqlClient`、`SqlCommand`、`SqlDataAdapter`、`DataSet`，SQL 多寫在 controller action 內。

`HQBackSite` 使用 `Dapper`，集中透過 `BaseController.Execute`、`Query`、`QuerySingle` 讀取 `Web.config` connection string。

常見資料表與用途：

| 表/資料源 | 主要用途 | 主要程式 |
| --- | --- | --- |
| `para` | 部門/分店網頁、版型、分類參數 | `UnitController`、`MenuController`、`DeptController` |
| `content` | 頁面區塊、上方分類、內容設定 | `ContentController`、`DeptController`、`MenuController` |
| `news` | 首頁訊息、資安、人資、banner、ICS | `NewsController`、`MainController`、`IcsController`、`MenuController` |
| `[count]` | 頁面瀏覽計數 | `MainController.Count*` |
| `otb_edu_userinfo` | Room/教育訓練使用者資訊 | `RoomController` |
| `StoreTel`、`emp_tel`、`users` | 電話與員工資料 | `ValuesController`、`AccountController`、`BackSiteAuthorizeAttribute` |
| `para_permission` | 新後台內容管理部門權限 | `MenuController`、`BaseController` |
| `t_menu_resource`、`t_user_menu_permission` | 新後台左側選單權限 | `MenuController.GetUserMenuPermissions` |
| `ics_list`、`ics_upload` | ICS 文件明細與附件 | `MenuController`、`IcsController` |
| `CAMPAIGN_CATEGORY`、`CAMPAIGN_CONTENT` | 新 Newsletter 公文/通訊資料 | `MainController`、`DeptController` |
| linked `webmax6` | 舊公文/分店資料 | `MainController`、`DeptController` |

## 設定與執行邊界

### IIS Express

本機站台設定在 `.vs/HQ/config/applicationhost.config`：

- `HQ`: port `14555`
- `HQBackSite`: port `57568`

啟動服務前要確認 physical path 指向此 repo 的 `HQ/` 與 `HQBackSite/`，避免誤啟舊資料夾。

### Web.config

兩個站台都在 `Web.config` 定義 connection string 名稱：

- `TestSqlServer`
- `SqlServer`
- `EpSqlServer`
- `NewsLetter`

文件或討論只應記錄 connection string 名稱與用途，不應複製密碼、token、JWT signing key 或完整連線字串。

重要 appSetting：

| 專案 | Key | 用途 |
| --- | --- | --- |
| `HQ` | `TestingMode` | 影響 Newsletter Preview/正式 URL 組法 |
| `HQBackSite` | `EnableLocalLoginDebug` | 本機登入 debug 回傳較多資訊 |
| `HQBackSite` | `LocalBootstrapAccounts` | 本機 bootstrap 帳號白名單 |

## 驗證與測試分界

標準測試專案是 `tests/HQBackSite.Tests/HQBackSite.Tests.csproj`，主要覆蓋：

- `WordHtmlConverterTests`
- `MenuControllerWordHtmlTests`
- `MenuControllerIcsTests`
- `MenuControllerContentTests`

與 HQ 前台 Web API 相關的測試目前不完整。若修改 `HQ/Controllers/api` 或前台 JS，應優先補窄範圍回歸測試，或至少做本機 IIS Express smoke test。

## 安全與操作準則

- 不要在回覆、文件或 log 摘錄中貼出 connection string、DB password、JWT signing key、cookie 值。
- `HQ/Controllers/api/MainController.ReleaseAllCache` 是 GET/POST 皆可觸發的快取清除端點；除非任務明確需要，不要呼叫。
- `HQBackSite` 受 `BackSiteAuthorizeAttribute` 保護，依 `Authorization` cookie JWT 與 `EpSqlServer.dbo.users` 判斷登入。
- `HQ` 舊 Backside 使用 Forms Authentication 與 `Users` 表；這與 `HQBackSite` 的 JWT cookie 不同。
- `HQ` Web API 大多直接暴露資料操作，修改前要查呼叫端 JS 與資料表影響。
- `ValuesController.SqlString` 含動態 SQL 字串組裝，修改電話搜尋時要特別檢查 SQL injection 與參數化。
- `MainController.HtmlScraper` 目前例外回應可能包含 stack trace；若處理外部 URL 或錯誤呈現，需評估資訊揭露。
- `BaseController.GetUserCodeNames(string emid)` 目前有硬編碼回傳，若調整權限邏輯要先釐清臨時碼是否仍被依賴。

## RAG 切分建議

RAG 不應把整個 repo 當成同一塊知識。建議以「功能 + 邊界」切分：

- `runtime`: solution、csproj、IIS Express、Web.config key 名稱。
- `hq-front`: `HQ/Views/HQ`、`HQ/Views/Home`、`HQ/Scripts/Hq`、`HQ/Scripts/Home`。
- `hq-api`: `HQ/Controllers/api`。
- `hq-old-backside`: `HQ/Controllers/BacksideController.cs`、`HQ/Views/Backside`、`HQ/Scripts/Backside`。
- `hqbacksite-auth`: `AccountController`、`BackSiteAuthorizeAttribute`、`JwtUtil`、layout login/logout。
- `hqbacksite-menu`: `MenuController`、`Menu/` views、`lib.js`、`menu.js`、`content.js`。
- `word-html`: `WordHtmlConverter`、WordHtml views/actions/tests、既有 Word HTML module docs。
- `tests`: `tests/HQBackSite.Tests`。
- `vendor-generated`: HelpPage、UEditor、Bootstrap、jQuery、Modernizr、圖片/字型/Office 匯出靜態頁。

對應 chunk/查詢索引詳見 `memory-bank/11-rag/hq-rag-index.md` 與 `memory-bank/11-rag/hq-code-index.jsonl`。

## Skill 使用建議

在此 repo 內工作時，先讀：

1. `AGENTS.md`
2. `memory-bank/00-index.md`
3. 本文件
4. `memory-bank/10-modules/hq-file-analysis.md`
5. 依功能讀 `memory-bank/11-rag/hq-rag-index.md` 或既有 module docs

若任務涉及特定模組，再套用既有技能：

- 登入、JWT、cookie、未授權流程：`hq-auth-and-cookie-jwt`
- SQL、`para`、`news`、`content`、`unit`、權限表：`hq-sql-and-para-table`
- `MenuController` CRUD：`hqbacksite-menu-crud`
- Word 轉 HTML：`hqbacksite-word-html`
- net472 測試：`hq-test-runner-net472`
- Legacy MVC/IIS Express 啟動與路由：`aspnet-mvc-legacy-maintenance`

Repo-local skill source 放在 `memory-bank/13-skills/skm-hq-master-boundary/SKILL.md`。
