# HQ / HQBackSite 檔案逐檔分析

Status: 依據目前 repo 程式碼整理，日期為 2026-09-03。

## 分析範圍

本文件逐檔整理第一方可維護程式碼，並把第三方、scaffold、匯出靜態頁與資產明確標示。這樣後續 AI/RAG 取用時，可以避免把 vendor 或產出檔誤當成業務邏輯。

分類標籤：

- `Frontend`: Razor view、瀏覽器端 JS/CSS、靜態 HTML。
- `Backend MVC`: ASP.NET MVC controller/filter/helper，回 view 或 JSON。
- `Backend API`: ASP.NET Web API controller，主要供前台/舊後台 JS 呼叫。
- `Data/Model`: ViewModel/DTO/paging/result model。
- `Runtime`: solution、csproj、Global.asax、Route、Bundle、Web.config。
- `Tests`: MSTest 回歸測試。
- `Vendor/Generated`: 第三方套件、HelpPage scaffold、UEditor、Excel 匯出頁、GeneratedHtml、圖片/字型/二進位資產。

## Root 與 Runtime

| 檔案 | 分類 | 分析 |
| --- | --- | --- |
| `AGENTS.md` | Runtime | repo 工作準則。要求先讀文件、保護 dirty changes、驗證後提交、不要洩漏 secrets。 |
| `README.md` | Runtime | solution 結構、build/test/runtime 基本說明；目前 runtime 只列 HQBackSite，需要搭配本文件看 HQ port。 |
| `HQ.sln` | Runtime | Visual Studio solution，包含 `HQ`、`HQBackSite`、`HQBackSite.Tests` 三個 project。 |
| `.editorconfig` | Runtime | 編輯器格式設定。改程式前可確認縮排/編碼。 |
| `.gitattributes` | Runtime | Git 屬性設定。通常不碰。 |
| `.gitignore` | Runtime | 忽略規則。此檔目前在工作區已有 unrelated dirty change，除非任務指定不要修改。 |
| `package.json` / `package-lock.json` | Runtime | Node 依賴/工具設定；目前不是主要 build path。 |
| `PROJECT_DOCUMENTATION.md` | Runtime | 根目錄既有總文件；memory-bank 內另有一份同名文件。 |
| `MENU_PERMISSION_TECH_DOC.md` | Runtime | 選單權限技術文件；內容需以目前 `MenuController` 與資料表現況校準。 |
| `.vs/HQ/config/applicationhost.config` | Runtime | 本機 IIS Express 設定。非版本化主要原始碼，但啟動服務時必須確認 physical path 與 ports。 |

## Memory Bank 文件

| 檔案 | 分類 | 分析 |
| --- | --- | --- |
| `memory-bank/00-index.md` | Runtime | AI/human 文件入口，應引用新增 module/RAG/skill 文件。 |
| `memory-bank/PROJECT_DOCUMENTATION.md` | Runtime | 既有專案總覽，記錄 startup、route、auth、operation log、主要資料表與注意事項。 |
| `memory-bank/workflows.md` | Runtime | 匯入自 Cline 的通用流程；內含 Spring/Thymeleaf 舊脈絡，不完全符合此 ASP.NET repo，使用時要以 `AGENTS.md` 與目前程式為準。 |
| `memory-bank/pr-review.md` | Runtime | PR review 流程參考。 |
| `memory-bank/20-pr-review-standards.md` | Runtime | review 標準參考。 |
| `memory-bank/09-decisions/2026-03-10-word-html-converter.md` | Runtime | Word 轉 HTML 技術決策紀錄。 |
| `memory-bank/10-modules/hqbacksite-word-html-test-page.md` | Runtime | Word 上傳轉靜態 HTML 測試頁技術設計。 |
| `memory-bank/10-modules/hqbacksite-word-html-technical-spec.md` | Runtime | WordHtmlConverter 現行技術規格。 |
| `memory-bank/10-modules/hq-frontend-backend-boundary.md` | Runtime | 本次新增的前後端/資料/安全邊界總規格。 |
| `memory-bank/10-modules/hq-file-analysis.md` | Runtime | 本文件，逐檔分析索引。 |
| `memory-bank/11-rag/hq-rag-index.md` | Runtime | RAG 查詢與 chunk 切分說明。 |
| `memory-bank/11-rag/hq-code-index.jsonl` | Runtime | RAG-friendly JSONL code index。 |
| `memory-bank/13-skills/skm-hq-master-boundary/SKILL.md` | Runtime | repo-local Codex skill source。 |

