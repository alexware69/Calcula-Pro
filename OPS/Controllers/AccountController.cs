using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OnlinePriceSystem.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        [Route("")]
        [Route("index")]
        [Route("~/")]
        public IActionResult Index()
        {
            string user = HttpContext.Session.GetString("username");
            if(user != null) return View("Success");
            else return View();
        }

        [Route("login")]
        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            if (ModelState.IsValid && ValidLogin(username, password))
            {
                HttpContext.Session.SetString("username", username);
                return View("Success");
            }
            else
            {
                ViewBag.error = "Invalid Account";
                return View("Index");
            }
        }

        public bool ValidLogin(string username, string password)
        {
            ops_inhouseEntities dc = new ops_inhouseEntities();

            var user = dc.user_accounts.Select(u => u).Where(u => u.user == username && u.pwd == password);
            if (user.Count() == 1)
                return true;
            else return false;
        }

        [Route("logout")]
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("username");
            return RedirectToAction("Index");
        }
    }
}