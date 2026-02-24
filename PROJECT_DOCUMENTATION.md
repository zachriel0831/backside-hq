# HQ 專案說明文檔（含接口與啟動類）

> 掃描範圍：`master/HQ`、`master/HQBackSite`

## 1) 專案總覽

- `HQ`：前台網站 + 後台（舊版）+ Web API
- `HQBackSite`：後台管理站（新版）

### 前置作業（2026-02-22）

#### 跑馬燈資料寫入規則（Message）

- 功能位置：`HQBackSite/Controllers/MenuController.cs`
- 適用動作：
  - `POST /Menu/MessageAdd`
  - `POST /Menu/MessageUpdate`

`dept` 與 `type` 對應如下（含跑馬燈）：

```csharp
var deptMap = new Dictionary<string, string>
{
    { "案例分享", "msg1" },
    { "最新公告", "msg1" },
    { "安全新知", "msg1" },
    { "政策推動", "msg2" },
    { "熱門話題", "msg2" },
    { "職場生活", "msg2" },
    { "快樂員購", "msg2" },
    { "跑馬燈設定", "light" }
};
```

寫入 `news` 的 SQL（Insert）如下，`type` 由 `deptMap` 取得（跑馬燈會寫入 `light`）：

```sql
INSERT INTO news
(dept,background,descpt,priority,type,start_date,end_date,create_user)
VALUES
(@dept,@background,@descpt,@priority,@type,@start_date,@end_date,@account)
```

補充：`GET /Menu/MessageQuery` 已納入 `type in ('msg1','msg2','light')`，可正確查到「跑馬燈設定」資料。

---

## 2) 啟動類（Startup）與啟動流程

### HQ
- 啟動類：`HQ/Global.asax.cs`
- 類別：`WebApiApplication : HttpApplication`
- 啟動方法：`Application_Start()`
- 啟動流程：
  1. `AreaRegistration.RegisterAllAreas()`
  2. `GlobalConfiguration.Configure(WebApiConfig.Register)`（註冊 Web API）
  3. `FilterConfig.RegisterGlobalFilters(...)`
  4. `RouteConfig.RegisterRoutes(...)`
  5. `BundleConfig.RegisterBundles(...)`

### HQBackSite
- 啟動類：`HQBackSite/Global.asax.cs`
- 類別：`MvcApplication : HttpApplication`
- 啟動方法：`Application_Start()`
- 啟動流程：
  1. `AreaRegistration.RegisterAllAreas()`
  2. `FilterConfig.RegisterGlobalFilters(...)`
  3. `RouteConfig.RegisterRoutes(...)`
  4. `BundleConfig.RegisterBundles(...)`

---

## 3) 路由規則

### HQ MVC 路由
- 檔案：`HQ/App_Start/RouteConfig.cs`
- 規則：`{controller}/{action}/{id}`
- 預設：`controller=Hq`, `action=Index`

### HQ Web API 路由
- 檔案：`HQ/App_Start/WebApiConfig.cs`
- 規則：`api/{controller}/{action}/{id}`
- `id` 可選

### HQBackSite MVC 路由
- 檔案：`HQBackSite/App_Start/RouteConfig.cs`
- 規則：`{controller}/{action}/{id}`
- 預設：`controller=Home`, `action=Index`

---

## 4) 接口清單

> 說明：
> - MVC 未標註 `[HttpPost]` 的 Action，原則上可由 GET 呼叫（或依 IIS/前端使用方式）。
> - Web API 依 `api/{controller}/{action}/{id}` 組 URL。

## 4.1 HQ（MVC）

### `HomeController`
- `GET /Home/Index?id={id}`
- `GET /Home/FaxView`
- `GET /Home/System`
- `GET /Home/Restaurant`

### `HqController`
- `GET /Hq/Room1`
- `GET /Hq/Index`
- `GET /Hq/Dept`

### `BacksideController`（舊版後台）
- `GET /Backside/Index`
- `GET /Backside/Login`
- `POST /Backside/Login`
- `GET /Backside/Logout`
- `GET /Backside/Page`（`[Authorize]`）
- `GET /Backside/Unit`（`[Authorize]`）
- `GET /Backside/Content`（`[Authorize]`）
- `GET /Backside/News`（`[Authorize]`）
- `GET /Backside/Safe`（`[Authorize]`）
- `GET /Backside/Hr`（`[Authorize]`）
- `GET /Backside/Test`

## 4.2 HQ（Web API）

