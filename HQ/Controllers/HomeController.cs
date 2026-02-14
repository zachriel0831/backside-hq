using System.Web.Mvc;

namespace HQ.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 初始化index頁面
        /// </summary>
        /// <param name="id">id編號</param>
        /// <returns></returns>
        public ActionResult Index(string id)
        {
            ViewBag.Title = "Home Page";
            return View(id);
        }
        public ActionResult FaxView()
        {

            return View();
        }
        public ActionResult System()
        {

            return View();
        }
        public ActionResult Restaurant()
        {

            return View();
        }
        
    }

}
