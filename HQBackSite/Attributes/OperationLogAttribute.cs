using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HQBackSite.Utils;

namespace HQBackSite.Attributes
{
    /// <summary>
    /// 全域操作日誌（AOP）
    /// - 於 Action 執行前/後記錄請求資訊、參數、耗時、結果與例外
    /// - 自動遮罩敏感欄位
    /// </summary>
    public class OperationLogAttribute : ActionFilterAttribute
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string StopwatchKey = "__oplog_stopwatch";
        private const string StartAtKey = "__oplog_startat";

        private sealed class RequestUserInfo
        {
            public string Account { get; set; }
            public string UserName { get; set; }

            public string Display => string.IsNullOrWhiteSpace(Account) ? "anonymous" : Account;
        }

        private static string BuildEntryPrefix(DateTime ts, string controller, string action)
        {
            return $"[{ts:yyyy-MM-dd HH:mm:ss.fff}] [class:{controller}Controller] [method:{action}]";
        }

        private static RequestUserInfo GetRequestUserInfo(HttpContextBase httpContext)
        {
            var info = new RequestUserInfo
            {
                Account = string.Empty,
                UserName = string.Empty
            };

            try
            {
                var authCookie = httpContext?.Request?.Cookies?["Authorization"];
                if (authCookie == null || string.IsNullOrWhiteSpace(authCookie.Value))
                {
                    return info;
                }

                var claims = JwtUtil.Decrypt(authCookie.Value);
                if (claims == null || claims.Count == 0)
                {
                    return info;
                }

                if (claims.ContainsKey("Account"))
                {
                    info.Account = claims["Account"] ?? string.Empty;
                }

                if (claims.ContainsKey("Name"))
                {
                    info.UserName = claims["Name"] ?? string.Empty;
                }
            }
            catch
            {
                // ignore, keep anonymous
            }

            return info;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null)
            {
                return;
            }

            var now = DateTime.Now;
            var sw = Stopwatch.StartNew();

            filterContext.HttpContext.Items[StopwatchKey] = sw;
            filterContext.HttpContext.Items[StartAtKey] = now;

            var request = filterContext.HttpContext?.Request;
            var route = filterContext.RouteData;
            var user = GetRequestUserInfo(filterContext.HttpContext);

            var controller = route?.Values["controller"]?.ToString() ?? string.Empty;
            var action = route?.Values["action"]?.ToString() ?? string.Empty;
            var method = request?.HttpMethod ?? string.Empty;
            var url = request?.Url?.ToString() ?? string.Empty;
            var query = SafeSerializeNameValueCollection(request?.QueryString);
            var form = SafeSerializeNameValueCollection(request?.Form);
            var actionArgs = SafeSerializeActionArguments(filterContext.ActionParameters);

            var beginPrefix = BuildEntryPrefix(now, controller, action);
            var beginMessage = $@"[OP_BEGIN] {beginPrefix}
Timestamp: {now:yyyy-MM-dd HH:mm:ss.fff}
User: {user.Display}
Controller: {controller}
Action: {action}
Method: {method}
Url: {url}
Query: {query}
Form: {form}
ActionArgs: {actionArgs}";

            Logger.Info(beginMessage);

            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext == null)
            {
                return;
            }

            var request = filterContext.HttpContext?.Request;
            var response = filterContext.HttpContext?.Response;
            var route = filterContext.RouteData;
            var user = GetRequestUserInfo(filterContext.HttpContext);

            var controller = route?.Values["controller"]?.ToString() ?? string.Empty;
            var action = route?.Values["action"]?.ToString() ?? string.Empty;
            var method = request?.HttpMethod ?? string.Empty;
            var url = request?.Url?.ToString() ?? string.Empty;

            var sw = filterContext.HttpContext?.Items[StopwatchKey] as Stopwatch;
            if (sw != null && sw.IsRunning)
            {
                sw.Stop();
            }

            var startAt = filterContext.HttpContext?.Items[StartAtKey] as DateTime?;
            var elapsedMs = sw?.ElapsedMilliseconds ?? 0;
            var statusCode = response?.StatusCode ?? 0;
            var resultType = filterContext.Result?.GetType().Name ?? "(null)";
            var hasException = filterContext.Exception != null;

            var endAt = DateTime.Now;
            var endPrefix = BuildEntryPrefix(endAt, controller, action);
            var endMessage = $@"[OP_END] {endPrefix}
Timestamp: {endAt:yyyy-MM-dd HH:mm:ss.fff}
StartAt: {(startAt.HasValue ? startAt.Value.ToString("yyyy-MM-dd HH:mm:ss.fff") : string.Empty)}
ElapsedMs: {elapsedMs}
User: {user.Display}
Controller: {controller}
Action: {action}
Method: {method}
Url: {url}
StatusCode: {statusCode}
ResultType: {resultType}
HasException: {hasException}
Exception: {(hasException ? filterContext.Exception.ToString() : string.Empty)}";

            if (hasException)
            {
                Logger.Error(endMessage);
            }
            else
            {
                Logger.Info(endMessage);
            }

            base.OnActionExecuted(filterContext);
        }

        private static string SafeSerializeActionArguments(IDictionary<string, object> args)
        {
            if (args == null || args.Count == 0)
            {
                return "{}";
            }

            var safeMap = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in args)
            {
                if (IsSensitiveKey(item.Key))
                {
                    safeMap[item.Key] = "***";
                }
                else
                {
                    safeMap[item.Key] = SanitizeObject(item.Value);
                }
            }

            return JsonConvert.SerializeObject(safeMap);
        }

        private static object SanitizeObject(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (obj is string || obj.GetType().IsPrimitive)
            {
                return obj;
            }

            var props = obj.GetType().GetProperties()
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in props)
            {
                object value;
                try
                {
                    value = prop.GetValue(obj, null);
                }
                catch
                {
                    value = "<read_failed>";
                }

                result[prop.Name] = IsSensitiveKey(prop.Name) ? "***" : value;
            }

            return result;
        }

        private static string SafeSerializeNameValueCollection(NameValueCollection data)
        {
            if (data == null || data.Count == 0)
            {
                return "{}";
            }

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var key in data.AllKeys)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                map[key] = IsSensitiveKey(key) ? "***" : (data[key] ?? string.Empty);
            }

            return JsonConvert.SerializeObject(map);
        }

        private static bool IsSensitiveKey(string key)
        {
            var lower = (key ?? string.Empty).ToLowerInvariant();
            return lower.Contains("password") ||
                   lower.Contains("pwd") ||
                   lower.Contains("token") ||
                   lower.Contains("secret") ||
                   lower.Contains("account") ||
                   lower.Contains("userid") ||
                   lower.Contains("user_id") ||
                   lower.Contains("username") ||
                   lower.Contains("employeeid") ||
                   lower.Contains("employee_id") ||
                   lower.Contains("emid") ||
                   lower.Contains("authorization") ||
                   lower.Contains("cookie");
        }
    }
}