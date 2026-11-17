
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Library.Models;
using Library.Filters;

namespace Library.Controllers
{
    // Controller xử lý trang chủ thư viện BookNest
    public class LibraryController : Controller
    {
        // GET: Library/Index
        // Trang chủ thư viện - lấy dữ liệu từ SQL Server
        public ActionResult Index(string search)
        {
            // Tránh trình duyệt dùng lại trang đã cache
            // khi người dùng đăng nhập hoặc đăng xuất.
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            using (var db = new LibraryManagementEntities())
            {
                // ========================================
                // 1. XÁC ĐỊNH VAI TRÒ NGƯỜI DÙNG
                // ========================================

                string role = null;

                if (User != null &&
                    User.Identity != null &&
                    User.Identity.IsAuthenticated)
                {
                    string username = User.Identity.Name;

                    role = db.Accounts
                        .AsNoTracking()
                        .Where(a => a.Username == username)
                        .Select(a => a.Role)
                        .FirstOrDefault();
                }

                // Chuẩn hóa vai trò
                role = (role ?? "").Trim();

                bool isAdmin = string.Equals(
                    role,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase);

                bool isLibrarian = string.Equals(
                    role,
                    "Librarian",
                    StringComparison.OrdinalIgnoreCase);

                // Chỉ nhân viên được xem thống kê quản lý
                bool showStats = isAdmin || isLibrarian;

                // Truyền trạng thái hiển thị sang View
                ViewBag.ShowManagementStats = showStats;


                // ========================================
                // 2. THỐNG KÊ TỔNG ĐẦU SÁCH
                // ========================================

                // Tất cả người dùng đều được xem
                // tổng số đầu sách trong thư viện.
                ViewBag.TotalBooks = db.Books.Count();


                // ========================================
                // 3. THỐNG KÊ DÀNH CHO ADMIN / LIBRARIAN
                // ========================================

                if (showStats)
                {
                    // Tổng số bản sách còn trong kho
                    ViewBag.Available = db.Books
                        .Where(b => b.IsActive == true)
                        .Sum(b => (int?)b.StockQuantity) ?? 0;

                    // Tổng số thể loại
                    ViewBag.TotalCategories =
                        db.Categories.Count();

                    // Tổng thành viên đang hoạt động
                    ViewBag.TotalMembers = db.Members
                        .Count(m => m.IsLocked == false);

                    // Tổng phiếu mượn chưa trả
                    ViewBag.TotalLoans = db.Loans
                        .Count(l => l.ReturnDate == null);

                    // Tổng yêu cầu đặt trước đang chờ
                    ViewBag.PendingReservations =
                        db.Reservations
                            .Count(r => r.Status == "Chờ");
                }


                // ========================================
                // 4. TRUY VẤN DANH SÁCH SÁCH
                // ========================================

                var query =
                    from b in db.Books.AsNoTracking()
                    join c in db.Categories
                        on b.CategoryID equals c.CategoryID
                    join a in db.Authors
                        on b.AuthorID equals a.AuthorID
                    where b.IsActive == true
                    select new { b, c, a };


                // ========================================
                // 5. TÌM KIẾM SÁCH
                // ========================================

                if (!string.IsNullOrWhiteSpace(search))
                {
                    string keyword = search.Trim();

                    query = query.Where(x =>
                        x.b.Title.Contains(keyword) ||
                        x.a.AuthorName.Contains(keyword) ||
                        x.c.CategoryName.Contains(keyword));
                }


                // ========================================
                // 6. CHUYỂN SANG BOOKCARDVIEWMODEL
                // ========================================

                var books = query
                    .OrderByDescending(x => x.b.BookID)
                    .Select(x => new BookCardViewModel
                    {
                        BookID = x.b.BookID,
                        Title = x.b.Title,
                        CategoryName = x.c.CategoryName,
                        AuthorName = x.a.AuthorName,
                        Price = x.b.Price,
                        StockQuantity = x.b.StockQuantity,
                        ImageUrl = x.b.ImageUrl,
                        Description = x.b.Description,
                        IsActive = x.b.IsActive
                    })
                    .ToList();


                // ========================================
                // 7. TRUYỀN DỮ LIỆU SANG VIEW
                // ========================================

                ViewBag.Search = search;

                return View(books);
            }
        }


        // ========================================
        // TRANG GIỚI THIỆU
        // ========================================

        // GET: Library/About
        public ActionResult About()
        {
            return View();
        }


        // ========================================
        // KIỂM TRA KẾT NỐI DATABASE
        // ========================================

        // Chỉ Admin được truy cập
        [RoleAuthorize("Admin")]
        public ActionResult TestDatabase()
        {
            using (var db = new LibraryManagementEntities())
            {
                int totalBooks = db.Database
                    .SqlQuery<int>(
                        "SELECT COUNT(*) FROM dbo.Book")
                    .Single();

                return Content(
                    "Kết nối database thành công! " +
                    "Tổng số sách: " + totalBooks);
            }
        }
    }
}
