using Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Library.Controllers
{
    // Controller xử lý các request của trang thư viện
    public class LibraryController : Controller
    {
        // GET: Library
        // Action Index xử lý URL /Library/Index

        // GET: Library
        // Trang chủ thư viện - lấy dữ liệu từ SQL Server
        public ActionResult Index(string search)
        {
            using (var db = new LibraryManagementEntities())
            {
                // 1. THỐNG KÊ TRANG CHỦ

                // Tổng số đầu sách trong database
                ViewBag.TotalBooks = db.Books.Count();

                // Tổng số bản sách đang có trong kho
                ViewBag.Available = db.Books
                    .Where(b => b.IsActive == true)
                    .Sum(b => (int?)b.StockQuantity) ?? 0;

                // Tổng số thể loại
                ViewBag.TotalCategories = db.Categories.Count();

                // Tổng thành viên đang hoạt động
                ViewBag.TotalMembers = db.Members
                    .Count(m => m.IsLocked == false);

                // Số phiếu chưa có ngày trả
                // Tạm sử dụng ReturnDate vì bảng Loan chưa có Status
                ViewBag.TotalLoans = db.Loans
                    .Count(l => l.ReturnDate == null);

                // Số yêu cầu đặt trước đang chờ xử lý
                ViewBag.PendingReservations = db.Reservations
                    .Count(r => r.Status == "Chờ xử lý");


                // 2. TRUY VẤN DANH SÁCH SÁCH

                var query =
                    from b in db.Books.AsNoTracking()
                    join c in db.Categories
                        on b.CategoryID equals c.CategoryID
                    join a in db.Authors
                        on b.AuthorID equals a.AuthorID
                    where b.IsActive == true
                    select new { b, c, a };


                // 3. TÌM KIẾM SÁCH

                if (!string.IsNullOrWhiteSpace(search))
                {
                    string keyword = search.Trim();

                    query = query.Where(x =>
                        x.b.Title.Contains(keyword) ||
                        x.a.AuthorName.Contains(keyword) ||
                        x.c.CategoryName.Contains(keyword)
                    );
                }


                // 4. CHUYỂN DỮ LIỆU SANG VIEWMODEL

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


                // 5. GIỮ LẠI TỪ KHÓA TÌM KIẾM

                ViewBag.Search = search;


                // 6. TRUYỀN DANH SÁCH SÁCH SANG VIEW

                return View(books);
            }
        }


        // GET: Library/About
        // Hiển thị trang Giới thiệu thư viện
        public ActionResult About()
        {
            return View();
        }


        // GET: Library/TestDatabase
        // Kiểm tra kết nối Entity Framework với SQL Server
        public ActionResult TestDatabase()
        {
            using (var db = new LibraryManagementEntities())
            {
                int totalBooks = db.Database
                    .SqlQuery<int>(
                        "SELECT COUNT(*) FROM dbo.Book"
                    )
                    .Single();

                return Content(
                    "Kết nối database thành công! " +
                    "Tổng số sách: " + totalBooks
                );
            }
        }


    }
}