using System.Web.Mvc;
using HQBackSite.Attributes;

namespace HQBackSite
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new OperationLogAttribute());
            filters.Add(new GlobalExceptionFilterAttribute());
        }
    }
}
