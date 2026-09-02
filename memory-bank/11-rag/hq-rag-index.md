# HQ / HQBackSite RAG 索引

Status: 依據目前 repo 程式碼整理，日期為 2026-09-03。

## 目的

這份文件定義 HQ/HQBackSite 要如何被 RAG、AI agent、code search 切分與取用。它不是完整架構文件，而是「查問題時先抓哪些文件」的路由表。

完整規格請先讀：

1. `memory-bank/10-modules/hq-frontend-backend-boundary.md`
2. `memory-bank/10-modules/hq-file-analysis.md`
3. `memory-bank/PROJECT_DOCUMENTATION.md`

## 檢索總原則

- 先抓 project docs，再抓程式碼。
- 以功能切 chunk，不以整個專案或整個資料夾切 chunk。
- 第一方 C#/Razor/JS/CSS 每檔可獨立索引；超過約 500 行的大檔要依方法/region 切。
- 第三方、HelpPage、UEditor、Bootstrap、jQuery、GeneratedHtml、userfiles、圖片與 build output 預設排除。
- 不要把完整 connection string、密碼、JWT signing key、cookie/token 值放進 RAG。
- 若 RAG 命中 `Web.config`，只允許摘要 connection string 名稱與 appSetting key。

## Chunk Taxonomy

| Chunk id | 何時取用 | 優先路徑 | 排除/注意 |
| --- | --- | --- | --- |
| `runtime-solution` | build、startup、port、IIS Express、solution 問題 | `HQ.sln`、`HQ/*.csproj`、`HQBackSite/*.csproj`、`Global.asax.cs`、`App_Start/*`、`README.md` | `.vs` 可用於本機啟動診斷，但不要當跨環境真相 |
| `config-secrets-safe` | connection string 名稱、appSettings、環境差異 | `HQ/Web.config`、`HQBackSite/Web.config` | 不嵌入密碼或完整 connectionString |
| `hq-front-home` | HQ 首頁、跑馬燈、banner、IT/HR 公文、ICS 區塊 | `HQ/Views/HQ/Index.cshtml`、`HQ/Scripts/Hq/Index.js`、`MainController`、`MarqueeController`、`IcsController` | `MainController` 需分方法群 |
| `hq-front-dept` | 部門/分店頁、tabs、頁面內容、分店公文 | `HQ/Views/HQ/Dept.cshtml`、`HQ/Scripts/Hq/Dept.js`、`DeptController`、`MainController.ItData` | 特別看分店代碼 mapping |
| `hq-front-phone` | 電話簿、分機、系統/fax/餐廳舊頁 | `HQ/Views/Home/*`、`HQ/Scripts/Home/mm_menu.js`、`ValuesController`、`HQ/Areas/tel/*` | `ValuesController.SqlString` 動態 SQL 高風險 |
| `hq-old-backside` | 舊後台 Page/Unit/Content/News/Safe/Hr | `BacksideController`、`Views/Backside/*`、`Scripts/Backside/*`、`Page/Unit/Content/NewsController` | 舊後台 auth 是 Forms auth，不是 HQBackSite JWT |
| `hqbacksite-auth` | 新後台登入、JWT cookie、未授權、logout | `AccountController`、`BackSiteAuthorizeAttribute`、`JwtUtil`、`GlobalExceptionFilterAttribute`、`OperationLogAttribute` | 不摘錄 JWT key；local debug login 要標示為本機行為 |
| `hqbacksite-menu-crud` | 新後台選單、類別、內容、訊息、ICS CRUD | `MenuController`、`Views/Menu/*`、`Models/*`、`Scripts/lib.js`、`content.js`、`menu.js` | `MenuController` 依 region/action 切 chunk |
| `word-html` | Word 上傳轉 HTML、preview、圖片、連結、table | `WordHtmlConverter`、`MenuController.WordHtml*`、`WordHtmlTest.cshtml`、相關測試與 module docs | 已有專門規格，優先讀既有 Word HTML docs |
| `tests-hqbacksite` | 跑測、補測、回歸範圍 | `tests/HQBackSite.Tests/*`、`hq-test-runner-net472` skill | HQ Web API 目前缺測，要在回覆中講明 |
| `vendor-generated` | 判斷某檔是否可修改 | `HQ/Areas/HelpPage/**`、`HQ/ueditor/**`、Bootstrap/jQuery、GeneratedHtml、userfiles、images | 預設低優先，不要自動改 vendor |

## Route / UI / Data 對照