## HQ 專案 Runtime

| 檔案 | 分類 | 分析 |
| --- | --- | --- |
| `HQ/HQ.csproj` | Runtime | ASP.NET MVC/Web API project，TargetFramework `v4.6.1`，IIS Express port `14555`。 |
| `HQ/packages.config` | Runtime | NuGet packages：MVC/WebApi 5.2.4、AngleSharp、iTextSharp、Newtonsoft.Json、Encoding.CodePages 等。 |
| `HQ/Web.config` | Runtime | HQ connection string、appSettings、Forms auth、modules/handlers、compilation 設定。只記 key 名稱，不複製密碼。 |
| `HQ/Web.Debug.config` | Runtime | Debug transform。 |
| `HQ/Web.Release.config` | Runtime | Release transform。 |
| `HQ/Global.asax` | Runtime | ASP.NET application entry markup，指向 `MvcApplication`。 |
| `HQ/Global.asax.cs` | Runtime | Application_Start 註冊 Area、Web API、filters、routes、bundles，並移除 XML formatter media types。 |
| `HQ/App_Start/BundleConfig.cs` | Runtime | 定義 jQuery/util/modernizr/bootstrap/css bundle，另有 `~/Scripts/layout` 與 `~/Scripts/dept`。 |
| `HQ/App_Start/FilterConfig.cs` | Runtime | 註冊 global `HqErrorHandlerAttribute`；例外導向 error view。 |
| `HQ/App_Start/RouteConfig.cs` | Runtime | MVC default route `{controller}/{action}/{id}`，預設 `Hq/index`。 |
| `HQ/App_Start/WebApiConfig.cs` | Runtime | Web API route `api/{controller}/{action}/{id}`。 |
| `HQ/Properties/AssemblyInfo.cs` | Runtime | Assembly metadata。 |

## HQ MVC Controllers

| 檔案 | 分類 | 分析 |
| --- | --- | --- |
| `HQ/Controllers/HqController.cs` | Backend MVC | 員工入口 `Room1`、`Index`、`Dept` 頁面入口，設定部分 `ViewBag.title`。 |
| `HQ/Controllers/HomeController.cs` | Backend MVC | 舊 Home/電話/系統/餐廳等靜態頁入口：`Index`、`FaxView`、`System`、`Restaurant`。 |
| `HQ/Controllers/BacksideController.cs` | Backend MVC | 舊 Backside 登入/登出與管理頁入口。POST Login 查 `Users` 並建立 FormsAuthentication ticket；Page/Unit/Content/News/Safe/Hr 受 `[Authorize]` 保護。 |

## HQ Web API Controllers

| 檔案 | 分類 | 分析 |
| --- | --- | --- |
| `HQ/Controllers/api/MainController.cs` | Backend API | 首頁與共用資料核心 API。包含公文/通訊資料合併、首頁 banner、跑馬燈以外的首頁區塊、資安/人資公告、外部連結、瀏覽次數、HTML/PDF 連結擷取、清除 MemoryCache。改此檔要同步看 `HQ/Scripts/Hq/Index.js`、`HQ/Scripts/Hq/Dept.js`、`HQ/Scripts/Hq/Left_btn.js`。 |
| `HQ/Controllers/api/DeptController.cs` | Backend API | 部門/分店頁 API。處理頁籤、頁面內容、分店公文、最新訊息、新舊 Newsletter/webmax6 資料合併與分店代碼 mapping。改此檔要同步看 `HQ/Scripts/Hq/Dept.js`。 |
| `HQ/Controllers/api/ValuesController.cs` | Backend API | 電話資料 API。`Read` 讀店別電話；`Search` 依 `bu/ad_dept/tel` 組合搜尋員工電話。`SqlString` 有大量動態 SQL 字串組裝，安全與維護風險較高。 |
| `HQ/Controllers/api/PageController.cs` | Backend API | 舊後台 Page CRUD 與 Dept/Page 下拉資料。以 `DataSet`/`int` 回傳，供 `HQ/Scripts/Backside/Page.js` 使用。 |
| `HQ/Controllers/api/UnitController.cs` | Backend API | 舊後台 Unit CRUD 與 Style/Dept/Page/Subtype 下拉資料，操作 `unit` 與 `para type='1010'`。 |
| `HQ/Controllers/api/ContentController.cs` | Backend API | 舊後台 Content CRUD 與 Dept/Page/Subtype 下拉資料，操作 `content`。部分查詢用 request dictionary keys 組 WHERE 條件，修改時注意白名單。 |
| `HQ/Controllers/api/NewsController.cs` | Backend API | 舊後台 News CRUD，支援一般訊息、資安、人資等依 `type/dept` 管理的資料。Create 使用 Forms auth ticket 寫入 create_user。 |
| `HQ/Controllers/api/RoomController.cs` | Backend API | 讀取 `para type='1050'` 與 `otb_edu_userinfo` CRUD。前台 `Left_btn.js` 使用 `Read` 產生左側選單。 |
| `HQ/Controllers/api/IcsController.cs` | Backend API | 前台國際認證規範公告查詢，查 `News type='ics'` 並 left join `ics_list`，支援 dept/background/des_no filter。 |
| `HQ/Controllers/api/MarqueeController.cs` | Backend API | 首頁跑馬燈查詢，讀 active `News type='light'` 且 dept 為跑馬燈設定。 |

