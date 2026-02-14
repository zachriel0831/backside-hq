# HQ 專案說明文檔（含接口與啟動類）

> 掃描範圍：`master/HQ`、`master/HQBackSite`

## 1) 專案總覽

- `HQ`：前台網站 + 後台（舊版）+ Web API
- `HQBackSite`：後台管理站（新版）

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

---

## 6) 建議補充（後續可選）

若要當正式交接文件，建議下一版再加：
1. 每個 API 的 Request Body 範例（目前多使用 `Dictionary<string,string>`）
2. 主要資料表對照（`news`, `content`, `unit`, `page`, `para`）
3. 權限矩陣（哪類帳號可進入哪些後台功能）


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