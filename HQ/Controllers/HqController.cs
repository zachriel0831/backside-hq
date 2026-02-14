using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HQ.Controllers
{
    public class HqController : Controller
    {
        // GET: Kit
        public ActionResult Room1()
        {
            return View();
        }
        public ActionResult Index()
        {
            ViewBag.Title = "新光三越內部入口網站";
            return View();
        }
        public ActionResult Dept()
        {
            ViewBag.Title = "新光三越內部入口網站";
            return View();
        }
    }
}