## HQ Razor Views

| 檔案 | 分類 | 分析 |
| --- | --- | --- |
| `HQ/Views/_ViewStart.cshtml` | Frontend | 指定預設 layout 為 `~/Views/Shared/_Layout.cshtml`。 |
| `HQ/Views/Shared/_Layout.cshtml` | Frontend | HQ 前台主 layout，載入 bundle、`~/Scripts/layout`、共用 CSS 與 legacy HTML 結構。 |
| `HQ/Views/Shared/_Backside.cshtml` | Frontend | 舊 Backside layout，載入 UEditor、Common Backside 左側按鈕與共用樣式。 |
| `HQ/Views/Shared/_Login.cshtml` | Frontend | 舊 Backside login layout。 |
| `HQ/Views/Shared/Error.cshtml` | Frontend | HQ 錯誤頁。 |
| `HQ/Views/HQ/Index.cshtml` | Frontend | 員工入口首頁 Razor，大量 HTML 區塊由 `HQ/Scripts/Hq/Index.js` 補資料與互動。 |
| `HQ/Views/HQ/Dept.cshtml` | Frontend | 部門/分店頁 Razor，載入 `~/Scripts/dept`，內容由 `Dept.js` 組裝。 |
| `HQ/Views/HQ/Room1.cshtml` | Frontend | Room1 頁面，目前引用 `~/Scripts/Kit/HrAnnouncement.js`，在目前檔案清單中未找到該 JS，屬可能 stale/broken reference。 |
| `HQ/Views/Home/Index.cshtml` | Frontend | Legacy 首頁/電話入口，載入 `mm_menu.js`。 |
| `HQ/Views/Home/FaxView.cshtml` | Frontend | 匯出式 fax directory HTML view，layout null，維護時多半視為靜態資料頁。 |
| `HQ/Views/Home/System.cshtml` | Frontend | 匯出式 system contact HTML view，layout null，含舊式表格/圖片連結。 |
| `HQ/Views/Home/Restaurant.cshtml` | Frontend | 靜態餐廳資訊 view，layout null。 |
| `HQ/Views/Backside/Login.cshtml` | Frontend | 舊 Backside 登入表單，model 為 `LoginViewModel`。 |
| `HQ/Views/Backside/Page.cshtml` | Frontend | 舊後台頁面設定 UI，載入 `Backside/Page.js`。 |
| `HQ/Views/Backside/Unit.cshtml` | Frontend | 舊後台單元設定 UI，載入 `Backside/Unit.js`。 |
| `HQ/Views/Backside/Content.cshtml` | Frontend | 舊後台內容設定 UI，載入 `Backside/Content.js`。 |
| `HQ/Views/Backside/News.cshtml` | Frontend | 舊後台訊息設定 UI，載入 `Backside/News.js`。 |
| `HQ/Views/Backside/Safe.cshtml` | Frontend | 舊後台資安內容 UI，載入 `Backside/Safe.js`。 |
| `HQ/Views/Backside/Hr.cshtml` | Frontend | 舊後台人資內容 UI，載入 `Backside/Hr.js`。 |
| `HQ/Views/Backside/Test.cshtml` | Frontend | UEditor 測試頁，form 指向 `UEditor/Index`，目前 controller 清單沒有相對應 MVC controller，視為 legacy/stale 測試入口。 |

## HQ Business JavaScript