| 功能 | UI/前端 | 後端 | 資料來源 | 測試 |
| --- | --- | --- | --- | --- |
| HQ 首頁 | `HQ/Views/HQ/Index.cshtml`、`HQ/Scripts/Hq/Index.js` | `MainController`、`MarqueeController`、`IcsController` | `news`、`ics_list`、`CAMPAIGN_*`、linked `webmax6`、MemoryCache | 目前無直接測試 |
| HQ 部門頁 | `HQ/Views/HQ/Dept.cshtml`、`HQ/Scripts/Hq/Dept.js` | `DeptController`、部分 `MainController.ItData` | `para`、`unit`、`content`、`news`、`CAMPAIGN_*`、linked `webmax6` | 目前無直接測試 |
| 電話簿 | `HQ/Views/Home/Index.cshtml`、`HQ/Scripts/Home/mm_menu.js` | `ValuesController` | `StoreTel`、`emp_tel`、`users`、linked HR DB | 目前無直接測試 |
| 舊後台登入 | `HQ/Views/Backside/Login.cshtml` | `BacksideController.Login` | `Users`、Forms auth ticket | 目前無直接測試 |
| 舊後台頁面設定 | `Views/Backside/Page.cshtml`、`Backside/Page.js` | `PageController` | `page`、`para` | 目前無直接測試 |
| 舊後台單元設定 | `Views/Backside/Unit.cshtml`、`Backside/Unit.js` | `UnitController` | `unit`、`para` | 目前無直接測試 |
| 舊後台內容設定 | `Views/Backside/Content.cshtml`、`Backside/Content.js` | `ContentController` | `content`、`para` | 目前無直接測試 |
| 舊後台訊息/資安/人資 | `Views/Backside/News|Safe|Hr.cshtml`、對應 JS | `NewsController` | `news` | 目前無直接測試 |
| 新後台登入 | `HQBackSite/Views/Account/Login.cshtml`、`login.js` | `AccountController`、`JwtUtil`、`BackSiteAuthorizeAttribute` | `users`、local bootstrap appSettings | 目前無直接測試 |
| 新後台 layout/menu | `_Layout.cshtml`、`_Menu.cshtml`、`menu.js` | `MenuController.GetMenu`、`GetUserInfo`、`GetUserMenuPermissions` | `users`、`t_menu_resource`、`t_user_menu_permission` | 間接測試 |
| 新後台選單設定 | `Menu/Index*.cshtml` | `MenuController.Index*` | `para type='1050'` | 可補測 |
| 新後台權限設定 | `Menu/Permission.cshtml` | `MenuController.Permission*` | `para_permission`、`users` | 可補測 |
| 新後台類別設定 | `Menu/Category*.cshtml` | `MenuController.Category*` | `content subtype='d_topbtn'`、`para` | 可補測 |
| 新後台內容設定 | `Menu/Content*.cshtml`、`content.js`、`lib.ckeditor.js` | `MenuController.Content*`、`UploadFileForUrl` | `content`、`para`、uploaded files | `MenuControllerContentTests` |
| 新後台訊息設定 | `Menu/Message*.cshtml`、`content.js`、`lib.ckeditor.js` | `MenuController.Message*` | `news` | 可補測 |
| 新後台 ICS | `Menu/ICS*.cshtml`、`content.js` | `MenuController.ICS*`、前台 `IcsController` | `news`、`ics_list`、`ics_upload` | `MenuControllerIcsTests` |
| Word 轉 HTML | `Menu/WordHtmlTest.cshtml` | `MenuController.WordHtml*`、`WordHtmlConverter` | uploaded `.docx`、`Views/GeneratedHtml`、images | `WordHtmlConverterTests`、`MenuControllerWordHtmlTests` |

## 建議 Chunk 切法

### Small/normal files

以下檔案可一檔一 chunk：

- `App_Start/*.cs`
- `Global.asax.cs`
- `HqController.cs`
- `HomeController.cs`
- `BacksideController.cs`
- `PageController.cs`
- `UnitController.cs`
- `ContentController.cs`
- `NewsController.cs`
- `RoomController.cs`
- `IcsController.cs`
- `MarqueeController.cs`
- `BaseController.cs`
- `AccountController.cs`
- `Attributes/*.cs`
- `Models/*.cs`
- `Scripts/Backside/*.js`
- `Scripts/Common/*.js`
- `HQBackSite/Scripts/*.js`
- normal `.cshtml`

### Large files

下列檔案請依方法/區段切 chunk：

