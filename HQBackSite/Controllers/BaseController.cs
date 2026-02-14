using Dapper;
using HQBackSite.Models;
using HQBackSite.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Mvc;


namespace HQBackSite.Controllers
{
    public class BaseController : Controller
    {
        #region Result

        protected new ActionResult Json(object data)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DateFormatString = "yyyy-MM-dd HH:mm:ss"
            };
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(data, settings),
                ContentType = "application/json",
                ContentEncoding = Encoding.UTF8
            };
        }

        protected ResultModel Success()
        {
            return new ResultModel
            {
                Code = 1,
                Message = "成功"
            };
        }

        protected ResultModel Success(string message)
        {
            return new ResultModel
            {
                Code = 1,
                Message = message
            };
        }

        protected ResultModel Fail()
        {
            return new ResultModel
            {
                Code = 0,
                Message = "失敗"
            };
        }

        protected ResultModel Fail(string message)
        {
            return new ResultModel
            {
                Code = 0,
                Message = message
            };
        }

        protected ResultModel Fail(string message, object data)
        {
            return new ResultModel
            {
                Code = 0,
                Message = message,
                Data = data
            };
        }

        protected ResultDataModel<T> SuccessData<T>(T data)
        {
            return new ResultDataModel<T>
            {
                Code = 1,
                Message = "成功",
                Data = data
            };
        }

        #endregion

        #region Dapper

        protected enum ConnectionStringName
        {
            TestSqlServer,
            SqlServer,
            EpSqlServer,
            NewsLetter
        }

        protected string GetConnectionString(ConnectionStringName connectionStringName = ConnectionStringName.SqlServer)
        {
            string name = connectionStringName.ToString();
            return ConfigurationManager.ConnectionStrings[name]?.ConnectionString;
        }

        protected int Execute(string sql, object param = null, ConnectionStringName connectionStringName = ConnectionStringName.SqlServer)
        {
            int rtn = 0;
            string connectionStrings = GetConnectionString(connectionStringName);

            try
            {
                using (var conn = new SqlConnection(connectionStrings))
                {
                    conn.Open();

                    if (param != null)
                    {
                        rtn = conn.Execute(sql, param);
                    }
                    else
                    {
                        rtn = conn.Execute(sql);
                    }
                }
            }
            catch (Exception ex)
            {
                LogDbException("Execute", sql, param, connectionStringName, ex);
                throw;
            }

            return rtn;
        }

        protected List<T> Query<T>(string sql, object param = null, ConnectionStringName connectionStringName = ConnectionStringName.SqlServer)
        {
            List<T> rtn = new List<T>();
            string connectionStrings = GetConnectionString(connectionStringName);

            try
            {
                using (var conn = new SqlConnection(connectionStrings))
                {
                    conn.Open();

                    var command = new CommandDefinition(sql, param);
                    rtn = conn.Query<T>(command).ToList();
                }
            }
            catch (Exception ex)
            {
                LogDbException("Query", sql, param, connectionStringName, ex);
                throw;
            }

            return rtn;
        }

        protected T QuerySingle<T>(string sql, object param = null, ConnectionStringName connectionStringName = ConnectionStringName.SqlServer)
        {
            T rtn;
            string connectionStrings = GetConnectionString(connectionStringName);

            try
            {
                using (var conn = new SqlConnection(connectionStrings))
                {
                    conn.Open();

                    if (param != null)
                    {
                        rtn = conn.QuerySingleOrDefault<T>(sql, param);
                    }
                    else
                    {
                        rtn = conn.QuerySingleOrDefault<T>(sql);
                    }
                }
            }
            catch (Exception ex)
            {
                LogDbException("QuerySingle", sql, param, connectionStringName, ex);
                throw;
            }

            return rtn;
        }

        private void LogDbException(string operation, string sql, object param, ConnectionStringName connectionStringName, Exception ex)
        {
            string paramText;
            try
            {
                paramText = param == null ? "null" : JsonConvert.SerializeObject(param);
            }
            catch
            {
                paramText = "<serialize_param_failed>";
            }

            var message = $@"[DB_ERROR]
Operation: {operation}
Connection: {connectionStringName}
SQL: {sql}
Param: {paramText}
Exception: {ex}";

            Trace.TraceError(message);
            Debug.WriteLine(message);
        }
        #endregion

        #region Permission

        protected string GetAccountFromCookie()
        {
            try
            {
                // 1. 取得 Cookie
                var authCookie = HttpContext.Request.Cookies["Authorization"];
                if (authCookie == null || string.IsNullOrWhiteSpace(authCookie.Value))
                    return null;

                var token = authCookie.Value;

                // 2. 解密 JWT Token
                var claims = JwtUtil.Decrypt(token);
                if (claims == null || claims.Count == 0)
                    return null;

                // 3. 取得使用者帳號
                if (!claims.ContainsKey("Account") || string.IsNullOrWhiteSpace(claims["Account"]))
                    return null;

                return claims["Account"];
            }
            catch (Exception ex)
            {
                Trace.TraceError($"[AUTH_COOKIE_ERROR] {ex}");
                Debug.WriteLine($"[AUTH_COOKIE_ERROR] {ex}");
                return null;
            }
        }

        protected List<string> GetUserCodeNames()
        {
            string account = null;
            try
            {
                account = GetAccountFromCookie();
                if (string.IsNullOrWhiteSpace(account))
                    return new List<string>();

                var sql = @"SELECT code_name FROM dbo.para_permission WITH(NOLOCK) WHERE deleted = 0 AND emid = @emid";

                var list = Query<string>(sql, new { emid = account });

                return list ?? new List<string>();
            }
            catch (Exception ex)
            {
                Trace.TraceError($"[GET_USER_CODE_NAMES_ERROR] account={account}, ex={ex}");
                Debug.WriteLine($"[GET_USER_CODE_NAMES_ERROR] account={account}, ex={ex}");
                return new List<string>();
            }
        }

        protected List<string> GetUserCodeNames(string emid)
        {
            //try
            //{
            //    var sql = @"SELECT code_name FROM dbo.para_permission WITH(NOLOCK) WHERE deleted = 0 AND emid = @emid";

            //    var list = Query<string>(sql, new { emid });

            //    return list ?? new List<string>();
            //}
            //catch
            //{
            //    return new List<string>();
            //}

            return new List<string>() { "Aa095796"};
        }

        protected bool CheckUserDepartment()
        {
            try
            {
                var account = GetAccountFromCookie();
                if (string.IsNullOrWhiteSpace(account))
                    return false;

                var sql = @"SELECT TOP 1 ORGAN_ID, DEPARTMENT, EMPLOYEE_ID, LOCAL_NAME FROM dbo.users WITH(NOLOCK) WHERE EMPLOYEE_ID = @employeeId";
                var user = QuerySingle<UserModel>(sql, new { employeeId = account }, ConnectionStringName.EpSqlServer);

                if (user == null || string.IsNullOrWhiteSpace(user.DEPARTMENT))
                    return false;

                return user.DEPARTMENT.Trim() == "資訊部開發";
            }
            catch (Exception ex)
            {
                Trace.TraceError($"[CHECK_USER_DEPARTMENT_ERROR] ex={ex}");
                Debug.WriteLine($"[CHECK_USER_DEPARTMENT_ERROR] ex={ex}");
                return false;
            }
        }

        #endregion
    }
}