### `api/ContentController`
- `POST /api/Content/DeptSearch`
- `POST /api/Content/PageSearch`
- `POST /api/Content/SubtypeSearch`
- `POST /api/Content/ContentRead`
- `POST /api/Content/ContentCreate`
- `POST /api/Content/ContentUpdate`
- `POST /api/Content/ContentDelete`

### `api/DeptController`
- `POST /api/Dept/LetterData`
- `POST /api/Dept/NewData`
- `POST /api/Dept/DeptNewData`

### `api/MainController`
- `POST /api/Main/HtmlScraper`
- `POST /api/Main/Read`
- `POST /api/Main/WebConnectData`
- `POST /api/Main/SafeRead`
- `POST /api/Main/HrRead`
- `POST /api/Main/HrData`
- `POST /api/Main/ItData`
- `POST /api/Main/Count`
- `POST /api/Main/BannerData`
- `GET|POST /api/Main/ReleaseAllCache`

> 補充：`CountUpdate`、`CountInst` 在程式中為 `public` 且無 `[HttpPost]`，屬內部流程呼叫，文件不建議當正式對外接口使用。

### `api/NewsController`
- `POST /api/News/NewsRead`
- `POST /api/News/NewsCreate`
- `POST /api/News/NewsUpdate`
- `POST /api/News/NewsDelete`

### `api/PageController`
- `POST /api/Page/DeptSearch`
- `POST /api/Page/PageSearch`
- `POST /api/Page/PageRead`
- `POST /api/Page/PageCreate`
- `POST /api/Page/PageUpdate`
- `POST /api/Page/PageDelete`

### `api/RoomController`
- `POST /api/Room/Read`
- `POST /api/Room/Create`
- `POST /api/Room/Update`
- `POST /api/Room/Deleted`

### `api/UnitController`
- `POST /api/Unit/StyleSearch`
- `POST /api/Unit/DeptSearch`
- `POST /api/Unit/PageSearch`
- `POST /api/Unit/SubtypeSearch`
- `POST /api/Unit/UnitRead`
- `POST /api/Unit/UnitCreate`
- `POST /api/Unit/UnitUpdate`
- `POST /api/Unit/UnitDelete`

### `api/ValuesController`
- `POST /api/Values/Read`
- `POST /api/Values/Search`

---

## 4.3 HQBackSite（MVC）

### `AccountController`
- `GET /Account/Login`
- `POST /Account/Login`
- `GET /Account/Logout`

### `HomeController`（`[BackSiteAuthorize]`）
- `GET /Home/Index`
- `GET /Home/Error`

### `MenuController`（`[BackSiteAuthorize]`）

#### 選單設定
- `GET /Menu/Index`
- `POST /Menu/IndexQuery`
- `GET /Menu/IndexAdd`
- `POST /Menu/IndexAdd`
- `GET /Menu/IndexUpdate`
- `POST /Menu/IndexUpdate`
- `POST /Menu/IndexRemove`

#### 權限設定
- `GET /Menu/Permission`
- `GET /Menu/PermissionQueryUser`
- `GET /Menu/PermissionQueryDeptUser`
- `POST /Menu/PermissionAssign`
- `POST /Menu/PermissionRemove`

#### 類別設定
- `GET /Menu/Category`
- `POST /Menu/CategoryQuery`
- `GET /Menu/CategoryAdd`
- `POST /Menu/CategoryAdd`
- `GET /Menu/CategoryUpdate`
- `POST /Menu/CategoryUpdate`

#### 內容設定
- `GET /Menu/Content`
- `GET /Menu/ContentQuery`
- `GET /Menu/GetCategories`
- `GET /Menu/ContentAdd`
- `POST /Menu/ContentAdd`
- `GET /Menu/ContentUpdate`
- `POST /Menu/ContentUpdate`
- `POST /Menu/UploadFileForUrl`

#### 訊息設定
- `GET /Menu/Message`
- `GET /Menu/MessageQuery`
- `GET /Menu/MessageAdd`
- `POST /Menu/MessageAdd`
- `GET /Menu/MessageUpdate`
- `POST /Menu/MessageUpdate`

#### 國際認證規範公告（ICS）
- `GET /Menu/ICS`
- `GET /Menu/ICSQuery`
- `GET /Menu/ICSAdd`
- `POST /Menu/ICSAdd`
- `GET /Menu/ICSUpdate`
- `POST /Menu/ICSUpdate`

#### 版面子元件（非對外 URL）
- `GetUserInfo`（`[ChildActionOnly]`）
- `GetMenu`（`[ChildActionOnly]`）

