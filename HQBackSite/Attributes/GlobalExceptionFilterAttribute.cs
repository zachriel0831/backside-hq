using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Web.Mvc;

namespace HQBackSite.Attributes
{
    public class GlobalExceptionFilterAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null || filterContext.ExceptionHandled)
            {
                return;
            }

            var exception = filterContext.Exception;
            var httpContext = filterContext.HttpContext;
            var request = httpContext?.Request;

            var controller = filterContext.RouteData.Values["controller"]?.ToString() ?? string.Empty;
            var action = filterContext.RouteData.Values["action"]?.ToString() ?? string.Empty;
            var method = request?.HttpMethod ?? string.Empty;
            var url = request?.Url?.ToString() ?? string.Empty;
            var queryString = request?.QueryString?.ToString() ?? string.Empty;
            var formData = SafeSerializeNameValueCollection(request?.Form);

            var logMessage = $@"[UNHANDLED_EXCEPTION]
Controller: {controller}
Action: {action}
Method: {method}
Url: {url}
QueryString: {queryString}
Form: {formData}
Exception: {exception}";

            Trace.TraceError(logMessage);
            Debug.WriteLine(logMessage);

            var showDetail = ShouldShowDetailedError(httpContext);
            var isAjax = request?.IsAjaxRequest() == true ||
                         ((request?.Headers["Accept"] ?? string.Empty).IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0);

            if (isAjax)
            {
                var payload = new
                {
                    code = 0,
                    message = "系統發生錯誤，請聯絡管理員",
                    data = showDetail
                        ? new
                        {
                            controller,
                            action,
                            method,
                            url,
                            queryString,
                            form = formData,
                            exception = exception.ToString()
                        }
                        : null
                };

                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    DateFormatString = "yyyy-MM-dd HH:mm:ss"
                };

                filterContext.Result = new ContentResult
                {
                    ContentType = "application/json",
                    ContentEncoding = Encoding.UTF8,
                    Content = JsonConvert.SerializeObject(payload, settings)
                };
            }
            else if (showDetail)
            {
                filterContext.Result = new ContentResult
                {
                    ContentType = "text/plain",
                    ContentEncoding = Encoding.UTF8,
                    Content = logMessage
                };
            }
            else
            {
                filterContext.Result = new ViewResult { ViewName = "Error" };
            }

            if (httpContext?.Response != null)
            {
                httpContext.Response.StatusCode = 500;
                httpContext.Response.TrySkipIisCustomErrors = true;
            }

            filterContext.ExceptionHandled = true;
        }

        private static bool ShouldShowDetailedError(System.Web.HttpContextBase httpContext)
        {
            if (httpContext?.Request?.IsLocal != true)
            {
                return false;
            }

            var setting = ConfigurationManager.AppSettings["EnableLocalExceptionDebug"];
            if (string.IsNullOrWhiteSpace(setting))
            {
                return true;
            }

            bool enable;
            return bool.TryParse(setting, out enable) && enable;
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

                if (IsSensitiveKey(key))
                {
                    map[key] = "***";
                }
                else
                {
                    map[key] = data[key] ?? string.Empty;
                }
            }

            return JsonConvert.SerializeObject(map);
        }

        private static bool IsSensitiveKey(string key)
        {
            var lower = (key ?? string.Empty).ToLowerInvariant();
            return lower.Contains("password") ||
                   lower.Contains("pwd") ||
                   lower.Contains("token") ||
                   lower.Contains("account") ||
                   lower.Contains("userid") ||
                   lower.Contains("user_id") ||
                   lower.Contains("username") ||
                   lower.Contains("employeeid") ||
                   lower.Contains("employee_id") ||
                   lower.Contains("emid") ||
                   lower.Contains("secret");
        }
    }
}
