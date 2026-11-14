
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library.Models;

namespace Library.Filters
{
    public class RoleAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] allowedRoles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            allowedRoles = roles ?? new string[0];
        }

        protected override bool AuthorizeCore(
            HttpContextBase httpContext)
        {
            // Chưa đăng nhập thì từ chối
            if (!base.AuthorizeCore(httpContext))
            {
                return false;
            }

            string username = httpContext.User.Identity.Name;

            // Đọc vai trò trực tiếp từ cơ sở dữ liệu
            using (var db = new LibraryManagementEntities())
            {
                var account = db.Accounts
                    .Where(a => a.Username == username)
                    .Select(a => new { a.Role })
                    .FirstOrDefault();

                if (account == null)
                {
                    return false;
                }

                return allowedRoles.Any(role =>
                    string.Equals(
                        role,
                        account.Role,
                        StringComparison.OrdinalIgnoreCase));
            }
        }

        protected override void HandleUnauthorizedRequest(
            AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // Đã đăng nhập nhưng không đủ quyền
                filterContext.Result =
                    new HttpStatusCodeResult(403, "Forbidden");
            }
            else
            {
                // Chưa đăng nhập: chuyển đến Auth/Login
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}
