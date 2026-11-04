using System;
using System.Collections.Generic;
using Library.Helpers;
using Library.Models.ViewModels;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Library.Models;

namespace Library.Controllers
{
    public class UsersController : Controller
    {
        private LibraryManagementEntities db = new LibraryManagementEntities();

        // GET: Users
        public ActionResult Index(
    string search,
    string role,
    int page = 1)
        {
            ViewBag.Title = "Quản lý tài khoản nhân viên";

            const int pageSize = 10;

            var query = db.Accounts.AsQueryable();

            // Tìm theo tên đăng nhập
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(a => a.Username.Contains(search));
            }

            // Lọc theo vai trò
            if (role == "Admin" || role == "Librarian")
            {
                query = query.Where(a => a.Role == role);
            }
            else
            {
                role = null;
            }

            int totalItems = query.Count();

            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling(totalItems / (double)pageSize));

            page = Math.Max(1, Math.Min(page, totalPages));

            // Chỉ lấy các trường cần hiển thị, không lấy mật khẩu.
            var rows = query
                .OrderBy(a => a.Username)
                .ThenBy(a => a.AccountID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.AccountID,
                    a.Username,
                    a.Role
                })
                .ToList();

            var accounts = rows.Select(a => new Account
            {
                AccountID = a.AccountID,
                Username = a.Username,
                Role = a.Role
            }).ToList();

            ViewBag.Search = search;
            ViewBag.CurrentRole = role;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(accounts);
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            ViewBag.Title = "Chi tiết tài khoản";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Chỉ lấy thông tin cần xem, không lấy mật khẩu.
            var data = db.Accounts
                .Where(a => a.AccountID == id.Value)
                .Select(a => new
                {
                    a.AccountID,
                    a.Username,
                    a.Role
                })
                .SingleOrDefault();

            if (data == null)
            {
                return HttpNotFound();
            }

            var account = new Account
            {
                AccountID = data.AccountID,
                Username = data.Username,
                Role = data.Role
            };

            return View(account);
        }

        // GET: Users/Create
        // GET: Users/Create
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm tài khoản nhân viên";

            return View(new UserCreateViewModel
            {
                Role = "Librarian"
            });
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserCreateViewModel model)
        {
            ViewBag.Title = "Thêm tài khoản nhân viên";

            if (!string.IsNullOrWhiteSpace(model.Username))
            {
                model.Username = model.Username.Trim();
                string username = model.Username.ToLower();

                if (db.Accounts.Any(a => a.Username.ToLower() == username))
                {
                    ModelState.AddModelError(
                        "Username",
                        "Tên đăng nhập này đã tồn tại.");
                }
            }

            if (ModelState.IsValid)
            {
                var account = new Account
                {
                    Username = model.Username,
                    Role = model.Role,
                    Password = PasswordHelper.HashPassword(model.Password)
                };

                db.Accounts.Add(account);

                try
                {
                    db.SaveChanges();

                    TempData["Success"] = "Tạo tài khoản thành công.";
                    return RedirectToAction("Index");
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    db.Entry(account).State = EntityState.Detached;

                    var sqlException = ex.GetBaseException()
                        as System.Data.SqlClient.SqlException;

                    if (sqlException == null ||
                        (sqlException.Number != 2601 &&
                         sqlException.Number != 2627))
                    {
                        throw;
                    }

                    ModelState.AddModelError(
                        "Username",
                        "Tên đăng nhập này đã tồn tại.");
                }
            }

            // Không đưa mật khẩu đã nhập trở lại trang.
            model.Password = null;
            model.ConfirmPassword = null;

            return View(model);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            ViewBag.Title = "Sửa tài khoản nhân viên";

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var account = db.Accounts.Find(id);

            if (account == null)
                return HttpNotFound();

            return View(new UserEditViewModel
            {
                AccountID = account.AccountID,
                Username = account.Username,
                Role = account.Role
            });
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AccountID,Role,NewPassword,ConfirmPassword")]UserEditViewModel model)
        {
            ViewBag.Title = "Sửa tài khoản nhân viên";

            var account = db.Accounts.Find(model.AccountID);

            if (account == null)
                return HttpNotFound();

            // Lấy tên từ database, không lấy tên gửi lên từ form.
            model.Username = account.Username;

            // Tránh hạ quyền Admin cuối cùng.
            if (account.Role == "Admin" &&
                model.Role != "Admin" &&
                !db.Accounts.Any(a =>
                    a.Role == "Admin" &&
                    a.AccountID != account.AccountID))
            {
                ModelState.AddModelError(
                    "Role",
                    "Không thể đổi vai trò của Admin cuối cùng.");
            }

            if (ModelState.IsValid)
            {
                account.Role = model.Role;

                // Để trống thì không thay đổi mật khẩu đang lưu.
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    account.Password =
                        PasswordHelper.HashPassword(model.NewPassword);
                }

                db.SaveChanges();

                TempData["Success"] = "Cập nhật tài khoản thành công.";
                return RedirectToAction("Index");
            }

            model.NewPassword = null;
            model.ConfirmPassword = null;

            return View(model);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            ViewBag.Title = "Xóa tài khoản";

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var data = db.Accounts
                .Where(a => a.AccountID == id.Value)
                .Select(a => new
                {
                    a.AccountID,
                    a.Username,
                    a.Role
                })
                .SingleOrDefault();

            if (data == null)
                return HttpNotFound();

            var account = new Account
            {
                AccountID = data.AccountID,
                Username = data.Username,
                Role = data.Role
            };

            ViewBag.CanDelete = account.Role != "Admin" ||
                db.Accounts.Any(a =>
                    a.Role == "Admin" &&
                    a.AccountID != account.AccountID);

            return View(account);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Giữ việc kiểm tra Admin và xóa trong cùng transaction.
            using (var transaction = db.Database.BeginTransaction(
                System.Data.IsolationLevel.Serializable))
            {
                var account = db.Accounts.Find(id);

                if (account == null)
                    return HttpNotFound();

                bool lastAdmin = account.Role == "Admin" &&
                    !db.Accounts.Any(a =>
                        a.Role == "Admin" &&
                        a.AccountID != account.AccountID);

                if (lastAdmin)
                {
                    TempData["Error"] = "Không được xóa Admin cuối cùng.";
                    return RedirectToAction("Delete", new { id });
                }

                db.Accounts.Remove(account);

                try
                {
                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = "Xóa tài khoản thành công.";
                    return RedirectToAction("Index");
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException)
                {
                    transaction.Rollback();

                    TempData["Error"] =
                        "Không thể xóa tài khoản. Dữ liệu có thể đang được sử dụng.";

                    return RedirectToAction("Index");
                }
            }
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