| 檔案 | 分類 | 分析 |
| --- | --- | --- |
| `HQ/Scripts/util.js` | Frontend | 定義 `g_cus.CommonConnect` 與 `g_api.AjaxPost`。所有 legacy AJAX 走同步 jQuery POST JSON；這是 HQ JS 與 Web API 的主要橋接點。 |
| `HQ/Scripts/Hq/Index.js` | Frontend | 員工入口首頁動態渲染：IT/HR 公文、banner、首頁左右區塊、ICS、常用連結、資安/人資公告、tab/cookie/輪播互動。 |
| `HQ/Scripts/Hq/Dept.js` | Frontend | 部門/分店頁動態渲染：tabs、page content、download 區、department news、分店公文、系統通訊、最新訊息、頁籤切換。 |
| `HQ/Scripts/Hq/Left_btn.js` | Frontend | HQ 前台左側選單、瀏覽次數呼叫、電話搜尋切換與 cookie 清理。 |
| `HQ/Scripts/Home/mm_menu.js` | Frontend | 大型 legacy menu/電話簿 script，含 DHTML menu 與 `Values/Read`、`Values/Search` 呼叫。 |
| `HQ/Scripts/Common/Backside_lbtn.js` | Frontend | 舊 Backside 左側選單與頁面導向。 |
| `HQ/Scripts/Backside/Page.js` | Frontend | 舊後台 Page CRUD UI，呼叫 `/api/Page/*`。 |
| `HQ/Scripts/Backside/Unit.js` | Frontend | 舊後台 Unit CRUD UI，呼叫 `/api/Unit/*`。 |
| `HQ/Scripts/Backside/Content.js` | Frontend | 舊後台 Content CRUD UI，整合 UEditor，呼叫 `/api/Content/*`。 |
| `HQ/Scripts/Backside/News.js` | Frontend | 舊後台 generic News CRUD UI，呼叫 `/api/News/*`。 |
| `HQ/Scripts/Backside/Safe.js` | Frontend | 舊後台資安公告 UI，以 `type='msg1'` 使用 `/api/News/*`。 |
| `HQ/Scripts/Backside/Hr.js` | Frontend | 舊後台人資公告 UI，以 `type='msg2'` 使用 `/api/News/*`。 |

## HQ CSS 與靜態頁

| 檔案/路徑 | 分類 | 分析 |
| --- | --- | --- |
| `HQ/Content/Site.css` | Frontend | ASP.NET MVC template 基礎樣式與 pagination/table 小調整。 |
| `HQ/Content/css.css` | Frontend | legacy 全站共用樣式，主要是新細明體、連結與 `.fnt*` class。 |
| `HQ/Content/bootstrap*.css` | Vendor/Generated | Bootstrap 樣式與 map 檔，通常不改。 |
| `HQ/itMember.html` | Frontend | 直接 served 的資訊部成員/職掌靜態頁；已含 UTF-8 meta。 |
| `HQ/userfiles/file/skmbutterfly.html` | Frontend | userfiles 下的靜態 HTML 內容，可能由後台/歷史資料上傳而來。 |
| `HQ/userfiles/file/skmsmell.html` | Frontend | userfiles 下的靜態 HTML 內容，可能由後台/歷史資料上傳而來。 |
| `HQ/Areas/tel/fax.htm` | Vendor/Generated | Excel 匯出/legacy 靜態 fax 頁。 |
| `HQ/Areas/tel/system.htm` | Vendor/Generated | Excel 匯出/legacy 靜態 system 頁。 |
| `HQ/Areas/tel/restaurant.htm` | Vendor/Generated | legacy 靜態餐廳頁。 |
| `HQ/Areas/tel/*/*.htm`、`*.xml`、`stylesheet.css` | Vendor/Generated | Excel 匯出輔助檔；除非修靜態頁呈現，不當作業務邏輯。 |

## HQ HelpPage Scaffold

| 檔案/路徑 | 分類 | 分析 |
| --- | --- | --- |
| `HQ/Areas/HelpPage/**/*.cs` | Vendor/Generated | ASP.NET Web API HelpPage scaffold，用來產生 API 說明頁、sample、model description。沒有 HQ 業務邏輯。 |
| `HQ/Areas/HelpPage/Views/**` | Vendor/Generated | HelpPage Razor view scaffold。只有修改 API help 顯示時才進入。 |
| `HQ/Areas/HelpPage/App_Start/HelpPageConfig.cs` | Vendor/Generated | HelpPage 設定入口。若 API docs 沒使用，可保持不動。 |

## HQ UEditor

