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
    public class AuthorsController : Controller
    {
        private LibraryManagementEntities db = new LibraryManagementEntities();

        public ActionResult Index(string searchString, string filter, int? page)
        {
            ViewBag.Title = "Quản lý Tác giả";
            int pageSize = 8;
            int pageNumber = (page ?? 1);

            var authors = db.Authors.AsQueryable();

            // Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                authors = authors.Where(a => a.AuthorName.Contains(searchString));
                ViewBag.CurrentSearch = searchString;
            }

            // Bộ lọc: Tất cả / Có sách / Chưa có sách
            ViewBag.CurrentFilter = filter;
            if (filter == "HasBooks")
            {
                authors = authors.Where(a => a.Books.Any());
            }
            else if (filter == "NoBooks")
            {
                authors = authors.Where(a => !a.Books.Any());
            }

            authors = authors.OrderBy(a => a.AuthorName);

            int totalItemCount = authors.Count();
            int totalPages = (int)Math.Ceiling((double)totalItemCount / pageSize);
            if (pageNumber > totalPages && totalPages > 0) pageNumber = totalPages;
            if (pageNumber < 1) pageNumber = 1;

            // Đã sửa lại lỗi cú pháp ở đây
            var list = authors.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Page = pageNumber;
            ViewBag.TotalPages = totalPages;

            return View(list);
        }

        // DETAILS
        public ActionResult Details(int? id)
        {
            ViewBag.Title = "Chi tiết Tác giả";
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var author = db.Authors.Include(a => a.Books).FirstOrDefault(a => a.AuthorID == id);
            if (author == null) return HttpNotFound();

            return View(author);
        }

        // CREATE (GET & POST)
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm mới Tác giả";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult Create([Bind(Include = "AuthorID,AuthorName,Bio")] Library.Models.Author author)
        {
            ViewBag.Title = "Thêm mới Tác giả";

            if (!string.IsNullOrEmpty(author.AuthorName))
            {
                author.AuthorName = author.AuthorName.Trim();
                string lowerName = author.AuthorName.ToLower();

                if (db.Authors.Any(a => a.AuthorName.Trim().ToLower() == lowerName))
                {
                    ModelState.AddModelError("AuthorName", "Tên tác giả này đã tồn tại.");
                }
            }

            if (ModelState.IsValid)
            {
                db.Authors.Add(author);
                db.SaveChanges();
                TempData["Success"] = "Thêm mới tác giả thành công!";
                return RedirectToAction("Index");
            }
            return View(author);
        }

        // EDIT (GET & POST)
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult Edit(int? id)
        {
            ViewBag.Title = "Chỉnh sửa Tác giả";
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Library.Models.Author author = db.Authors.Find(id);
            if (author == null) return HttpNotFound();

            return View(author);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult Edit([Bind(Include = "AuthorID,AuthorName,Bio")] Library.Models.Author author)
        {
            ViewBag.Title = "Chỉnh sửa Tác giả";

            if (!string.IsNullOrEmpty(author.AuthorName))
            {
                author.AuthorName = author.AuthorName.Trim();
                string lowerName = author.AuthorName.ToLower();

                if (db.Authors.Any(a => a.AuthorID != author.AuthorID && a.AuthorName.Trim().ToLower() == lowerName))
                {
                    ModelState.AddModelError("AuthorName", "Tên tác giả này đã tồn tại ở tác giả khác.");
                }
            }

            if (ModelState.IsValid)
            {
                db.Entry(author).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật thông tin tác giả thành công!";
                return RedirectToAction("Index");
            }
            return View(author);
        }

        // DELETE (GET & POST)
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult Delete(int? id)
        {
            ViewBag.Title = "Xóa Tác giả";
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Library.Models.Author author = db.Authors.Find(id);
            if (author == null) return HttpNotFound();

            return View(author);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Library.Models.Author author = db.Authors.Include(a => a.Books).FirstOrDefault(a => a.AuthorID == id);
                if (author == null) return HttpNotFound();

                // Chặn xóa khi vẫn còn sách do tác giả sáng tác[cite: 1, 7]
                if (author.Books != null && author.Books.Any())
                {
                    TempData["Error"] = "Không thể xóa tác giả này vì vẫn còn sách do tác giả sáng tác!";
                    return RedirectToAction("Index");
                }

                db.Authors.Remove(author);
                db.SaveChanges();
                TempData["Success"] = "Xóa tác giả thành công!";
            }
            catch (Exception)
            {
                TempData["Error"] = "Lỗi cơ sở dữ liệu: Không thể xóa do vướng khóa ngoại.";
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