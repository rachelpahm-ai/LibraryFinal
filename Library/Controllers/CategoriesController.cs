using Library.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Library.Filters;

namespace Library.Controllers
{
    public class CategoriesController : Controller
    {
        private LibraryManagementEntities db = new LibraryManagementEntities();

        // 1. INDEX: Tìm kiếm, sắp xếp và phân trang bằng LINQ
        public ActionResult Index(string searchString, int? page)
        {
            ViewBag.Title = "Quản lý Thể loại";
            int pageSize = 8; // Số dòng trên mỗi trang (8-10 dòng)
            int pageNumber = (page ?? 1);

            var categories = db.Categories.AsQueryable();

            // Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                categories = categories.Where(c => c.CategoryName.Contains(searchString));
                ViewBag.CurrentSearch = searchString;
            }

            // Sắp xếp theo tên
            categories = categories.OrderBy(c => c.CategoryName);

            int totalItemCount = categories.Count();
            int totalPages = (int)Math.Ceiling((double)totalItemCount / pageSize);
            if (pageNumber > totalPages && totalPages > 0) pageNumber = totalPages;
            if (pageNumber < 1) pageNumber = 1;

            // Đã sửa lại cú pháp Skip/Take chuẩn xác
            var list = categories.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Page = pageNumber;
            ViewBag.TotalPages = totalPages;

            return View(list);
        }

        // 2. DETAILS: Hiện thông tin và danh sách sách thuộc thể loại (nếu trống hiện thông báo)
        public ActionResult Details(int? id)
        {
            ViewBag.Title = "Chi tiết Thể loại";
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Dùng Include để lấy danh sách sách thuộc thể loại này
            var category = db.Categories.Include(c => c.Books).FirstOrDefault(c => c.CategoryID == id);
            if (category == null) return HttpNotFound();

            return View(category);
        }

        // 3. CREATE (GET)
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm mới Thể loại";
            return View();
        }

        // CREATE (POST): Kiểm tra trùng tên (bỏ khoảng trắng, không phân biệt hoa thường)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult Create([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            ViewBag.Title = "Thêm mới Thể loại";

            if (!string.IsNullOrEmpty(category.CategoryName))
            {
                category.CategoryName = category.CategoryName.Trim();
                string lowerName = category.CategoryName.ToLower();

                // Kiểm tra trùng tên (không phân biệt hoa thường, bỏ khoảng trắng)
                bool isDuplicate = db.Categories.Any(c => c.CategoryName.Trim().ToLower() == lowerName);
                if (isDuplicate)
                {
                    ModelState.AddModelError("CategoryName", "Tên thể loại này đã tồn tại.");
                }
            }

            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();
                TempData["Success"] = "Thêm mới thể loại thành công!";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // 4. EDIT (GET)
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult Edit(int? id)
        {
            ViewBag.Title = "Chỉnh sửa Thể loại";
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Category category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();
            return View(category);
        }

        // EDIT (POST): Loại trừ chính bản ghi đang sửa khi check trùng tên
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult Edit([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            ViewBag.Title = "Chỉnh sửa Thể loại";

            if (!string.IsNullOrEmpty(category.CategoryName))
            {
                category.CategoryName = category.CategoryName.Trim();
                string lowerName = category.CategoryName.ToLower();

                // Loại trừ chính bản ghi đang sửa (CategoryID khác id hiện tại)
                bool isDuplicate = db.Categories.Any(c => c.CategoryID != category.CategoryID && c.CategoryName.Trim().ToLower() == lowerName);
                if (isDuplicate)
                {
                    ModelState.AddModelError("CategoryName", "Tên thể loại này đã tồn tại ở thể loại khác.");
                }
            }

            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật thể loại thành công!";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // 5. DELETE (GET)
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult Delete(int? id)
        {
            ViewBag.Title = "Xóa Thể loại";
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Category category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();
            return View(category);
        }

        // DELETE (POST): Chặn xóa khi còn sách + Bắt lỗi khóa ngoại CSDL
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Category category = db.Categories.Include(c => c.Books).FirstOrDefault(c => c.CategoryID == id);
                if (category == null) return HttpNotFound();

                // Kiểm tra trước bằng code xem còn sách không
                if (category.Books != null && category.Books.Any())
                {
                    TempData["Error"] = "Không thể xóa thể loại này vì vẫn còn sách bên trong!";
                    return RedirectToAction("Index");
                }

                db.Categories.Remove(category);
                db.SaveChanges();
                TempData["Success"] = "Xóa thể loại thành công!";
            }
            catch (Exception)
            {
                // Bắt lỗi khi vướng khóa ngoại từ tầng CSDL phòng hờ
                TempData["Error"] = "Lỗi cơ sở dữ liệu: Không thể xóa bản ghi đang có ràng buộc khóa ngoại.";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}