| 檔案 | 分類 | 分析 |
| --- | --- | --- |
| `HQ/ueditor/index.html` | Vendor/Generated | UEditor demo/index。 |
| `HQ/ueditor/net/controller.ashx` | Vendor/Generated | UEditor ASP.NET handler，依 action 分派 config、uploadimage、uploadfile、listimage、listfile、catchimage 等。 |
| `HQ/ueditor/net/config.json` | Vendor/Generated | UEditor 上傳/列檔設定，包含允許副檔名、大小、保存路徑、URL prefix。 |
| `HQ/ueditor/net/Web.config` | Vendor/Generated | UEditor handler 區域設定。 |
| `HQ/ueditor/net/App_Code/Config.cs` | Vendor/Generated | 讀取 `config.json` 並提供 typed getter。 |
| `HQ/ueditor/net/App_Code/ConfigHandler.cs` | Vendor/Generated | 輸出 UEditor config。 |
| `HQ/ueditor/net/App_Code/CrawlerHandler.cs` | Vendor/Generated | 遠端圖片抓取 handler。 |
| `HQ/ueditor/net/App_Code/Handler.cs` | Vendor/Generated | UEditor handler base class 與 JSON output。 |
| `HQ/ueditor/net/App_Code/ListFileHandler.cs` | Vendor/Generated | 圖片/檔案列表 handler。 |
| `HQ/ueditor/net/App_Code/NotSupportedHandler.cs` | Vendor/Generated | 不支援 action 的回應。 |
| `HQ/ueditor/net/App_Code/PathFormater.cs` | Vendor/Generated | 上傳檔名與路徑格式化。 |
| `HQ/ueditor/net/App_Code/UploadHandler.cs` | Vendor/Generated | 上傳處理、大小/副檔名檢查、base64 scrawl 支援。 |

## HQ 第三方 JS

| 檔案 | 分類 | 分析 |
| --- | --- | --- |
| `HQ/Scripts/jquery-1.9.1.js` | Vendor/Generated | jQuery。 |
| `HQ/Scripts/bootstrap.js` / `bootstrap.min.js` | Vendor/Generated | Bootstrap JS。 |
| `HQ/Scripts/modernizr-2.8.3.js` | Vendor/Generated | Modernizr。 |
| `HQ/Scripts/jquery.marquee.min.js` | Vendor/Generated | 跑馬燈 plugin，首頁使用。 |

## HQBackSite Runtime

| 檔案 | 分類 | 分析 |
| --- | --- | --- |
| `HQBackSite/HQBackSite.csproj` | Runtime | 新版後台 ASP.NET MVC project，TargetFramework `v4.7.2`，IIS Express port `57568`。 |
| `HQBackSite/packages.config` | Runtime | NuGet packages：Dapper、NLog、JWT/Microsoft.IdentityModel、MVC 5.2.7、System.DirectoryServices 等。 |
| `HQBackSite/Web.config` | Runtime | 新後台 connection string、appSettings、customErrors、compilation 設定。只記 key 名稱，不複製密碼。 |
| `HQBackSite/Web.Debug.config` | Runtime | Debug transform。 |
| `HQBackSite/Web.Release.config` | Runtime | Release transform。 |
| `HQBackSite/Global.asax` | Runtime | ASP.NET application entry markup，指向 `MvcApplication`。 |
| `HQBackSite/Global.asax.cs` | Runtime | Application_Start 註冊 Area、filters、routes、bundles。 |
| `HQBackSite/App_Start/BundleConfig.cs` | Runtime | 目前沒有註冊 bundle；layout 直接引用 CSS/JS。 |
| `HQBackSite/App_Start/FilterConfig.cs` | Runtime | 註冊 `OperationLogAttribute` 與 `GlobalExceptionFilterAttribute`。 |
| `HQBackSite/App_Start/RouteConfig.cs` | Runtime | MVC default route `{controller}/{action}/{id}`，預設 `Home/Index`。 |
| `HQBackSite/Properties/AssemblyInfo.cs` | Runtime | Assembly metadata。 |

## HQBackSite Attributes / Utils

| 檔案 | 分類 | 分析 |
| --- | --- | --- |
| `HQBackSite/Attributes/BackSiteAuthorizeAttribute.cs` | Backend MVC | 新後台授權 filter。讀 `Authorization` cookie、驗 JWT、檢查 local bootstrap 或 `EpSqlServer.dbo.users`。Ajax 未授權回 JSON，非 Ajax redirect login。 |
| `HQBackSite/Attributes/OperationLogAttribute.cs` | Backend MVC | action 前後操作紀錄，記 controller/action/url/query/form/args/result/elapsed/status，內建敏感欄位遮蔽。 |
| `HQBackSite/Attributes/GlobalExceptionFilterAttribute.cs` | Backend MVC | 全域例外處理與 log。local request 可顯示較詳細錯誤；AJAX/JSON 回 JSON，其他回 Error view。 |
| `HQBackSite/Utils/JwtUtil.cs` | Backend MVC | JWT 產生/驗證工具。含 signing key，文件與回覆不得複製 key 值。 |
| `HQBackSite/Utils/WordHtmlConverter.cs` | Backend MVC | `.docx` 轉 HTML 核心工具。處理 OOXML paragraph/run/table/hyperlink/image/highlight/shading/PDF link rewrite/image extraction。細節見 Word HTML 技術規格。 |

