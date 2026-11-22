using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Library.Models;
using System.IO;
using Library.Filters;

namespace Library.Controllers
{
    public class BooksController : Controller
    {
        private bool IsValidBookImage(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength <= 0 || file.ContentLength > 2 * 1024 * 1024)
                return false;

            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                return false;

            var stream = file.InputStream;
            if (!stream.CanSeek) return false;

            stream.Position = 0;
            byte[] head = new byte[8];
            int read = stream.Read(head, 0, head.Length);
            stream.Position = 0;

            // Kiểm tra chữ ký file JPEG và PNG, không chỉ dựa vào phần mở rộng.
            bool jpg = read >= 3 && head[0] == 0xFF && head[1] == 0xD8 && head[2] == 0xFF;
            bool png = read >= 8 && head[0] == 0x89 && head[1] == 0x50 &&
                       head[2] == 0x4E && head[3] == 0x47 && head[4] == 0x0D &&
                       head[5] == 0x0A && head[6] == 0x1A && head[7] == 0x0A;

            return ext == ".png" ? png : jpg;
        }

        private string SaveBookImage(HttpPostedFileBase file)
        {
            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            string uniqueName = Guid.NewGuid().ToString("N") + ext;
            string folder = Server.MapPath("~/Content/images");
            Directory.CreateDirectory(folder);
            file.SaveAs(Path.Combine(folder, uniqueName));
            return uniqueName; // lưu tên file (không lưu đường dẫn tuyệt đối) vào SQL
        }
        private LibraryManagementEntities db = new LibraryManagementEntities();
        // Kiểm tra người dùng có quyền quản lý sách hay không
        private bool CanManageBooks()
        {
            if (User == null ||
                User.Identity == null ||
                !User.Identity.IsAuthenticated)
            {
                return false;
            }

            string username = User.Identity.Name;

            string role = db.Accounts
                .Where(a => a.Username == username)
                .Select(a => a.Role)
                .FirstOrDefault();

            role = (role ?? "").Trim();

            return string.Equals(role, "Admin",
                       StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "Librarian",
                       StringComparison.OrdinalIgnoreCase);
        }

        // GET: Books
        // Danh mục sách dùng chung cho tất cả người dùng
        public ActionResult Index()
        {
            bool canManage = CanManageBooks();

            ViewBag.CanManageBooks = canManage;
            ViewBag.Title = "Danh mục sách";

            var books = db.Books
                .AsNoTracking()
                .Include(b => b.Author)
                .Include(b => b.Category)
                .AsQueryable();

            // Khách và Reader chỉ xem sách đang hoạt động
            if (!canManage)
            {
                books = books.Where(b => b.IsActive == true);
            }

            // Không lọc StockQuantity:
            // sách hết hàng vẫn được hiển thị
            return View(
                books.OrderBy(b => b.Title).ToList()
            );
        }



        // GET: Books/Details/5
        // Xem thông tin chi tiết một cuốn sách
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            bool canManage = CanManageBooks();

            var book = db.Books
                .AsNoTracking()
                .Include(b => b.Author)
                .Include(b => b.Category)
                .SingleOrDefault(b => b.BookID == id.Value);

            if (book == null)
            {
                return HttpNotFound();
            }

            // Không công khai sách ngừng hoạt động
            if (!canManage && book.IsActive != true)
            {
                return HttpNotFound();
            }

            ViewBag.CanManageBooks = canManage;
            ViewBag.Title = "Chi tiết sách";

            return View(book);
        }


        // GET: Books/Create
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult Create()
        {
            ViewBag.AuthorID = new SelectList(db.Authors, "AuthorID", "AuthorName");
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult Create(
        [Bind(Include = "Title,CategoryID,AuthorID,Price,StockQuantity,Description,IsActive")] Book book, HttpPostedFileBase ImageFile)
        {

            if (ImageFile != null && ImageFile.ContentLength > 0 && !IsValidBookImage(ImageFile))
            {
                ModelState.AddModelError("ImageFile", "Chỉ chấp nhận ảnh JPG/JPEG/PNG tối đa 2 MB.");
            }

            if (book.StockQuantity < 0)
            {
                ModelState.AddModelError(
                    "StockQuantity",
                    "Số lượng sách không được nhỏ hơn 0."
                );
            }

            if (book.Price < 0)
            {
                ModelState.AddModelError(
                    "Price",
                    "Giá sách không được nhỏ hơn 0."
                );
            }
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    book.ImageUrl = SaveBookImage(ImageFile);
                }

                db.Books.Add(book);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AuthorID = new SelectList(db.Authors, "AuthorID", "AuthorName", book.AuthorID);
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", book.CategoryID);
            return View(book);
        }


        // GET: Books/Edit/5
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            ViewBag.AuthorID = new SelectList(db.Authors, "AuthorID", "AuthorName", book.AuthorID);
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", book.CategoryID);
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult Edit(
    [Bind(Include = "BookID,Title,CategoryID,AuthorID,Price,StockQuantity,Description,IsActive")] Book book,
    HttpPostedFileBase ImageFile)
        {
            if (ImageFile != null && ImageFile.ContentLength > 0 && !IsValidBookImage(ImageFile))
            {
                ModelState.AddModelError("ImageFile", "Chỉ chấp nhận ảnh JPG/JPEG/PNG tối đa 2 MB.");
            }

            var current = db.Books.Find(book.BookID);
            if (current == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                current.Title = book.Title;
                current.CategoryID = book.CategoryID;
                current.AuthorID = book.AuthorID;
                current.Price = book.Price;
                current.StockQuantity = book.StockQuantity;
                current.Description = book.Description;
                current.IsActive = book.IsActive;

                // Không chọn ảnh mới -> KHÔNG ĐỔI ImageUrl.
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    current.ImageUrl = SaveBookImage(ImageFile);
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Giữ preview ảnh cũ trong trường hợp form có lỗi validation.
            book.ImageUrl = current.ImageUrl;
            ViewBag.AuthorID = new SelectList(db.Authors, "AuthorID", "AuthorName", book.AuthorID);
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", book.CategoryID);
            return View(book);
        }


        // GET: Books/Delete/5
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Librarian")]
        public ActionResult DeleteConfirmed(int id)
        {
            Book book = db.Books.Find(id);
            db.Books.Remove(book);
            db.SaveChanges();
            return RedirectToAction("Index");
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
