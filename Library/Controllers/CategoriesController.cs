
using Library.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Library.Filters;

namespace Library.Controllers
{
    public class CategoriesController : Controller
    {
        private LibraryManagementEntities db =
            new LibraryManagementEntities();

        // ==========================================
        // 1. KIỂM TRA QUYỀN QUẢN LÝ THỂ LOẠI
        // ==========================================

        private bool CanManageCategories()
        {
            if (User == null ||
                User.Identity == null ||
                !User.Identity.IsAuthenticated)
            {
                return false;
            }

            string username = User.Identity.Name;

            string role = db.Accounts
                .AsNoTracking()
                .Where(a => a.Username == username)
                .Select(a => a.Role)
                .FirstOrDefault();

            role = (role ?? "").Trim();

            return string.Equals(
                       role, "Admin",
                       StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                       role, "Librarian",
                       StringComparison.OrdinalIgnoreCase);
        }

        // ==========================================
        // 2. INDEX - DANH SÁCH THỂ LOẠI
        // ==========================================

        // GET: Categories
        // Công khai cho tất cả người dùng
        public ActionResult Index(
            string searchString, int? page)
        {
            bool canManage = CanManageCategories();

            ViewBag.CanManageCategories = canManage;

            ViewBag.Title = canManage
                ? "Quản lý Thể loại"
                : "Khám phá Thể loại";

            int pageSize = 8;
            int pageNumber = page ?? 1;

            var categories = db.Categories
                .AsNoTracking()
                .Include(c => c.Books)
                .AsQueryable();

            // Tìm kiếm theo tên thể loại
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                categories = categories.Where(c =>
                    c.CategoryName.Contains(searchString));
            }

            ViewBag.CurrentSearch = searchString;

            // Sắp xếp theo tên
            categories = categories.OrderBy(
                c => c.CategoryName);

            // Phân trang
            int totalItemCount = categories.Count();

            int totalPages = (int)Math.Ceiling(
                (double)totalItemCount / pageSize);

            if (pageNumber > totalPages && totalPages > 0)
            {
                pageNumber = totalPages;
            }

            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            var list = categories
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Page = pageNumber;
            ViewBag.TotalPages = totalPages;

            return View(list);
        }

        // ==========================================
        // 3. DETAILS - SÁCH THEO THỂ LOẠI
        // ==========================================

        // GET: Categories/Details/5
        // Công khai nhưng lọc sách theo vai trò
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            bool canManage = CanManageCategories();

            // Lấy thể loại và các sách thuộc thể loại
            var category = db.Categories
                .AsNoTracking()
                .Include(c => c.Books)
                .Include("Books.Author")
                .FirstOrDefault(
                    c => c.CategoryID == id.Value);

            if (category == null)
            {
                return HttpNotFound();
            }

            // Khách và Reader chỉ được xem
            // những cuốn sách đang hoạt động
            if (!canManage)
            {
                category.Books = category.Books
                    .Where(b => b.IsActive == true)
                    .ToList();
            }

            // Không lọc StockQuantity:
            // sách hết hàng vẫn được hiển thị.
            ViewBag.CanManageCategories = canManage;
            ViewBag.BookCount = category.Books.Count;

            ViewBag.Title = "Thể loại: " +
                category.CategoryName;

            return View(category);
        }

        // ==========================================
        // 4. CREATE - THÊM THỂ LOẠI
        // ==========================================

        // GET: Categories/Create
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm mới Thể loại";
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult Create(
            [Bind(Include = "CategoryID,CategoryName")]
            Category category)
        {
            ViewBag.Title = "Thêm mới Thể loại";

            if (!string.IsNullOrEmpty(category.CategoryName))
            {
                category.CategoryName =
                    category.CategoryName.Trim();

                string lowerName =
                    category.CategoryName.ToLower();

                bool isDuplicate = db.Categories.Any(
                    c => c.CategoryName.Trim().ToLower()
                         == lowerName);

                if (isDuplicate)
                {
                    ModelState.AddModelError(
                        "CategoryName",
                        "Tên thể loại này đã tồn tại.");
                }
            }

            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();

                TempData["Success"] =
                    "Thêm mới thể loại thành công!";

                return RedirectToAction("Index");
            }

            return View(category);
        }

        // ==========================================
        // 5. EDIT - CHỈNH SỬA THỂ LOẠI
        // ==========================================

        // GET: Categories/Edit/5
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult Edit(int? id)
        {
            ViewBag.Title = "Chỉnh sửa Thể loại";

            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            Category category = db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult Edit(
            [Bind(Include = "CategoryID,CategoryName")]
            Category category)
        {
            ViewBag.Title = "Chỉnh sửa Thể loại";

            if (!string.IsNullOrEmpty(category.CategoryName))
            {
                category.CategoryName =
                    category.CategoryName.Trim();

                string lowerName =
                    category.CategoryName.ToLower();

                bool isDuplicate = db.Categories.Any(
                    c => c.CategoryID != category.CategoryID
                         && c.CategoryName.Trim().ToLower()
                            == lowerName);

                if (isDuplicate)
                {
                    ModelState.AddModelError(
                        "CategoryName",
                        "Tên thể loại này đã tồn tại ở thể loại khác.");
                }
            }

            if (ModelState.IsValid)
            {
                db.Entry(category).State =
                    EntityState.Modified;

                db.SaveChanges();

                TempData["Success"] =
                    "Cập nhật thể loại thành công!";

                return RedirectToAction("Index");
            }

            return View(category);
        }

        // ==========================================
        // 6. DELETE - XÓA THỂ LOẠI
        // ==========================================

        // GET: Categories/Delete/5
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult Delete(int? id)
        {
            ViewBag.Title = "Xóa Thể loại";

            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            Category category = db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Category category = db.Categories
                    .Include(c => c.Books)
                    .FirstOrDefault(
                        c => c.CategoryID == id);

                if (category == null)
                {
                    return HttpNotFound();
                }

                // Không xóa thể loại nếu còn sách
                if (category.Books != null &&
                    category.Books.Any())
                {
                    TempData["Error"] =
                        "Không thể xóa thể loại này vì vẫn còn sách bên trong!";

                    return RedirectToAction("Index");
                }

                db.Categories.Remove(category);
                db.SaveChanges();

                TempData["Success"] =
                    "Xóa thể loại thành công!";
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Lỗi cơ sở dữ liệu: Không thể xóa bản ghi đang có ràng buộc khóa ngoại.";

                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        // ==========================================
        // 7. GIẢI PHÓNG DB CONTEXT
        // ==========================================

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