## HQBackSite Controllers

| 檔案 | 分類 | 分析 |
| --- | --- | --- |
| `HQBackSite/Controllers/BaseController.cs` | Backend MVC | 新後台 controller base。提供 UTF-8/camelCase JSON、`Success/Fail/SuccessData`、Dapper DB helper、cookie account、部門權限 helper。 |
| `HQBackSite/Controllers/AccountController.cs` | Backend MVC | 新後台登入/登出。POST Login 驗帳號與 local bootstrap，發 `Authorization` JWT cookie。AD 驗證程式存在但目前登入流程中註解未啟用。 |
| `HQBackSite/Controllers/HomeController.cs` | Backend MVC | 新後台首頁與錯誤頁，`Index` 受 `[BackSiteAuthorize]`。 |
| `HQBackSite/Controllers/MenuController.cs` | Backend MVC | 新後台核心功能 controller。包含選單、權限、類別、內容、檔案上傳、Word 轉 HTML、訊息、ICS、layout child actions、選單權限 API。改此檔通常要同步看 `Views/Menu`、`Scripts/lib.js`、`Scripts/content.js`、`Scripts/menu.js` 與測試。 |

## HQBackSite Models

| 檔案 | 分類 | 分析 |
| --- | --- | --- |
| `HQBackSite/Models/PageModel.cs` | Data/Model | paging base model，PageNo/PageSize/TotalCount，含 min/default 防護。 |
| `HQBackSite/Models/PagedListModel.cs` | Data/Model | `List<T>` + paging metadata，實作 `IPagedList`。 |
| `HQBackSite/Models/PagerModel.cs` | Data/Model | pager partial view model 與 `IPagedList` interface。 |
| `HQBackSite/Models/ResultModel.cs` | Data/Model | 統一 JSON response model：Code/Message/Data。 |
| `HQBackSite/Models/UserModel.cs` | Data/Model | 員工/登入 model：ORGAN_ID、DEPARTMENT、EMPLOYEE_ID、LOCAL_NAME、EMAIL、Account、Password。 |
| `HQBackSite/Models/ParaModel.cs` | Data/Model | `para` table view model，含 `data6text` 顏色顯示 mapping。 |
| `HQBackSite/Models/ParaPermissionModel.cs` | Data/Model | `para_permission` 權限 model。 |
| `HQBackSite/Models/ContentModel.cs` | Data/Model | `content` table view model，繼承 `PageModel`，含 subtype 顯示文字 mapping。 |
| `HQBackSite/Models/NewsModel.cs` | Data/Model | `news` table view model，繼承 `PageModel`，含 status/urlpath/ICS JSON 與 group 欄位。 |
| `HQBackSite/Models/IcsGroupModel.cs` | Data/Model | ICS 明細 group，包含 list 與 uploads。 |
| `HQBackSite/Models/IcsListModel.cs` | Data/Model | `ics_list` model：文件分類、編號、名稱、URL、附件、主責、備註、刪除旗標。 |
| `HQBackSite/Models/IcsUploadModel.cs` | Data/Model | `ics_upload` model：檔名、URL、content type、刪除旗標。 |
| `HQBackSite/Models/MenuGroupModel.cs` | Data/Model | 左側選單群組 model。 |
| `HQBackSite/Models/MenuItemModel.cs` | Data/Model | 左側選單項目 model。 |
| `HQBackSite/Models/MenuViewModel.cs` | Data/Model | layout menu view model，包含 current module 與 groups。 |
| `HQBackSite/Models/MenuResourceModel.cs` | Data/Model | `t_menu_resource`、`t_user_menu_permission` 與權限 API response model。 |

## HQBackSite Views

