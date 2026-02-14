using HQBackSite.Models;
using HQBackSite.Utils;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HQBackSite.Controllers
{
    public class AccountController : BaseController
    {
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(UserModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Account))
            {
                return Json(Fail("帳號不可為空"));
            }

            model.Account = model.Account.Trim();
            if (model.Account.Length != 8)
            {
                return Json(Fail("帳號必須為八碼"));
            }

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                return Json(Fail("請輸入密碼"));
            }

            // 驗證是否有權限
            var codeNames = GetUserCodeNames(model.Account);
            if (codeNames.Count == 0)
            {
                if (!IsLocalBootstrapAccount(model.Account))
                {
                    var debugData = BuildLoginDebugData(model.Account);
                    if (debugData != null)
                    {
                        return Json(Fail("您沒有權限", debugData));
                    }

                    return Json(Fail("您沒有權限"));
                }
            }

            // 驗證AD
            //var accSql = @"SELECT TOP 1 RTRIM(EMAIL) EMAIL FROM dbo.users WITH(NOLOCK) WHERE EMPLOYEE_ID = @Account";
            //var accRes = QuerySingle<UserModel>(accSql, new { model.Account }, ConnectionStringName.EpSqlServer);
            //if (accRes == null)
            //{
            //    return Json(Fail("找不到使用者"));
            //}
            //if (!VerifyAD(accRes.EMAIL, model.Password))
            //{
            //    return Json(Fail("登入帳號或密碼錯誤"));
            //}

            var claims = new Dictionary<string, string>
                {
                    { "Account", model.Account },
                };
            var cookie = new HttpCookie("Authorization", JwtUtil.Generate(claims, DateTime.Now.AddDays(1)));
            cookie.Expires = DateTime.Now.AddDays(1); 
            cookie.HttpOnly = true;                
            cookie.Path = "/";                         
            Response.Cookies.Add(cookie);

            var redirectUrl = Url.Action("Index", "Home");
            return Json(new
            {
                code = 1,
                message = "成功",
                redirectUrl,
                data = new
                {
                    redirectUrl
                }
            });
        }

        private bool IsLocalBootstrapAccount(string account)
        {
            if (Request?.IsLocal != true)
            {
                return false;
            }

            var bootstrapAccounts = ConfigurationManager.AppSettings["LocalBootstrapAccounts"];
            if (string.IsNullOrWhiteSpace(bootstrapAccounts))
            {
                return false;
            }

            var accountSet = bootstrapAccounts
                .Split(new[] { ',', ';', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim());

            return accountSet.Contains(account);
        }

        private object BuildLoginDebugData(string account)
        {
            if (!ShouldReturnLoginDebugData())
            {
                return null;
            }

            try
            {
                var permissionCountSql = @"SELECT COUNT(1) FROM dbo.para_permission WITH(NOLOCK) WHERE deleted = 0 AND emid = @emid";
                var permissionCount = QuerySingle<int>(permissionCountSql, new { emid = account });

                var permissionTopSql = @"SELECT TOP 5 code_name FROM dbo.para_permission WITH(NOLOCK) WHERE deleted = 0 AND emid = @emid ORDER BY id DESC";
                var permissionTop = Query<string>(permissionTopSql, new { emid = account });

                var userSql = @"SELECT TOP 1 RTRIM(EMAIL) EMAIL FROM dbo.users WITH(NOLOCK) WHERE EMPLOYEE_ID = @Account";
                var user = QuerySingle<UserModel>(userSql, new { Account = account }, ConnectionStringName.EpSqlServer);

                return new
                {
                    account,
                    permissionCount,
                    permissionTop,
                    existsInUsersTable = user != null,
                    adEmail = user?.EMAIL,
                    isLocalRequest = Request?.IsLocal == true,
                    localBootstrapEnabled = IsLocalBootstrapAccount(account),
                    hint = "若 permissionCount=0，請在 dbo.para_permission 新增對應 emid 權限"
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    account,
                    error = ex.Message
                };
            }
        }

        private bool ShouldReturnLoginDebugData()
        {
            if (Request?.IsLocal != true)
            {
                return false;
            }

            var setting = ConfigurationManager.AppSettings["EnableLocalLoginDebug"];
            if (string.IsNullOrWhiteSpace(setting))
            {
                return false;
            }

            bool enable;
            return bool.TryParse(setting, out enable) && enable;
        }

        public bool VerifyAD(string userName, string password)
        {
            var accountName = userName;
            var atIndex = userName.IndexOf('@');
            if (atIndex > 0)
            {
                accountName = userName.Substring(0, atIndex);
            }

            try
            {
                var ldapPath = "LDAP://DC=skm,DC=com,DC=tw"; 

                using (var entry = new DirectoryEntry(ldapPath, userName, password, AuthenticationTypes.Secure))
                using (var search = new DirectorySearcher(entry))
                {
                    search.SearchScope = SearchScope.Subtree;
                    search.Filter = $"(sAMAccountName={accountName})";
                    search.PropertiesToLoad.Add("cn");
                    search.PropertiesToLoad.Add("objectSid");

                    var result = search.FindOne();
                    return result != null;
                }
            }
            catch 
            {
                return false;
            }
        }

        public ActionResult Logout()
        {
            var cookie = new HttpCookie("Authorization")
            {
                Expires = DateTime.Now.AddDays(-1),
                HttpOnly = true,
                Path = "/"
            };
            Response.Cookies.Add(cookie);

            return RedirectToAction("Login", "Account");
        }
    }
}