---

## 5) 授權與存取控制重點

- `HQBackSite/Attributes/BackSiteAuthorizeAttribute.cs`
  - 透過 Cookie `Authorization` + JWT 解密驗證
  - 會回查 `EpSqlServer` 的 `users` 資料
  - 未授權時：
    - Ajax 回 JSON `{ code=-1, message="請先登入" }`
    - 非 Ajax 轉導 `Account/Login`

- `HQ/Controllers/BacksideController.cs`
  - `Page/Unit/Content/News/Safe/Hr` 使用 `[Authorize]`

### 全域操作日誌（AOP / ActionFilter）

- 檔案：`HQBackSite/Attributes/OperationLogAttribute.cs`
- 註冊：`HQBackSite/App_Start/FilterConfig.cs`
- 作用：所有 MVC Action 入口皆會在執行前後輸出操作日誌（`Trace` / `Debug`）
  - `OnActionExecuting`：記錄 controller、action、method、url、query、form、action args
  - `OnActionExecuted`：記錄耗時、status code、result type、是否例外

#### 脫敏規則（已啟用）

以下關鍵字欄位會自動遮罩為 `***`（大小寫不敏感）：

- 密碼類：`password`, `pwd`
- 驗證類：`token`, `secret`, `authorization`, `cookie`
- 帳戶識別類：`account`, `userid`, `user_id`, `username`, `employeeid`, `employee_id`, `emid`

> 備註：`GlobalExceptionFilterAttribute` 也已同步套用帳戶/密碼脫敏規則，避免例外日誌洩漏敏感資訊。

---

## 6) 建議補充（後續可選）

若要當正式交接文件，建議下一版再加：
1. 每個 API 的 Request Body 範例（目前多使用 `Dictionary<string,string>`）
2. 主要資料表對照（`news`, `content`, `unit`, `page`, `para`）
3. 權限矩陣（哪類帳號可進入哪些後台功能）


---

## 7) 測試說明（2026-02-22 更新）

### 7.1 ICS（國際認證規範公告）單元測試覆蓋範圍

本次已針對以下路由完成「新刪修查」相關單元測試：

- `GET /Menu/ICS`
- `GET /Menu/ICSQuery`
- `GET /Menu/ICSAdd`
- `POST /Menu/ICSAdd`
- `GET /Menu/ICSUpdate`
- `POST /Menu/ICSUpdate`

對應測試檔：

- `tests/HQBackSite.Tests/MenuControllerIcsTests.cs`

測試案例總數：`9`

- 查詢/頁面回傳：`ICS`, `ICSQuery`, `ICSAdd`, `ICSUpdate(GET)`
- 新增：`ICSAdd(POST)` 成功與失敗（未選擇訊息類別）
- 更新：`ICSUpdate(POST)` 成功與失敗（未選擇訊息類別、不同類別分支處理）


### 7.2 測試專案與解決方案設定

已新增測試專案：

- `tests/HQBackSite.Tests/HQBackSite.Tests.csproj`

並加入 `HQ.sln`：

- `HQBackSite.Tests`

目前測試專案關鍵依賴：

- `Microsoft.NET.Test.Sdk`
- `MSTest.TestAdapter`
- `MSTest.TestFramework`
- `System.Web.Mvc` / `System.Web.WebPages` / `System.Web.Razor`（針對 MVC5 Controller 測試）


### 7.3 執行方式與結果

由於本專案為 ASP.NET MVC (.NET Framework) 舊版 Web 專案，建議使用 VS2022 的 `MSBuild.exe` + `vstest.console.exe`：

```cmd
cmd /c ""C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" "d:\work_space\新光三越\20260203 新版HQ 接手剩下頁面\master\tests\HQBackSite.Tests\HQBackSite.Tests.csproj" /t:Build /p:Configuration=Debug /v:q && "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "d:\work_space\新光三越\20260203 新版HQ 接手剩下頁面\master\tests\HQBackSite.Tests\bin\Debug\net472\HQBackSite.Tests.dll""
```

執行結果：

- `Total tests: 9`
- `Passed: 9`
- `Failed: 0`


### 7.4 已知事項

- 建置過程中可見 `MSB3277`（組件版本衝突）警告，目前不影響本次 ICS 測試通過。
- 若後續要降低警告，建議統一測試專案與主專案的 `System.*` 相關套件版本。


### 7.5 內容設定（Content）單元測試覆蓋範圍（2026-02-22 新增）

本次依照同樣模式補上以下路由單元測試：

