using Microsoft.AspNetCore.Mvc;

namespace YourProject.Controllers {
    public class AccountController : Controller {
        // GET: /Account/Index
        public IActionResult Index() {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(string username, string password) {
            // stub
            if (username == "admin" && password == "password") {
                return RedirectToAction("Dashboard", "Home");
            } else {
                ViewBag.Error = "Invalid login attempt.";
                return View("Index");
            }
        }

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(string username, string password) {
            // todo ..
            // register user here
            return RedirectToAction("Index");
        }

        public IActionResult BrowseAsGuest() {
            // todo ..
            // continue browsing without an account
            return RedirectToAction("Home", "Index");
        }
    }
}