| 檔案 | 建議 chunk |
| --- | --- |
| `HQ/Controllers/api/MainController.cs` | `HtmlScraper`、link parsing/PDF helpers、`Read/Campaign`、`WebConnectData`、`SafeRead/HrRead`、`HrData/ItData`、`Count*`、`BannerData`、`ReleaseAllCache` |
| `HQ/Controllers/api/DeptController.cs` | `TabsPage/PageContent`、campaign helper、store mapping、`LetterData`、`NewData`、`DeptNewData` |
| `HQ/Controllers/api/ValuesController.cs` | `Read/Search`、`SqlString` branch mapping |
| `HQ/Scripts/Hq/Index.js` | marquee/banner、IT/HR content、main left/right、ICS/common links、safe/hr home、tabs/cookie/timers |
| `HQ/Scripts/Hq/Dept.js` | tabs/page content、render helpers、department news、letter/system/new data、tab interactions |
| `HQ/Scripts/Home/mm_menu.js` | DHTML menu engine、phone API integration |
| `HQBackSite/Controllers/MenuController.cs` | wrappers/access, Index, Permission, Category, Content, Upload, WordHtml, Message, ICS, layout/menu permissions |
| `HQBackSite/Utils/WordHtmlConverter.cs` | public API, document loading, paragraph/run conversion, table conversion, hyperlink rewrite, image extraction, CSS/sanitization helpers |
| `HQBackSite/Content/Site.css` | layout/sidebar, search/table/pager/modal, login/header, editor styling |

## 查詢提示詞建議

| 使用者問題 | RAG tags/query |
| --- | --- |
| 服務啟動、localhost、port 錯 | `runtime-solution`, `iis-express`, `14555`, `57568`, `applicationhost.config` |
| 頁面亂碼 | `charset`, `encoding`, `itMember.html`, `Web.config globalization`, `Content-Type` |
| HQ 首頁缺資料 | `hq-front-home`, `Index.js`, `MainController`, `MarqueeController`, `IcsController`, `news type` |
| 分店/部門頁資料不對 | `hq-front-dept`, `DeptController`, `para type 1050`, `unit`, `content`, `webmax6`, `CAMPAIGN_CONTENT` |
| 後台登入問題 | `hqbacksite-auth`, `Authorization cookie`, `JwtUtil`, `BackSiteAuthorize`, `LocalBootstrapAccounts` |
| 新後台左側選單不顯示 | `GetUserMenuPermissions`, `t_menu_resource`, `t_user_menu_permission`, `menu.js`, `data-menu-code` |
| 新後台內容 CRUD | `hqbacksite-menu-crud`, `ContentModel`, `ContentQuery`, `ContentAdd`, `ContentUpdate`, `UploadFileForUrl` |
| ICS 前後台 | `ICS`, `IcsController`, `MenuController.ICS`, `ics_list`, `ics_upload`, `NewsModel` |
| Word 轉 HTML | `word-html`, `WordHtmlConverter`, `WordHtmlConvert`, `WordHtmlPreview`, `forcePdfHyperlinks` |
| 電話搜尋 | `ValuesController`, `mm_menu.js`, `StoreTel`, `emp_tel`, `SqlString` |
| SQL 表關係 | `hq-sql-and-para-table`, `para`, `content`, `unit`, `news`, `permission` |
| 測試怎麼跑 | `hq-test-runner-net472`, `HQBackSite.Tests`, `vstest.console`, `MSBuild` |

## Metadata Schema

`hq-code-index.jsonl` 每列建議欄位：

| 欄位 | 用途 |
| --- | --- |
| `id` | 穩定 chunk id。 |
| `title` | 人可讀標題。 |
| `layer` | `runtime`、`frontend`、`backend-api`、`backend-mvc`、`data-model`、`tests`、`vendor-generated`。 |
| `area` | 功能區，例如 `hq-front-home`、`hqbacksite-auth`。 |
| `paths` | 相關檔案清單。 |
| `routes` | 相關 URL/action。 |
| `data_sources` | 表或外部資料源名稱，不含 secrets。 |
| `tags` | 檢索 tag。 |
| `summary` | 行為摘要。 |
| `related_docs` | 相關 memory-bank 文件。 |
| `risk` | 安全/資料/維護注意事項。 |

## RAG Guardrails

- 對 config 類問題，只檢索 key 名稱與用途，不回填真實密碼。
- 對資料修改問題，先檢索前端呼叫端、後端 action、資料表、測試四個面向。
- 對舊後台與新後台問題，先判斷 cookie/auth 機制：HQ Backside 是 Forms auth；HQBackSite 是 JWT cookie。
- 對 Word 轉 HTML，優先用既有 module docs 與測試；不要只看 `MenuController`。
- 對 vendor/generated 命中，先確認是否真的要修改；多數情況應改呼叫端或包覆層。
