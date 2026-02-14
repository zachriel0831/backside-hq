using Dapper;
using HQBackSite.Models;
using HQBackSite.Utils;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HQBackSite.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class BackSiteAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // 1. 檢查 Cookie
            var authCookie = httpContext.Request.Cookies["Authorization"];
            if (authCookie == null || string.IsNullOrWhiteSpace(authCookie.Value))
                return false;

            var token = authCookie.Value;

            try
            {
                // 2. 解密 JWT Token
                var claims = JwtUtil.Decrypt(token);
                if (claims == null || claims.Count == 0)
                    return false;

                // 3. 取得使用者帳號
                if (!claims.ContainsKey("Account") || string.IsNullOrWhiteSpace(claims["Account"]))
                    return false;

                var account = claims["Account"];

                // 4. 本機測試白名單（僅 localhost）
                if (IsLocalBootstrapAccount(httpContext, account))
                    return true;

                // 5. 查詢資料庫驗證使用者
                var user = QueryUserFromDatabase(account);
                if (user == null)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IsLocalBootstrapAccount(HttpContextBase httpContext, string account)
        {
            if (httpContext?.Request?.IsLocal != true)
                return false;

            var bootstrapAccounts = ConfigurationManager.AppSettings["LocalBootstrapAccounts"];
            if (string.IsNullOrWhiteSpace(bootstrapAccounts))
                return false;

            var accountSet = bootstrapAccounts
                .Split(new[] { ',', ';', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim());

            return accountSet.Contains(account);
        }

        private UserModel QueryUserFromDatabase(string employeeId)
        {
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["EpSqlServer"]?.ConnectionString;
                if (string.IsNullOrWhiteSpace(connectionString))
                    return null;

                var sql = @"
SELECT TOP 1 ORGAN_ID, DEPARTMENT, EMPLOYEE_ID, LOCAL_NAME 
FROM dbo.users WITH(NOLOCK) 
WHERE EMPLOYEE_ID = @employeeId";

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    var user = conn.QuerySingleOrDefault<UserModel>(sql, new { employeeId });
                    return user;
                }
            }
            catch
            {
                return null;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new JsonResult
                {
                    Data = new { code = -1, message = "請先登入" },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(
                        new { controller = "Account", action = "Login" }
                    )
                );
            }
        }
    }
}