- `GET /Menu/Content`
- `GET /Menu/ContentQuery`
- `GET /Menu/GetCategories`
- `GET /Menu/ContentAdd`
- `POST /Menu/ContentAdd`
- `GET /Menu/ContentUpdate`
- `POST /Menu/ContentUpdate`
- `POST /Menu/UploadFileForUrl`

對應測試檔：

- `tests/HQBackSite.Tests/MenuControllerContentTests.cs`

Content 測試案例數：`12`

- 頁面與查詢：`Content`, `ContentQuery`, `GetCategories`
- 新增：`ContentAdd(GET/POST)`（含必填檢核、`d_dw` 需 URL 檢核、成功新增）
- 更新：`ContentUpdate(GET/POST)`（含必填檢核、成功更新）
- 上傳：`UploadFileForUrl(POST)`（無檔案時失敗路徑）

本次整體測試集（ICS + Content）執行結果：

- `Total tests: 21`
- `Passed: 21`
- `Failed: 0`


## 資料庫 DDL

### SQL Server 版本
```sql
CREATE TABLE [portal].[dbo].[ics_upload] (
    [updateID] INT IDENTITY(1,1) NOT NULL,
    [desno] INT NULL,
    [file_name] NVARCHAR(400) NULL,
    [file_url] NVARCHAR(400) NULL,
    [file_content_type] VARCHAR(50) NULL,
    [deleted] INT NOT NULL DEFAULT (0),
    [create_date] DATETIME NOT NULL DEFAULT (getdate()),
    [create_user] NVARCHAR(20) NULL,
    
    CONSTRAINT [PK_ics_upload] PRIMARY KEY CLUSTERED ([updateID] ASC)
) ON [PRIMARY];

CREATE TABLE [portal].[dbo].[ics_list] (
    [listID] INT IDENTITY(1,1) NOT NULL,
    [desno] INT NULL,
    [category] NVARCHAR(20) NULL,
    [doc_no] NVARCHAR(50) NULL,
    [doc_name] NVARCHAR(50) NULL,
    [doc_url] NVARCHAR(200) NULL,
    [attachment_info] NVARCHAR(200) NULL,
    [main_user] NVARCHAR(20) NULL,
    [remark] NVARCHAR(200) NULL,
    [deleted] INT NOT NULL DEFAULT (0),
    [create_date] DATETIME NOT NULL DEFAULT (getdate()),
    [create_user] NVARCHAR(20) NULL,
    
    CONSTRAINT [PK_ics_list] PRIMARY KEY CLUSTERED ([listID] ASC)
) ON [PRIMARY];

CREATE TABLE [portal].[dbo].[para_permission] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [emid] VARCHAR(8) NULL,
    [code_name] VARCHAR(30) NULL,
    [deleted] INT NOT NULL DEFAULT (0),
    [create_date] DATETIME NOT NULL DEFAULT (getdate()),
    [create_user] NVARCHAR(20) NULL,
    
    CONSTRAINT [PK_para_permission] PRIMARY KEY CLUSTERED ([id] ASC)
) ON [PRIMARY];
```



USE [Portal]
GO

