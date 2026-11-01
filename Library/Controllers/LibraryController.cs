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
        public ActionResult Index()
        {
            // Tạo danh sách sách mẫu
            var books = new List<BookCardViewModel>
            {
                // Sách mẫu 1
                new BookCardViewModel
                {
                    BookID = 1,
                    Title = "Clean Code",
                    CategoryName = "Lập trình",
                    AuthorName = "Robert C. Martin",
                    Price = 450000,
                    StockQuantity = 10,
                    ImageUrl = "book-01.jpg",
                    Description = "Handbook of Agile Software Craftsmanship",
                    IsActive = true
                },

                // Sách mẫu 2
                new BookCardViewModel
                {
                    BookID = 2,
                    Title = "Cho Tôi Xin Một Vé Đi Tuổi Thơ",
                    CategoryName = "Văn học",
                    AuthorName = "Nguyễn Nhật Ánh",
                    Price = 85000,
                    StockQuantity = 15,
                    ImageUrl = "book-02.jpg",
                    Description = "Tác phẩm nổi tiếng viết về tuổi thơ",
                    IsActive = true
                },

                // Sách mẫu 3
                new BookCardViewModel
                {
                    BookID = 3,
                    Title = "Đắc Nhân Tâm",
                    CategoryName = "Kỹ năng sống",
                    AuthorName = "Dale Carnegie",
                    Price = 98000,
                    StockQuantity = 20,
                    ImageUrl = "book-03.jpg",
                    Description = "Sách kỹ năng sống và nghệ thuật thu phục lòng người",
                    IsActive = true
                },

                // Sách mẫu 4
                new BookCardViewModel
                {
                    BookID = 4,
                    Title = "Lược Sử Thời Gian",
                    CategoryName = "Khoa học",
                    AuthorName = "Stephen Hawking",
                    Price = 150000,
                    StockQuantity = 8,
                    ImageUrl = "book-04.jpg",
                    Description = "Cuốn sách kinh điển về vũ trụ học",
                    IsActive = true
                }
            };

            // Truyền danh sách books sang View Index
            return View(books);
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