| 檔案 | 分類 | 分析 |
| --- | --- | --- |
| `HQBackSite/Views/_ViewStart.cshtml` | Frontend | 指定預設 layout 為 `~/Views/Shared/_Layout.cshtml`。 |
| `HQBackSite/Views/Shared/_Layout.cshtml` | Frontend | 新後台主 layout。載入 bootstrap/site/fontawesome、使用者資訊、左側選單、logout、`lib.js`、`menu.js`。 |
| `HQBackSite/Views/Shared/_LayoutForAnonymous.cshtml` | Frontend | 未登入頁 layout，登入頁使用。 |
| `HQBackSite/Views/Shared/_Pager.cshtml` | Frontend | 共用分頁 partial，搭配 `PagedListModel`/`PagerModel` 與 JS page 切換。 |
| `HQBackSite/Views/Shared/Error.cshtml` | Frontend | 新後台錯誤頁。 |
| `HQBackSite/Views/Account/Login.cshtml` | Frontend | 新後台登入頁，載入 `login.js`，POST `/account/login`。 |
| `HQBackSite/Views/Home/Index.cshtml` | Frontend | 新後台首頁，目前內容很少，主要作為登入後 landing。 |
| `HQBackSite/Views/Menu/_Menu.cshtml` | Frontend | 左側選單 partial，依 `MenuViewModel` 與 `data-menu-code` render。 |
| `HQBackSite/Views/Menu/_UserInfo.cshtml` | Frontend | 使用者資訊 partial。 |
| `HQBackSite/Views/Menu/Index.cshtml` | Frontend | 選單設定清單/搜尋入口，model `ParaModel`。 |
| `HQBackSite/Views/Menu/IndexAdd.cshtml` | Frontend | 選單設定新增 modal form。 |
| `HQBackSite/Views/Menu/IndexUpdate.cshtml` | Frontend | 選單設定編輯 modal form。 |
| `HQBackSite/Views/Menu/IndexQuery.cshtml` | Frontend | 選單設定查詢結果 partial。 |
| `HQBackSite/Views/Menu/Permission.cshtml` | Frontend | 選單權限設定頁，model `List<ParaModel>`，查使用者並分配/移除部門 code_name。 |
| `HQBackSite/Views/Menu/Category.cshtml` | Frontend | 選單類別設定主頁，開 modal 新增/更新並查詢分類。 |
| `HQBackSite/Views/Menu/CategoryAdd.cshtml` | Frontend | 類別新增 modal form。 |
| `HQBackSite/Views/Menu/CategoryUpdate.cshtml` | Frontend | 類別更新 modal form。 |
| `HQBackSite/Views/Menu/CategoryQuery.cshtml` | Frontend | 類別查詢結果 partial。 |
| `HQBackSite/Views/Menu/Content.cshtml` | Frontend | 內容設定主頁。 |
| `HQBackSite/Views/Menu/ContentAdd.cshtml` | Frontend | 內容新增 modal form，整合 CKEditor 與上傳。 |
| `HQBackSite/Views/Menu/ContentUpdate.cshtml` | Frontend | 內容更新 modal form，整合 CKEditor 與上傳。 |
| `HQBackSite/Views/Menu/ContentQuery.cshtml` | Frontend | 內容查詢結果 partial。 |
| `HQBackSite/Views/Menu/Message.cshtml` | Frontend | 訊息設定主頁。 |
| `HQBackSite/Views/Menu/MessageAdd.cshtml` | Frontend | 訊息新增 modal form，整合 CKEditor。 |
| `HQBackSite/Views/Menu/MessageUpdate.cshtml` | Frontend | 訊息更新 modal form，整合 CKEditor。 |
| `HQBackSite/Views/Menu/MessageQuery.cshtml` | Frontend | 訊息查詢結果 partial。 |
| `HQBackSite/Views/Menu/ICS.cshtml` | Frontend | 國際認證規範公告主頁。 |
| `HQBackSite/Views/Menu/ICSAdd.cshtml` | Frontend | ICS 新增 modal form，支援資安/ISO 不同明細 JSON。 |
| `HQBackSite/Views/Menu/ICSUpdate.cshtml` | Frontend | ICS 更新 modal form，支援明細更新、新增、軟刪。 |
| `HQBackSite/Views/Menu/ICSQuery.cshtml` | Frontend | ICS 查詢結果 partial。 |
| `HQBackSite/Views/Menu/WordHtmlTest.cshtml` | Frontend | Word 轉 HTML 測試頁，POST 到 `WordHtmlConvert` 並開 preview。 |

## HQBackSite JavaScript / CSS

