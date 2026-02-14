using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace HQ.Controllers
{
    public class BacksideController : Controller
    {
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Page");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // TODO: 檢查登入者資訊
                string ConnectionString = ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString;
                DataSet ds = new DataSet();
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = "Select usr_id From Users " +
                                 "where usr_id = @user and passwrd = @passwrd";
                    SqlCommand command = new SqlCommand(sql, connection);

                    command.Parameters.Add(new SqlParameter() { ParameterName = "@user", Value = model.UserName });
                    command.Parameters.Add(new SqlParameter() { ParameterName = "@passwrd", Value = model.Password });

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                    sqlDataAdapter.Fill(ds);
                    connection.Close();
                }
                if (ds.Tables[0].Rows.Count==0)
                {
                    ModelState.AddModelError("", "帳號或密碼不正確");
                    return View(model);
                }
                // 驗證資訊
                var ticket = new FormsAuthenticationTicket(
                    version: 1,
                    name: model.UserName,
                    issueDate: DateTime.UtcNow,
                    expiration: DateTime.UtcNow.AddMinutes(30),
                    isPersistent: false,
                    userData: "",
                    cookiePath: FormsAuthentication.FormsCookiePath);

                var encryptedTicket = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                Response.Cookies.Add(cookie);

                return RedirectToAction("Page");
            }
            return View(model);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [Authorize]
        // GET: Back
        public ActionResult Page()
        {
            return View();
        }
        [Authorize]
        public ActionResult Unit()
        {
            return View();
        }
        [Authorize]
        public ActionResult Content()
        {
            return View();
        }
        [Authorize]
        public ActionResult News()
        {
            return View();
        }
        [Authorize]
        public ActionResult Safe()
        {
            return View();
        }
        [Authorize]
        public ActionResult Hr()
        {
            return View();
        }
        public ActionResult Test()
        {
            return View();
        }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "最好是沒有{0}!")]
        [Display(Name = "帳號")]
        public string UserName { get; set; }
        [Required]
        [Display(Name = "密碼")]
        public string Password { get; set; }
    }
}