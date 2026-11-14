
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Library.Helpers;
using Library.Models;
using Library.Models.ViewModels;

namespace Library.Controllers
{
    public class AuthController : Controller
    {
        private readonly LibraryManagementEntities db =
            new LibraryManagementEntities();

        // GET: Auth/Login
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;

            return View(new LoginViewModel());
        }

        // POST: Auth/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(
            LoginViewModel model,
            string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string username = model.Username.Trim();

            var account = db.Accounts.FirstOrDefault(
                a => a.Username == username);

            if (account == null ||
                !PasswordHelper.VerifyPassword(
                    model.Password, account.Password))
            {
                ModelState.AddModelError(
                    "",
                    "Tên đăng nhập hoặc mật khẩu không đúng.");

                model.Password = null;

                return View(model);
            }

            // Tạo cookie xác thực
            FormsAuthentication.SetAuthCookie(
                account.Username, false);

            // Chỉ chuyển hướng đến URL nội bộ hợp lệ
            if (!string.IsNullOrEmpty(returnUrl)
                && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // POST: Auth/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