| 檔案 | 分類 | 分析 |
| --- | --- | --- |
| `HQBackSite/Scripts/lib.js` | Frontend | 新後台共用 JS：modal 載入、AJAX submit rebind、日期格式、loading、`libs.api.get/post`、結果處理。 |
| `HQBackSite/Scripts/login.js` | Frontend | 登入頁 JS，POST `/account/login`，成功後導向 `/Home/Index`。 |
| `HQBackSite/Scripts/menu.js` | Frontend | 左側選單權限控制，呼叫 `/Menu/GetUserMenuPermissions` 後顯示/隱藏 `data-menu-code`。 |
| `HQBackSite/Scripts/content.js` | Frontend | 內容/訊息/ICS 表單上傳輔助，依 URL/檔案模式切換欄位，透過 `/Menu/UploadFileForUrl` 上傳檔案/圖片。 |
| `HQBackSite/Scripts/lib.ckeditor.js` | Frontend | CKEditor 初始化與 HTML 正規化，處理相對路徑、圖片、連結、table style、source textarea。 |
| `HQBackSite/Scripts/jquery.js` | Vendor/Generated | jQuery。 |
| `HQBackSite/Scripts/bootstrap.js` | Vendor/Generated | Bootstrap JS。 |
| `HQBackSite/Content/Site.css` | Frontend | 新後台 sidebar、table、modal、search、pagination、login、UEditor/CKEditor 樣式。 |
| `HQBackSite/Content/bootstrap.css` | Vendor/Generated | Bootstrap CSS。 |

## Tests

| 檔案 | 分類 | 分析 |
| --- | --- | --- |
| `tests/HQBackSite.Tests/HQBackSite.Tests.csproj` | Tests | SDK-style MSTest net472 專案，reference `HQBackSite.csproj` 與 MVC assemblies。 |
| `tests/HQBackSite.Tests/WordHtmlConverterTests.cs` | Tests | 測 Word HTML converter 的圖片、table span、base64 image、highlight/shading、一般 link、PDF link rewrite。 |
| `tests/HQBackSite.Tests/MenuControllerWordHtmlTests.cs` | Tests | 測 `MenuController` WordHtmlTest/Convert/Preview 的無檔、錯副檔名、成功轉換、找不到 preview、安全 file pattern。 |
| `tests/HQBackSite.Tests/MenuControllerContentTests.cs` | Tests | 測新版後台內容設定：Content view/query/categories/add/update/upload validation 與 execute 行為。 |
| `tests/HQBackSite.Tests/MenuControllerIcsTests.cs` | Tests | 測新版後台 ICS：view/query/add/update、資安/ISO 明細資料新增更新軟刪。 |

## 非程式或需避免誤修改的區域

| 路徑 | 分類 | 分析 |
| --- | --- | --- |
| `packages/` | Vendor/Generated | NuGet packages restore 產物。 |
| `node_modules/` | Vendor/Generated | Node packages。 |
| `HQBackSite/Views/GeneratedHtml/` | Vendor/Generated | WordHtmlConvert 產出的靜態 HTML preview；目前 untracked，不應混入文件/程式 commit。 |
| `HQ/userfiles/**` | Vendor/Generated | 使用者上傳與歷史內容；修改前要確認是否為資料產物。 |
| `HQ/images/**`、`HQ/Views/HQ/images/**` | Vendor/Generated | 前台圖片素材；目前有大量 untracked `Thumbs.db`，不要納入 commit。 |
| `HQ/content_img/**` | Vendor/Generated | 內容圖片/媒體素材。 |
| `tmp/`、`TestResults/` | Vendor/Generated | 臨時檔與測試結果。 |
| `bin/`、`obj/` | Vendor/Generated | build output。 |

## 高風險修改提示

- 改 `HQ/Controllers/api/MainController.cs` 前，先查 `HQ/Scripts/Hq/Index.js` 與 `HQ/Scripts/Hq/Dept.js` 的回傳欄位預期。
- 改 `HQ/Controllers/api/DeptController.cs` 前，先查分店代碼 mapping 與 Newsletter/old webmax6 fallback。
- 改 `HQ/Controllers/api/ValuesController.cs` 前，優先處理或保護動態 SQL 來源。
- 改 `HQBackSite/Controllers/MenuController.cs` 前，先定位 action 對應 view、JS、model、測試。
- 改 auth/cookie/JWT 前，套用 `hq-auth-and-cookie-jwt` skill。
- 改 `para/news/content/unit` 或權限表資料流前，套用 `hq-sql-and-para-table` skill。
- 改 Word 轉 HTML 前，套用 `hqbacksite-word-html` skill 並跑 `HQBackSite.Tests`。
