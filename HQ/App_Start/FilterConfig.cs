using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HQ
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new HandleErrorAttribute());
            filters.Add(new HqErrorHandlerAttribute());
        }
    }

    public class HqErrorHandlerAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            var controller = filterContext.Controller as Controller;
            controller.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
            controller.Response.TrySkipIisCustomErrors = true;
            filterContext.ExceptionHandled = true;

            var httpMethod = filterContext.HttpContext.Request.HttpMethod;
            var controllerName = (string)filterContext.RouteData.Values["controller"];
            var actionName = (string)filterContext.RouteData.Values["action"];
            var exception = filterContext.Exception;

            if (!filterContext.HttpContext.Request.IsAjaxRequest())
            {
                var model = new HandleErrorInfo(exception, controllerName, actionName);
                var view = new ViewResult
                {
                    ViewName = "Error",
                    ViewData = new ViewDataDictionary(model)
                };
                view.ViewData.Model = model;

                var viewData = controller.ViewData;
                if (viewData != null && viewData.Count > 0)
                    viewData.ToList().ForEach(view.ViewData.Add);

                view.ExecuteResult(filterContext);
            }

        }
    }
}