/****** Object:  Table [dbo].[para]    Script Date: 2026/2/13 下午 06:41:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[para](
	[type] [char](4) NOT NULL,
	[type_name] [varchar](20) NOT NULL,
	[code] [varchar](10) NOT NULL,
	[code_name] [varchar](30) NULL,
	[data1] [varchar](50) NULL,
	[data2] [varchar](50) NULL,
	[data3] [varchar](50) NULL,
	[data4] [varchar](50) NULL,
	[data5] [varchar](50) NULL,
	[data6] [varchar](50) NULL,
	[create_user] [varchar](20) NULL,
	[create_date] [datetime] NULL,
	[update_user] [varchar](20) NULL,
	[update_date] [datetime] NULL,
 CONSTRAINT [PK_para] PRIMARY KEY CLUSTERED 
(
	[type] ASC,
	[type_name] ASC,
	[code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

USE [Portal]
GO

/****** Object:  Table [dbo].[News]    Script Date: 2026/2/13 下午 08:25:43 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[News](
	[des_no] [int] IDENTITY(1,1) NOT NULL,
	[dept] [varchar](50) NULL,
	[descpt] [text] NULL,
	[urlpath] [text] NULL,
	[background] [varchar](100) NULL,
	[priority] [int] NULL,
	[type] [varchar](20) NULL,
	[start_date] [datetime] NULL,
	[end_date] [datetime] NULL,
	[create_user] [varchar](20) NULL,
	[create_date] [datetime] NULL,
	[update_date] [datetime] NULL,
	[back_type] [int] NULL,
 CONSTRAINT [PK_News] PRIMARY KEY CLUSTERED 
(
	[des_no] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


USE [Portal]
GO

/****** Object:  Table [dbo].[PAGE]    Script Date: 2026/2/13 下午 08:26:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PAGE](
	[page] [varchar](4) NOT NULL,
	[dept] [varchar](50) NOT NULL,
	[title] [varchar](50) NULL,
	[url] [varchar](200) NULL,
	[script] [varchar](100) NULL,
	[is_show] [char](1) NULL,
	[credit_date] [datetime] NULL,
 CONSTRAINT [PK_PAGE] PRIMARY KEY CLUSTERED 
(
	[page] ASC,
	[dept] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


USE [Portal]
GO

/****** Object:  Table [dbo].[UNIT]    Script Date: 2026/2/13 下午 08:28:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[UNIT](
	[page] [varchar](4) NOT NULL,
	[dept] [varchar](50) NOT NULL,
	[subtype] [varchar](50) NOT NULL,
	[unit_title] [varchar](50) NULL,
	[style] [varchar](20) NULL,
	[include_file] [varchar](100) NULL,
	[unit_height] [int] NULL,
	[unit_weight] [int] NULL,
	[title_pic] [varchar](100) NULL,
	[title_pic_att] [varchar](50) NULL,
	[vl_line] [varchar](100) NULL,
	[vr_line] [varchar](100) NULL,
	[h_line] [varchar](100) NULL,
	[left_up_line] [varchar](100) NULL,
	[right_up_line] [varchar](100) NULL,
	[left_down_line] [varchar](100) NULL,
	[right_down_line] [varchar](100) NULL,
	[bg_color] [varchar](100) NULL,
	[subject_len] [int] NULL,
	[priority] [int] NULL,
	[need_cr] [varchar](20) NULL,
	[is_show] [char](1) NULL,
	[create_date] [datetime] NULL,
 CONSTRAINT [PK_UNIT] PRIMARY KEY CLUSTERED 
(
	[page] ASC,
	[dept] ASC,
	[subtype] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[content](
	[page] [varchar](4) NOT NULL,
	[dept] [varchar](50) NOT NULL,
	[subtype] [varchar](50) NOT NULL,
	[subject_id] [int] IDENTITY(1,1) NOT NULL,
	[priority] [int] NULL,
	[subject] [varchar](200) NULL,
	[url] [varchar](200) NULL,
	[content] [text] NULL,
	[is_show] [char](1) NULL,
	[create_date] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


CREATE TABLE [dbo].[USERS](
	[ALTER_DATE] [varchar](8) NULL,
	[ALTER_TIME] [varchar](6) NULL,
	[COMMENT] [ntext] NULL,
	[CREATE_DATE] [char](8) NULL,
	[CREATE_TIME] [varchar](6) NULL,
	[EMAIL] [varchar](100) NULL,
	[EMPLOYEE_ID] [varchar](30) NULL,
	[ENG_NAME] [varchar](100) NULL,
	[EXPIRED_DATE] [char](8) NULL,
	[IS_LOCKEDOUT] [char](1) NOT NULL,
	[LAST_LOCKOUT_DATE] [char](14) NULL,
	[LAST_LOGIN_TIME] [char](14) NULL,
	[LAST_PASSWORD_CHANGED_TIME] [char](14) NULL,
	[LOCAL_NAME] [nvarchar](100) NULL,
	[ORGAN_ID] [char](8) NULL,
	[PASSWORD] [varchar](100) NULL,
	[STATUS] [char](1) NULL,
	[TEL_NO] [varchar](30) NULL,
	[TRY_COUNTER] [int] NOT NULL,
	[USER_ID] [varchar](30) NOT NULL,
	[USER_IDENTITY] [varchar](30) NULL,
	[AUTH_TYPE] [char](1) NULL,
	[TITLE] [nvarchar](100) NULL,
	[DEPARTMENT] [nvarchar](100) NULL,
	[LOGIN_SCRIPT] [nvarchar](200) NULL,
	[FAMILY_NAME] [nvarchar](50) NULL,
	[FIRST_NAME] [nvarchar](50) NULL,
 CONSTRAINT [PK_USERS] PRIMARY KEY CLUSTERED 
(
	[USER_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO



1. 訊息公告和國際認證規範
2. 前台表格畫面
3. user操作LOG(option)