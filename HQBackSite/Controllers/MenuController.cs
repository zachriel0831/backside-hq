using HQBackSite.Attributes;
using HQBackSite.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace HQBackSite.Controllers
{
    [BackSiteAuthorize]
    public class MenuController : BaseController
    {
        #region Testable Wrappers

        protected virtual string GetCurrentAccount()
        {
            return GetAccountFromCookie();
        }

        protected virtual string GetFormValue(string key)
        {
            return Request?.Form?[key];
        }

        protected virtual int ExecuteInternal(string sql, object param = null, ConnectionStringName connectionStringName = ConnectionStringName.SqlServer)
        {
            return Execute(sql, param, connectionStringName);
        }

        protected virtual List<T> QueryInternal<T>(string sql, object param = null, ConnectionStringName connectionStringName = ConnectionStringName.SqlServer)
        {
            return Query<T>(sql, param, connectionStringName);
        }

        protected virtual T QuerySingleInternal<T>(string sql, object param = null, ConnectionStringName connectionStringName = ConnectionStringName.SqlServer)
        {
            return QuerySingle<T>(sql, param, connectionStringName);
        }

        protected virtual List<string> GetUserCodeNamesInternal()
        {
            return GetUserCodeNames();
        }

        #endregion

        #region 選單設定

        public ActionResult Index()
        {
            return View(new ParaModel());
        }

        [HttpPost]
        public ActionResult IndexQuery(ParaModel request)
        {
            var sql = @"
DECLARE @SkipRows INT

SET @SkipRows = (@PageNo - 1) * @PageSize

SELECT TB.*
      ,@PageNo AS PageNo
      ,@PageSize AS PageSize
      ,COUNT(1) OVER() AS TotalCount
FROM
(
    SELECT [type_name], code, code_name, data1, data2, data6
    FROM dbo.para WITH(NOLOCK)
    WHERE [type] = '1050' 
      AND [type_name] IN (N'部門網頁', N'分店網頁')
      AND (@data1 IS NULL OR @data1 = '' OR data1 LIKE '%' + @data1 + '%')
      AND (@data2 IS NULL OR @data2 = '' OR data2 LIKE '%' + @data2 + '%')
) AS TB
ORDER BY 
    CASE WHEN TB.[type_name] = N'部門網頁' THEN 1 ELSE 2 END,
    CASE WHEN TB.[type_name] = N'部門網頁' THEN TB.code ELSE TB.code_name END
OFFSET @SkipRows ROWS 
FETCH NEXT @PageSize ROWS ONLY
";

            var list = Query<ParaModel>(sql, new
            {
                request.PageNo,
                request.PageSize,
                request.data1,
                request.data2
            });
            
            int totalCount = 0;
            if (list.Count > 0)
            {
                totalCount = list[0].TotalCount;
            }

            var pagedList = new PagedListModel<ParaModel>(list, request.PageNo, request.PageSize, totalCount);
            ViewBag.QueryModel = request;
            
            return PartialView("IndexQuery", pagedList);
        }

        public ActionResult IndexAdd()
        {
            return View(new ParaModel());
        }

        [HttpPost]
        public ActionResult IndexAdd(ParaModel request)
        {
            if (string.IsNullOrWhiteSpace(request.type_name))
            {
                return Json(Fail("請選類型"));
            }
            if (string.IsNullOrWhiteSpace(request.code))
            {
                return Json(Fail("請輸入部門編號"));
            }
            if (string.IsNullOrWhiteSpace(request.code_name))
            {
                return Json(Fail("請輸入部門代號"));
            }
            if (string.IsNullOrWhiteSpace(request.data1))
            {
                return Json(Fail("請輸入部門名稱"));
            }

            var checkSql = @"
SELECT [type_name], code, code_name, data1, data2 data6
FROM dbo.para WITH(NOLOCK)
WHERE [type] = '1050' 
AND [type_name] IN (N'部門網頁', N'分店網頁')
AND [code_name] = @code_name
";

            var checkRes = QuerySingle<ParaModel>(checkSql, new
            {
                request.code_name
            });
            if (checkRes != null)
            {
                return Json(Fail("部門代碼不可重複"));
            }

            var account = GetAccountFromCookie();

            var sql = @"
 INSERT INTO para (type, type_name, code, code_name, data1, data2, data6, create_user, create_date) 
 VALUES ('1050',@type_name,@code,@code_name,@data1,@data2,@data6,@account,getdate())";

            Execute(sql, new
            {
                request.type_name,
                request.code,
                request.code_name,
                request.data1,
                request.data2,
                request.data6,
                account,
            });

            return Json(Success());
        }

        public ActionResult IndexUpdate(string code_name)
        {
            var sql = @"
SELECT TOP 1 [type_name], code, code_name, data1, data2, data6
FROM dbo.para WITH(NOLOCK)
WHERE [type] = '1050' 
AND [type_name] IN (N'部門網頁', N'分店網頁')
AND code_name = @code_name
";

            var data = QuerySingle<ParaModel>(sql, new
            {
                code_name
            });

            return View(data);
        }

        [HttpPost]
        public ActionResult IndexUpdate(ParaModel request)
        {
            if (string.IsNullOrWhiteSpace(request.type_name))
            {
                return Json(Fail("請選類型"));
            }
            if (string.IsNullOrWhiteSpace(request.code))
            {
                return Json(Fail("請輸入部門編號"));
            }
            if (string.IsNullOrWhiteSpace(request.code_name))
            {
                return Json(Fail("請勿使用非法操作"));
            }
            if (string.IsNullOrWhiteSpace(request.data1))
            {
                return Json(Fail("請輸入部門名稱"));
            }

            var account = GetAccountFromCookie();

            var sql = @"
UPDATE para 
SET type_name = @type_name, code = @code, data1 = @data1, data2 = @data2, data6 = @data6, update_user = @account, update_date = getdate()
WHERE [type] = '1050' 
AND [type_name] IN (N'部門網頁', N'分店網頁')
AND code_name = @code_name";

            Execute(sql, new
            {
                request.code_name,
                request.type_name,
                request.code,
                request.data1,
                request.data2,
                request.data6,
                account,
            });

            return Json(Success());
        }

        [HttpPost]
        public ActionResult IndexRemove(ParaModel request)
        {
            if (string.IsNullOrWhiteSpace(request.code_name))
            {
                return Json(Fail("請勿使用非法操作"));
            }

            var sql = @"
UPDATE para 
SET type_name = type_name + '-刪除'
WHERE [type] = '1050' 
AND [type_name] IN (N'部門網頁', N'分店網頁')
AND code_name = @code_name";

            Execute(sql, new
            {
                request.code_name,
            });

            return Json(Success());
        }

        #endregion

        #region 選單權限設定

        public ActionResult Permission()
        {
            var sql = @"
SELECT [type_name], code_name, data1, data2 
FROM dbo.para WITH(NOLOCK)
WHERE [type] = '1050' 
AND [type_name] IN (N'部門網頁', N'分店網頁')
ORDER BY 
CASE WHEN [type_name] = N'部門網頁' THEN 1 ELSE 2 END,
CASE WHEN [type_name] = N'部門網頁' THEN data2 ELSE code_name END
";

            var list = Query<ParaModel>(sql);

            return View(list);
        }

        public ActionResult PermissionQueryUser(string emid)
        {
            var sql = @"SELECT TOP 1 ORGAN_ID, DEPARTMENT, EMPLOYEE_ID, LOCAL_NAME FROM dbo.users WITH(NOLOCK) WHERE EMPLOYEE_ID = @emid";

            var data = QuerySingle<UserModel>(sql, new { emid }, ConnectionStringName.EpSqlServer);

            if (data == null)
            {
                return Json(Fail("查無資料"));
            }
            else
            {
                return Json(SuccessData(data));
            }
        }

        public ActionResult PermissionQueryDeptUser(string code_name)
        {
            var assignSql = "SELECT * FROM dbo.para_permission WITH(NOLOCK) WHERE deleted = 0 AND code_name = @code_name";
            var assignRes = Query<ParaPermissionModel>(assignSql, new { code_name });
            if (assignRes == null || assignRes.Count == 0)
            {
                return Json(SuccessData(new List<UserModel>()));
            }

            var sql = @"SELECT ORGAN_ID, DEPARTMENT, EMPLOYEE_ID, LOCAL_NAME FROM dbo.users WITH(NOLOCK) WHERE EMPLOYEE_ID IN @EMPLOYEE_IDS";
            var ids = assignRes.Select(x => x.emid).ToList();
            var list = Query<UserModel>(sql, new { EMPLOYEE_IDS = ids }, ConnectionStringName.EpSqlServer);
            if (list == null || list.Count == 0)
            {
                return Json(SuccessData(new List<UserModel>()));
            }

            return Json(SuccessData(list));
        }

        [HttpPost]
        public ActionResult PermissionAssign(string emid, string code_name)
        {
            var existsSql = @"SELECT TOP 1 * FROM dbo.para_permission WITH(NOLOCK) WHERE deleted = 0 AND emid = @emid AND code_name = @code_name";

            var exists = QuerySingle<ParaPermissionModel>(existsSql,new { emid, code_name });
            if (exists != null)
            {
                return Json(Fail("權限已存在"));
            }

            var account = GetAccountFromCookie();

            var sql = @"
 INSERT INTO [dbo].[para_permission]
(
    [emid]
    ,[code_name]
    ,[deleted]
    ,[create_user]
)
VALUES
(
    @emid
    ,@code_name
    ,0
    ,@account
)";

            Execute(sql, new
            {
                emid,
                code_name,
                account,
            });

            return Json(Success("權限設定成功"));
        }

        [HttpPost]
        public ActionResult PermissionRemove(string emid, string code_name)
        {
            var sql = @"
UPDATE [dbo].[para_permission]
SET [deleted] = 1
WHERE emid = @emid 
AND code_name = @code_name";

            Execute(sql, new
            {
                emid,
                code_name
            });

            return Json(Success("權限移除成功"));
        }

        #endregion

        #region 選單類別設定

        public ActionResult Category()
        {
            var codeNames = GetUserCodeNames();

            var sql = @"
SELECT [type_name], code_name, data1, data2 
FROM dbo.para WITH(NOLOCK)
WHERE [type] = '1050' 
AND [type_name] IN (N'部門網頁', N'分店網頁')
AND code_name in @codeNames
ORDER BY 
CASE WHEN [type_name] = N'部門網頁' THEN 1 ELSE 2 END,
CASE WHEN [type_name] = N'部門網頁' THEN data2 ELSE code_name END
";
            ViewData["Paras"] =  Query<ParaModel>(sql, new { codeNames = codeNames });
             
            return View(new ContentModel { is_show = "Y" });
        }

        [HttpPost]
        public ActionResult CategoryQuery(ContentModel request)
        {
            var codeNames = GetUserCodeNames();

            var sql = @"
DECLARE @SkipRows INT

SET @SkipRows = (@PageNo - 1) * @PageSize

SELECT TB.*
      ,@PageNo AS PageNo
      ,@PageSize AS PageSize
      ,COUNT(1) OVER() AS TotalCount
FROM
(
      SELECT c.*,p.[type_name],p.code,p.data1,p.data2,p.code_name
      FROM dbo.content AS c WITH(NOLOCK) 
      LEFT JOIN dbo.para AS p WITH(NOLOCK) 
      ON p.code_name = c.dept
      WHERE subtype = 'd_topbtn' 
      AND data1 IS NOT NULL
      AND (dept in @codeNames)
      AND (@dept IS NULL OR @dept = '' OR dept = @dept)
      AND (@is_show IS NULL OR @is_show = '' OR is_show = @is_show)
) AS TB
ORDER BY 
    CASE WHEN TB.[type_name] = N'部門網頁' THEN 1 ELSE 2 END,
    CASE WHEN TB.[type_name] = N'部門網頁' THEN TB.code ELSE TB.code_name END
OFFSET @SkipRows ROWS 
FETCH NEXT @PageSize ROWS ONLY
";

            var list = Query<ContentModel>(sql, new
            {
                request.PageNo,
                request.PageSize,
                codeNames,
                request.dept,
                request.is_show,
            });

            int totalCount = 0;
            if (list.Count > 0)
            {
                totalCount = list[0].TotalCount;
            }

            var pagedList = new PagedListModel<ContentModel>(list, request.PageNo, request.PageSize, totalCount);
            ViewBag.QueryModel = request;

            return PartialView("CategoryQuery", pagedList);
        }

        public ActionResult CategoryAdd()
        {
            var codeNames = GetUserCodeNames();

            var sql = @"
SELECT [type_name], code_name, data1, data2 
FROM dbo.para WITH(NOLOCK)
WHERE [type] = '1050' 
AND [type_name] IN (N'部門網頁', N'分店網頁')
AND code_name in @codeNames
ORDER BY 
CASE WHEN [type_name] = N'部門網頁' THEN 1 ELSE 2 END,
CASE WHEN [type_name] = N'部門網頁' THEN data2 ELSE code_name END
";
            ViewData["Paras"] = Query<ParaModel>(sql, new { codeNames = codeNames });

            return View(new ContentModel());
        }

        [HttpPost]
        public ActionResult CategoryAdd(ContentModel request)
        {
            if (string.IsNullOrWhiteSpace(request.dept))
            {
                return Json(Fail("請設定部門"));
            }
            if (string.IsNullOrWhiteSpace(request.subject))
            {
                return Json(Fail("請設定名稱"));
            }
            if (string.IsNullOrWhiteSpace(request.is_show))
            {
                return Json(Fail("請設定顯示"));
            }

            var sql = @"
DECLARE @page INT;

SELECT @page = ISNULL(MAX(page), 0) + 10
FROM content WITH (UPDLOCK, HOLDLOCK)
WHERE dept = @dept AND subtype = 'd_topbtn';

INSERT INTO content (page, dept, subtype, subject, url, content, is_show, create_date) 
VALUES (@page, @dept, 'd_topbtn', @subject, @url, @content, @is_show, getdate())";

            Execute(sql, new
            {
                request.dept,
                request.subject,
                request.url,
                request.content,
                request.is_show,
            });

            return Json(Success());
        }

        public ActionResult CategoryUpdate(int subject_id)
        {
            var codeNames = GetUserCodeNames();

            var sql = @"
SELECT [type_name], code_name, data1, data2 
FROM dbo.para WITH(NOLOCK)
WHERE [type] = '1050' 
AND [type_name] IN (N'部門網頁', N'分店網頁')
AND code_name in @codeNames
ORDER BY 
CASE WHEN [type_name] = N'部門網頁' THEN 1 ELSE 2 END,
CASE WHEN [type_name] = N'部門網頁' THEN data2 ELSE code_name END
";
            ViewData["Paras"] = Query<ParaModel>(sql, new { codeNames = codeNames });

            var contentSql = @"
SELECT *
FROM dbo.content WITH(NOLOCK)
WHERE subject_id = @subject_id";

            var data = QuerySingle<ContentModel>(contentSql, new
            {
                subject_id
            });

            return View(data);
        }

        [HttpPost]
        public ActionResult CategoryUpdate(ContentModel request)
        {
            if (string.IsNullOrWhiteSpace(request.dept))
            {
                return Json(Fail("請設定部門"));
            }
            if (string.IsNullOrWhiteSpace(request.subject))
            {
                return Json(Fail("請設定名稱"));
            }
            if (string.IsNullOrWhiteSpace(request.is_show))
            {
                return Json(Fail("請設定顯示"));
            }

            var sql = @"
UPDATE content 
SET dept = @dept, subject = @subject, is_show = @is_show
WHERE subject_id = @subject_id";

            Execute(sql, new
            {
                request.subject_id,
                request.dept,
                request.subject,
                request.is_show,
            });

            return Json(Success());
        }

        #endregion

        #region 內容設定

        public ActionResult Content()
        {
            var codeNames = GetUserCodeNamesInternal();

            var sql = @"
SELECT [type_name], code_name, data1, data2 
FROM dbo.para WITH(NOLOCK)
WHERE [type] = '1050' 
AND [type_name] IN (N'部門網頁', N'分店網頁')
AND code_name in @codeNames
ORDER BY 
CASE WHEN [type_name] = N'部門網頁' THEN 1 ELSE 2 END,
CASE WHEN [type_name] = N'部門網頁' THEN data2 ELSE code_name END
";
            ViewData["Paras"] = QueryInternal<ParaModel>(sql, new { codeNames = codeNames });

            return View(new ContentModel { is_show = "Y" });
        }

        public ActionResult ContentQuery(ContentModel request)
        {
            var codeNames = GetUserCodeNamesInternal();

            var sql = @"
DECLARE @SkipRows INT

SET @SkipRows = (@PageNo - 1) * @PageSize

SELECT TB.*
      ,@PageNo AS PageNo
      ,@PageSize AS PageSize
      ,COUNT(1) OVER() AS TotalCount
FROM
(
    SELECT c.page,c.dept,c.subtype,c.subject_id,c.subject,c.content, b.subject as csubject, p.code, p.code_name, p.data1, p.data2, p.type_name,c.is_show
    FROM content AS c WITH(NOLOCK)
    LEFT JOIN (SELECT * FROM dbo.content WITH(NOLOCK) WHERE subtype = 'd_topbtn') AS b
    ON b.page = c.page and b.dept = c.dept
    LEFT JOIN (SELECT * FROM dbo.para WITH(NOLOCK) WHERE [type] = '1050') AS p
    ON p.code_name = c.dept
    WHERE ISNULL(c.subject, '') != '' 
    AND ISNULL(p.data1, '') != ''
    AND c.subtype NOT IN ('d_topbtn','d_left')
    AND c.dept in @codeNames 
    AND (@dept IS NULL OR @dept = '' OR c.dept = @dept)
) AS TB
WHERE (@is_show IS NULL OR @is_show = '' OR TB.is_show = @is_show)
AND (@subject IS NULL OR @subject = '' OR TB.subject LIKE '%' + @subject + '%')
ORDER BY
    CASE WHEN TB.[type_name] = N'部門網頁' THEN 1 ELSE 2 END,
    CASE WHEN TB.[type_name] = N'部門網頁' THEN TB.code ELSE TB.code_name END,
    TB.page, TB.subtype, TB.subject
OFFSET @SkipRows ROWS 
FETCH NEXT @PageSize ROWS ONLY
";

            var list = QueryInternal<ContentModel>(sql, new
            {
                request.PageNo,
                request.PageSize,
                codeNames,
                request.subject,
                request.dept,
                request.is_show,
            });

            int totalCount = 0;
            if (list.Count > 0)
            {
                totalCount = list[0].TotalCount;
            }

            var pagedList = new PagedListModel<ContentModel>(list, request.PageNo, request.PageSize, totalCount);
            ViewBag.QueryModel = request;

            return PartialView("ContentQuery", pagedList);
        }

        public ActionResult GetCategories(string dept)
        {
            if (string.IsNullOrWhiteSpace(dept))
            {
                return Json(SuccessData(new List<ContentModel>()));
            }

            var sql = @"
SELECT c.page, c.subject
FROM dbo.content AS c WITH(NOLOCK) 
WHERE subtype = 'd_topbtn' 
AND dept = @dept
AND is_show = 'Y'
ORDER BY c.page
";
            var list = QueryInternal<ContentModel>(sql, new { dept });
            return Json(SuccessData(list));
        }

        public ActionResult ContentAdd()
        {
            var sql = @"
SELECT [type_name], code_name, data1, data2 
FROM dbo.para WITH(NOLOCK)
WHERE [type] = '1050' 
AND [type_name] IN (N'部門網頁', N'分店網頁')
AND code_name in @codeNames
ORDER BY 
CASE WHEN [type_name] = N'部門網頁' THEN 1 ELSE 2 END,
CASE WHEN [type_name] = N'部門網頁' THEN data2 ELSE code_name END
";
            ViewData["Paras"] = QueryInternal<ParaModel>(sql, new { codeNames = GetUserCodeNamesInternal() });

            return View(new ContentModel());
        }

        [HttpPost]
        public ActionResult ContentAdd(ContentModel request)
        {
            if (string.IsNullOrWhiteSpace(request.dept))
            {
                return Json(Fail("請設定部門"));
            }
            if (string.IsNullOrWhiteSpace(request.page))
            {
                return Json(Fail("請設定類別"));
            }
            if (string.IsNullOrWhiteSpace(request.subtype))
            {
                return Json(Fail("請設定顯示位置"));
            }
            if (string.IsNullOrWhiteSpace(request.subject))
            {
                return Json(Fail("請設定標題"));
            }
            if (string.IsNullOrWhiteSpace(request.is_show))
            {
                return Json(Fail("請設定顯示"));
            }
            if (request.subtype == "d_dw")
            {
                if (string.IsNullOrWhiteSpace(request.url))
                {
                    return Json(Fail("請設定URL連結"));
                }
            }

            var sql = @"
INSERT INTO content
(dept,page,subtype,subject,is_show,url,content)
VALUES 
(@dept,@page,@subtype,@subject,@is_show,@url,@content)
";
            ExecuteInternal(sql, new
            {
                request.dept,
                request.page,
                request.subtype,
                request.subject,
                request.is_show,
                request.url,
                request.content,
            });

            return Json(Success());
        }

        public ActionResult ContentUpdate(int subject_id)
        {
            var contentSql = @"
SELECT TOP 1 c.page,c.dept,c.subtype,c.subject_id,c.subject,c.url,c.content,b.subject as csubject,p.code,p.code_name,p.data1, p.data2,p.type_name,c.is_show
FROM content AS c WITH(NOLOCK)
LEFT JOIN (SELECT * FROM dbo.content WITH(NOLOCK) WHERE subtype = 'd_topbtn') AS b
ON b.page = c.page and b.dept = c.dept
LEFT JOIN (SELECT * FROM dbo.para WITH(NOLOCK) WHERE [type] = '1050') AS p
ON p.code_name = c.dept
WHERE c.subtype <> 'd_topbtn' AND c.subject_id = @subject_id";

            var data = QuerySingleInternal<ContentModel>(contentSql, new
            {
                subject_id
            });

            var sql = @"
SELECT [type_name], code_name, data1, data2 
FROM dbo.para WITH(NOLOCK)
WHERE [type] = '1050' 
AND [type_name] IN (N'部門網頁', N'分店網頁')
AND code_name in @codeNames
ORDER BY 
CASE WHEN [type_name] = N'部門網頁' THEN 1 ELSE 2 END,
CASE WHEN [type_name] = N'部門網頁' THEN data2 ELSE code_name END
";
            ViewData["Paras"] = QueryInternal<ParaModel>(sql, new { codeNames = GetUserCodeNamesInternal() });

            var csql = @"
SELECT c.page, c.subject
FROM dbo.content AS c WITH(NOLOCK) 
WHERE subtype = 'd_topbtn' 
AND (dept = @dept)
AND (is_show = 'Y')
";
            ViewData["Categorys"] = QueryInternal<ContentModel>(csql, new { dept = data.dept });

            return View(data);
        }

        [HttpPost]
        public ActionResult ContentUpdate(ContentModel request)
        {
            if (string.IsNullOrWhiteSpace(request.page))
            {
                return Json(Fail("請設定類別"));
            }
            if (string.IsNullOrWhiteSpace(request.subtype))
            {
                return Json(Fail("請設定顯示位置"));
            }
            if (string.IsNullOrWhiteSpace(request.subject))
            {
                return Json(Fail("請設定標題"));
            }
            if (string.IsNullOrWhiteSpace(request.is_show))
            {
                return Json(Fail("請設定顯示"));
            }
            if (request.subtype == "d_dw")
            {
                if (string.IsNullOrWhiteSpace(request.url))
                {
                    return Json(Fail("請設定URL連結"));
                }
            }
         
            var sql = @"
UPDATE content 
SET page = @page, subtype = @subtype, subject = @subject, is_show = @is_show, url = @url, content = @content
WHERE subject_id = @subject_id AND subtype <> 'd_topbtn'
";
            ExecuteInternal(sql, new
            {
                request.subject_id,
                request.page,
                request.subtype,
                request.subject,
                request.is_show,
                request.url,
                request.content,
            });

            return Json(Success());
        }

        [HttpPost]
        public ActionResult UploadFileForUrl()
        {
            try
            {
                if (Request.Files.Count == 0)
                {
                    return Json(Fail("沒有上傳文件"));
                }

                var file = Request.Files[0];
                if (file == null || file.ContentLength == 0)
                {
                    return Json(Fail("文件為空"));
                }

                // 檢查文件大小（50MB）
                var maxSize = 50 * 1024 * 1024; // 50MB
                if (file.ContentLength > maxSize)
                {
                    return Json(Fail("文件大小超過限制（最大50MB）"));
                }

                // 生成日期目錄（yyyyMMdd）
                var dateFolder = System.DateTime.Now.ToString("yyyyMMdd");
                
                // 生成簡短亂碼文件名（8位隨機字符）
                var randomString = System.Guid.NewGuid().ToString("N").Substring(0, 8);
                var originalExtension = System.IO.Path.GetExtension(file.FileName);
                var newFileName = randomString + originalExtension;

                // 保存路徑：images/{日期}/
                var savePath = $"~/images/{dateFolder}/";
                var physicalPath = Server.MapPath(savePath);

                // 確保目錄存在
                if (!System.IO.Directory.Exists(physicalPath))
                {
                    System.IO.Directory.CreateDirectory(physicalPath);
                }

                // 保存文件
                var fullPath = System.IO.Path.Combine(physicalPath, newFileName);
                file.SaveAs(fullPath);

                // 返回完整URL（包含當前運行的domain）
                var relativeUrl = $"images/{dateFolder}/{newFileName}";
                var fullUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}/{relativeUrl}";

                return Json(SuccessData(new { url = fullUrl }));
            }
            catch (System.Exception ex)
            {
                return Json(Fail($"上傳失敗：{ex.Message}"));
            }
        }

        #endregion

        #region 訊息設定

        public ActionResult Message()
        {
            return View(new NewsModel());
        }

        public ActionResult MessageQuery(NewsModel request)
        {
            var codeNames = GetUserCodeNames();
            var today = System.DateTime.Now.Date;

            var sql = @"
DECLARE @SkipRows INT
DECLARE @Today DATE = CAST(GETDATE() AS DATE)

SET @SkipRows = (@PageNo - 1) * @PageSize

;WITH TB AS
(
    SELECT *
          ,CASE 
               WHEN start_date IS NOT NULL AND end_date IS NOT NULL 
                    AND @Today >= CAST(start_date AS DATE) 
                    AND @Today <= CAST(end_date AS DATE) 
               THEN N'上線'
               ELSE N'下線'
           END AS status
    FROM news WITH (NOLOCK) 
    WHERE type in ('msg1','msg2') AND dept in (N'案例分享',N'最新公告',N'安全新知',N'政策推動',N'熱門話題',N'職場生活',N'快樂員購')
), Filtered AS
(
    SELECT TB.*
          ,ROW_NUMBER() OVER (ORDER BY create_date DESC) AS RowNum
          ,COUNT(1) OVER() AS TotalCount
    FROM TB
    WHERE 
        (@dept IS NULL OR @dept = '' OR TB.dept = @dept)
    AND 
        (@background IS NULL OR @background = '' OR background LIKE '%' + @background + '%')
    AND
        (@status IS NULL OR @status = '' OR TB.status = @status)
)
SELECT Filtered.*
      ,@PageNo AS PageNo
      ,@PageSize AS PageSize
FROM Filtered
WHERE RowNum > @SkipRows
  AND RowNum <= (@SkipRows + @PageSize)
ORDER BY RowNum
";

            var list = Query<NewsModel>(sql, new
            {
                request.PageNo,
                request.PageSize,
                request.dept,
                request.background,
                request.status
            });

            int totalCount = 0;
            if (list.Count > 0)
            {
                totalCount = list[0].TotalCount;
            }

            var pagedList = new PagedListModel<NewsModel>(list, request.PageNo, request.PageSize, totalCount);
            ViewBag.QueryModel = request;

            return PartialView("MessageQuery", pagedList);
        }

        public ActionResult MessageAdd()
        {
            return View(new NewsModel());
        }

        [HttpPost]
        public ActionResult MessageAdd(NewsModel request)
        {
            if (string.IsNullOrWhiteSpace(request.dept))
            {
                return Json(Fail("請設定訊息類別"));
            }
            if (string.IsNullOrWhiteSpace(request.background))
            {
                return Json(Fail("請設定標題"));
            }
            if (string.IsNullOrWhiteSpace(request.descpt))
            {
                return Json(Fail("請設定內容"));
            }
            if (request.start_date == null)
            {
                return Json(Fail("請設定起日"));
            }
            if (request.end_date == null)
            {
                return Json(Fail("請設定訖日"));
            }

            var deptMap = new Dictionary<string, string>
            {
                { "案例分享", "msg1" },
                { "最新公告", "msg1" },
                { "安全新知", "msg1" },
                { "政策推動", "msg2" },
                { "熱門話題", "msg2" },
                { "職場生活", "msg2" },
                { "快樂員購", "msg2" }
            };
            if (!deptMap.TryGetValue(request.dept, out var type))
            {
                return Json(Fail("訊息類別不正確"));
            }

            var account = GetAccountFromCookie();

            var sql = @"
INSERT INTO news
(dept,background,descpt,priority,type,start_date,end_date,create_user)
VALUES 
(@dept,@background,@descpt,@priority,@type,@start_date,@end_date,@account)
";
            Execute(sql, new
            {
                request.dept,
                request.background,
                request.descpt,
                request.priority,
                type,
                request.start_date,
                request.end_date,
                account
            });

            return Json(Success());
        }

        public ActionResult MessageUpdate(int des_no)
        {
            var sql = @"SELECT TOP 1 * FROM news WITH (NOLOCK) WHERE des_no = @des_no";

            var data = QuerySingle<NewsModel>(sql, new
            {
                des_no
            });

            return View(data);
        }

        [HttpPost]
        public ActionResult MessageUpdate(NewsModel request)
        {
            if (string.IsNullOrWhiteSpace(request.dept))
            {
                return Json(Fail("請設定訊息類別"));
            }
            if (string.IsNullOrWhiteSpace(request.background))
            {
                return Json(Fail("請設定標題"));
            }
            if (string.IsNullOrWhiteSpace(request.descpt))
            {
                return Json(Fail("請設定內容"));
            }
            if (request.start_date == null)
            {
                return Json(Fail("請設定起日"));
            }
            if (request.end_date == null)
            {
                return Json(Fail("請設定訖日"));
            }

            var deptMap = new Dictionary<string, string>
            {
                { "案例分享", "msg1" },
                { "最新公告", "msg1" },
                { "安全新知", "msg1" },
                { "政策推動", "msg2" },
                { "熱門話題", "msg2" },
                { "職場生活", "msg2" },
                { "快樂員購", "msg2" }
            };
            if (!deptMap.TryGetValue(request.dept, out var type))
            {
                return Json(Fail("訊息類別不正確"));
            }

            var account = GetAccountFromCookie();

            var sql = @"
UPDATE news 
SET dept=@dept,background=@background,descpt=@descpt,priority=@priority,type=@type,start_date=@start_date,end_date=@end_date
WHERE des_no = @des_no
";
            Execute(sql, new
            {
                request.des_no,
                request.dept,
                request.background,
                request.descpt,
                request.priority,
                type,
                request.start_date,
                request.end_date
            });

            return Json(Success());
        }

        #endregion

        #region 國際認證規範公告

        public ActionResult ICS()
        {
            return View(new NewsModel());
        }

        public ActionResult ICSQuery(NewsModel request)
        {
            var sql = @"
DECLARE @SkipRows INT
DECLARE @Today DATE = CAST(GETDATE() AS DATE)

SET @SkipRows = (@PageNo - 1) * @PageSize

;WITH TB AS
(
    SELECT *
          ,CASE 
               WHEN start_date IS NOT NULL AND end_date IS NOT NULL 
                    AND @Today >= CAST(start_date AS DATE) 
                    AND @Today <= CAST(end_date AS DATE)
               THEN N'上線'
               ELSE N'下線'
           END AS status
    FROM news WITH (NOLOCK) 
    WHERE type in ('ics') AND dept in (N'ISO文件',N'資安文件',N'上傳文件')
), Filtered AS
(
    SELECT TB.*
          ,ROW_NUMBER() OVER (ORDER BY create_date DESC) AS RowNum
          ,COUNT(1) OVER() AS TotalCount
    FROM TB
    WHERE 
        (@dept IS NULL OR @dept = '' OR TB.dept = @dept)
    AND 
        (@background IS NULL OR @background = '' OR background LIKE '%' + @background + '%')
    AND
        (@status IS NULL OR @status = '' OR TB.status = @status)
)
SELECT Filtered.*
      ,@PageNo AS PageNo
      ,@PageSize AS PageSize
FROM Filtered
WHERE RowNum > @SkipRows
  AND RowNum <= (@SkipRows + @PageSize)
ORDER BY RowNum
";

            var list = QueryInternal<NewsModel>(sql, new
            {
                request.PageNo,
                request.PageSize,
                request.dept,
                request.background,
                request.status
            });

            int totalCount = 0;
            if (list.Count > 0)
            {
                totalCount = list[0].TotalCount;
            }

            var pagedList = new PagedListModel<NewsModel>(list, request.PageNo, request.PageSize, totalCount);
            ViewBag.QueryModel = request;

            return PartialView("ICSQuery", pagedList);
        }

        public ActionResult ICSAdd()
        {
            return View(new NewsModel());
        }

        [HttpPost]
        public ActionResult ICSAdd(NewsModel request)
        {
            if (string.IsNullOrWhiteSpace(request.dept))
            {
                return Json(Fail("請選擇訊息類別"));
            }

            var account = GetCurrentAccount();
            var icsSecurityDataJson = GetFormValue("icsSecurityData");
            if (string.IsNullOrWhiteSpace(icsSecurityDataJson))
            {
                icsSecurityDataJson = request?.icsSecurityData;
            }
            var icsIsoDataJson = GetFormValue("icsIsoData");
            if (string.IsNullOrWhiteSpace(icsIsoDataJson))
            {
                icsIsoDataJson = request?.icsIsoData;
            }

            // 插入 news 資料
            var sql = @"
INSERT INTO news
(dept, background, priority, type, start_date, end_date, create_user, urlpath)
VALUES 
(@dept, @background, @priority, 'ics', @start_date, @end_date, @account, @urlpath);
SELECT CAST(SCOPE_IDENTITY() AS INT);
";
            var desNo = QuerySingleInternal<int>(sql, new
            {
                request.dept,
                request.background,
                request.priority,
                request.start_date,
                request.end_date,
                account,
                urlpath = request.urlpath ?? ""
            });

            // 處理資安文件或 ISO 文件
            if (request.dept == "資安文件" && !string.IsNullOrWhiteSpace(icsSecurityDataJson))
            {
                var icsSecurityData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(icsSecurityDataJson);
                if (icsSecurityData != null && icsSecurityData.Count > 0)
                {
                    foreach (var item in icsSecurityData)
                    {
                        var insertSql = @"
INSERT INTO ics_list
(DesNo, Category, doc_name, doc_url, create_user, create_date, Deleted)
VALUES
(@DesNo, @Category, @Content, @DocUrl, @CreateUser, GETDATE(), 0)
";
                        ExecuteInternal(insertSql, new
                        {
                            DesNo = desNo,
                            Category = item.Category?.ToString() ?? "",
                            Content = item.Content?.ToString() ?? "",
                            DocUrl = item.DocUrl?.ToString() ?? "",
                            CreateUser = account
                        });
                    }
                }
            }
            else if (request.dept == "ISO文件" && !string.IsNullOrWhiteSpace(icsIsoDataJson))
            {
                var icsIsoData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(icsIsoDataJson);
                if (icsIsoData != null && icsIsoData.Count > 0)
                {
                    foreach (var item in icsIsoData)
                    {
                        var insertSql = @"
INSERT INTO ics_list
(DesNo, Category, DocNo, DocName, DocUrl, AttachmentInfo, MainUser, Remark, CreateUser, CreateDate, Deleted)
VALUES
(@DesNo, @Category, @DocNo, @DocName, @DocUrl, @AttachmentInfo, @MainUser, @Remark, @CreateUser, GETDATE(), 0)
";
                        ExecuteInternal(insertSql, new
                        {
                            DesNo = desNo,
                            Category = item.SerialNumber?.ToString() ?? "",
                            DocNo = item.DocNo?.ToString() ?? "",
                            DocName = item.DocName?.ToString() ?? "",
                            DocUrl = item.DocUrl?.ToString() ?? "",
                            AttachmentInfo = item.AttachmentInfo?.ToString() ?? "",
                            MainUser = item.MainUser?.ToString() ?? "",
                            Remark = item.Remark?.ToString() ?? "",
                            CreateUser = account
                        });
                    }
                }
            }

            return Json(Success());
        }

        public ActionResult ICSUpdate(int des_no)
        {
            var data = QuerySingleInternal<NewsModel>(@"SELECT TOP 1 * FROM news WITH (NOLOCK) WHERE des_no = @des_no", new { des_no }) ?? new NewsModel();

            if (data.IcsGroup == null)
            {
                data.IcsGroup = new IcsGroupModel();
            }
            data.IcsGroup.list = QueryInternal<IcsListModel>(@"SELECT *, DesNo AS des_no FROM ics_list WITH (NOLOCK) WHERE DesNo = @des_no AND Deleted = 0", new { des_no }) ?? new List<IcsListModel>();
            data.IcsGroup.uploads = QueryInternal<IcsUploadModel>(@"SELECT *, DesNo AS des_no FROM ics_upload WITH (NOLOCK) WHERE DesNo = @des_no AND Deleted = 0", new { des_no }) ?? new List<IcsUploadModel>();

            return View(data);
        }

        [HttpPost]
        public ActionResult ICSUpdate(NewsModel request)
        {
            if (string.IsNullOrWhiteSpace(request.dept))
            {
                return Json(Fail("請選擇訊息類別"));
            }

            var account = GetCurrentAccount();
            var icsSecurityDataJson = GetFormValue("icsSecurityData");
            if (string.IsNullOrWhiteSpace(icsSecurityDataJson))
            {
                icsSecurityDataJson = request?.icsSecurityData;
            }
            var icsIsoDataJson = GetFormValue("icsIsoData");
            if (string.IsNullOrWhiteSpace(icsIsoDataJson))
            {
                icsIsoDataJson = request?.icsIsoData;
            }

            // 更新 news 資料
            var sql = @"
UPDATE news
SET dept = @dept,
    background = @background,
    priority = @priority,
    start_date = @start_date,
    end_date = @end_date,
    urlpath = @urlpath,
    update_date = GETDATE()
WHERE des_no = @des_no
";
            ExecuteInternal(sql, new
            {
                request.des_no,
                request.dept,
                request.background,
                request.priority,
                request.start_date,
                request.end_date,
                urlpath = request.urlpath ?? ""
            });

            // 處理資安文件或 ISO 文件
            if (request.dept == "資安文件" && !string.IsNullOrWhiteSpace(icsSecurityDataJson))
            {
                var icsSecurityData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(icsSecurityDataJson);
                if (icsSecurityData != null)
                {
                    // 先標記所有現有資料為刪除
                    ExecuteInternal(@"UPDATE ics_list SET Deleted = 1 WHERE DesNo = @DesNo", new { DesNo = request.des_no });

                    // 新增或更新資料
                    foreach (var item in icsSecurityData)
                    {
                        var listId = item.ListID != null ? (int)item.ListID : 0;
                        var deleted = item.Deleted != null ? (int)item.Deleted : 0;

                        if (deleted == 1)
                        {
                            // 標記為刪除
                            if (listId > 0)
                            {
                                ExecuteInternal(@"UPDATE ics_list SET Deleted = 1 WHERE ListID = @ListID", new { ListID = listId });
                            }
                        }
                        else if (listId > 0)
                        {
                            // 更新現有資料
                            var updateSql = @"
UPDATE ics_list
SET Category = @Category,
    DocName = @Content,
    DocUrl = @DocUrl
WHERE ListID = @ListID
";
                            ExecuteInternal(updateSql, new
                            {
                                ListID = listId,
                                Category = item.Category?.ToString() ?? "",
                                Content = item.Content?.ToString() ?? "",
                                DocUrl = item.DocUrl?.ToString() ?? ""
                            });
                        }
                        else
                        {
                            // 新增資料
                            var insertSql = @"
INSERT INTO ics_list
(DesNo, Category, DocName, DocUrl, CreateUser, CreateDate, Deleted)
VALUES
(@DesNo, @Category, @Content, @DocUrl, @CreateUser, GETDATE(), 0)
";
                            ExecuteInternal(insertSql, new
                            {
                                DesNo = request.des_no,
                                Category = item.Category?.ToString() ?? "",
                                Content = item.Content?.ToString() ?? "",
                                DocUrl = item.DocUrl?.ToString() ?? "",
                                CreateUser = account
                            });
                        }
                    }
                }
            }
            else if (request.dept == "ISO文件" && !string.IsNullOrWhiteSpace(icsIsoDataJson))
            {
                var icsIsoData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(icsIsoDataJson);
                if (icsIsoData != null)
                {
                    // 先標記所有現有資料為刪除
                    ExecuteInternal(@"UPDATE ics_list SET Deleted = 1 WHERE DesNo = @DesNo", new { DesNo = request.des_no });

                    // 新增或更新資料
                    foreach (var item in icsIsoData)
                    {
                        var listId = item.ListID != null ? (int)item.ListID : 0;
                        var deleted = item.Deleted != null ? (int)item.Deleted : 0;

                        if (deleted == 1)
                        {
                            // 標記為刪除
                            if (listId > 0)
                            {
                                ExecuteInternal(@"UPDATE ics_list SET Deleted = 1 WHERE ListID = @ListID", new { ListID = listId });
                            }
                        }
                        else if (listId > 0)
                        {
                            // 更新現有資料
                            var updateSql = @"
UPDATE ics_list
SET Category = @Category,
    DocNo = @DocNo,
    DocName = @DocName,
    DocUrl = @DocUrl,
    AttachmentInfo = @AttachmentInfo,
    MainUser = @MainUser,
    Remark = @Remark
WHERE ListID = @ListID
";
                            ExecuteInternal(updateSql, new
                            {
                                ListID = listId,
                                Category = item.SerialNumber?.ToString() ?? "",
                                DocNo = item.DocNo?.ToString() ?? "",
                                DocName = item.DocName?.ToString() ?? "",
                                DocUrl = item.DocUrl?.ToString() ?? "",
                                AttachmentInfo = item.AttachmentInfo?.ToString() ?? "",
                                MainUser = item.MainUser?.ToString() ?? "",
                                Remark = item.Remark?.ToString() ?? ""
                            });
                        }
                        else
                        {
                            // 新增資料
                            var insertSql = @"
INSERT INTO ics_list
(DesNo, Category, DocNo, DocName, DocUrl, AttachmentInfo, MainUser, Remark, CreateUser, CreateDate, Deleted)
VALUES
(@DesNo, @Category, @DocNo, @DocName, @DocUrl, @AttachmentInfo, @MainUser, @Remark, @CreateUser, GETDATE(), 0)
";
                            ExecuteInternal(insertSql, new
                            {
                                DesNo = request.des_no,
                                Category = item.SerialNumber?.ToString() ?? "",
                                DocNo = item.DocNo?.ToString() ?? "",
                                DocName = item.DocName?.ToString() ?? "",
                                DocUrl = item.DocUrl?.ToString() ?? "",
                                AttachmentInfo = item.AttachmentInfo?.ToString() ?? "",
                                MainUser = item.MainUser?.ToString() ?? "",
                                Remark = item.Remark?.ToString() ?? "",
                                CreateUser = account
                            });
                        }
                    }
                }
            }
            else
            {
                // 如果不是資安文件或 ISO 文件，刪除所有相關的 ics_list 資料
                ExecuteInternal(@"UPDATE ics_list SET Deleted = 1 WHERE DesNo = @DesNo", new { DesNo = request.des_no });
            }

            return Json(Success());
        }

        #endregion

        [ChildActionOnly]
        public ActionResult GetUserInfo()
        {
            try
            {
                var account = GetAccountFromCookie();
                if (string.IsNullOrWhiteSpace(account))
                {
                    return PartialView("_UserInfo", null);
                }

                var sql = @"SELECT TOP 1 ORGAN_ID, DEPARTMENT, EMPLOYEE_ID, LOCAL_NAME FROM dbo.users WITH(NOLOCK) WHERE EMPLOYEE_ID = @employeeId";
                var user = QuerySingle<UserModel>(sql, new { employeeId = account }, ConnectionStringName.EpSqlServer);

                return PartialView("_UserInfo", user);
            }
            catch
            {
                return PartialView("_UserInfo", null);
            }
        }

        [ChildActionOnly]
        public ActionResult GetMenu(string currentModule = null)
        {
            var menuGroups = new List<MenuGroupModel>();

            bool showSystemManagement = CheckUserDepartment();

            if (showSystemManagement)
            {
                menuGroups.Add(new MenuGroupModel
                {
                    Id = 1,
                    Name = "系統管理",
                    Items = new List<MenuItemModel>
                    {
                        new MenuItemModel
                        {
                            Id = 1,
                            Text = "選單設定",
                            ModuleId = "menuSetting",
                            Action = "Index",
                            Controller = "Menu"
                        },
                        new MenuItemModel
                        {
                            Id = 2,
                            Text = "選單權限設定",
                            ModuleId = "menuPermission",
                            Action = "Permission",
                            Controller = "Menu"
                        }
                    }
                });
            }
            menuGroups.Add(new MenuGroupModel
            {
                Id = 2,
                Name = "內容管理",
                Items = new List<MenuItemModel>
                {
                    new MenuItemModel
                    {
                        Id = 3,
                        Text = "選單類別設定",
                        ModuleId = "contentCategory",
                        Action = "Category",
                        Controller = "Menu"
                    },
                    new MenuItemModel
                    {
                        Id = 4,
                        Text = "內容設定",
                        ModuleId = "content",
                        Action = "Content",
                        Controller = "Menu"
                    },
                    new MenuItemModel
                    {
                        Id = 5,
                        Text = "訊息設定",
                        ModuleId = "message",
                        Action = "Message",
                        Controller = "Menu"
                    },
                    new MenuItemModel
                    {
                        Id = 6,
                        Text = "國際認證規範公告",
                        ModuleId = "certification",
                        Action = "ICS",
                        Controller = "Menu"
                    }
                }
            });

            var viewModel = new MenuViewModel
            {
                CurrentModule = currentModule ?? string.Empty,
                MenuGroups = menuGroups
            };

            return PartialView("_Menu", viewModel);
        }

    }
}
