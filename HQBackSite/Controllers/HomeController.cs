using HQBackSite.Attributes;
using System.Web.Mvc;

namespace HQBackSite.Controllers
{
    [BackSiteAuthorize]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.CurrentModule = null; // 根路徑時不設定 active
            return View();
        }

        public ActionResult Error()
        {
            return View();
        }
    }
}