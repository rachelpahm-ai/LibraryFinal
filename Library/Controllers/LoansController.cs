using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using Library.Models;

namespace Library.Controllers
{
    public class LoansController : Controller
    {
       //kết nối database
        private LibraryManagementEntities db = new LibraryManagementEntities();

        
        public ActionResult Index(
            string searchName,
            string status,
            string sortOrder,
            int page = 1)

        {
            
            var loans = db.Loans
                .Include(l => l.Member)
                .Include(l => l.LoanDetails)
                .AsQueryable();

            //tìm kiếm
            if (!string.IsNullOrEmpty(searchName))
            {
                loans = loans.Where(l => l.Member.FullName.Contains(searchName));
            }

      
            //lọc 
            if (status == "DangMuon")
            {
                loans = loans.Where(l => l.ReturnDate == null);
            }
            else if (status == "DaTra")
            {
                loans = loans.Where(l => l.ReturnDate != null);
            }

            //sắp xếp
            if (sortOrder == "oldest")
            {
                loans = loans.OrderBy(l => l.LoanDate);
            }
            else
            {
                loans = loans.OrderByDescending(l => l.LoanDate);
            }

            int totalRecords = loans.Count();

            int pageSize = 10;

            int totalPages = (int)Math.Ceiling( (double)totalRecords / pageSize );

            if (page < 1)
                page = 1;

            if (totalPages > 0 && page > totalPages)
                page = totalPages;
            //phân trang
            var result = loans
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.SearchName = searchName;
            ViewBag.Status = status;
            ViewBag.SortOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(result);
        }
        // GET: Loans/Create
        public ActionResult Create()
        {
            var model = new LoanCreateViewModel();

            // Ngày mượn luôn lấy ngày hiện tại
            model.LoanDate = DateTime.Today;

            // Hạn trả mặc định sau 7 ngày
            model.DueDate = DateTime.Today.AddDays(7);

            LoadCreateLists(model);

            return View(model);
        }
        // POST: Loans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LoanCreateViewModel model)
        {
            model.LoanDate = DateTime.Today;

            // Kiểm tra hạn trả
            if (!model.DueDate.HasValue)
            {
                ModelState.AddModelError(
                    "DueDate", "Vui lòng chọn hạn trả");
            }
            else if (model.DueDate.Value.Date <= DateTime.Today)
            {
                ModelState.AddModelError(
                    "DueDate", "Hạn trả phải sau ngày mượn");
            }
            else if (model.DueDate.Value.Date > DateTime.Today.AddDays(30))
            {
                ModelState.AddModelError(
                    "DueDate", "Hạn trả không được quá 30 ngày");
            }

            // Kiểm tra thành viên
            var member = db.Members.FirstOrDefault(m =>
                m.MemberID == model.MemberID &&
                m.IsLocked == false &&
                (!m.CardExpiryDate.HasValue ||
                 m.CardExpiryDate.Value >= DateTime.Today));

            if (member == null)
            {
                ModelState.AddModelError(
                    "MemberID", "Thành viên không hợp lệ hoặc thẻ đã hết hạn");
            }

            // Kiểm tra danh sách sách
            if (model.SelectedBookIDs == null ||
                model.SelectedBookIDs.Count == 0)
            {
                ModelState.AddModelError(
                    "SelectedBookIDs", "Vui lòng chọn ít nhất một sách");
            }
            else
            {
                if (model.SelectedBookIDs.Distinct().Count()
                    != model.SelectedBookIDs.Count)
                {
                    ModelState.AddModelError(
                        "SelectedBookIDs", "Không được chọn trùng sách");
                }
            }

            var selectedBookIDs = model.SelectedBookIDs == null? new int[0]: model.SelectedBookIDs.ToArray();
            var books = db.Books.Where(b =>selectedBookIDs.Contains(b.BookID) &&b.IsActive == true && b.StockQuantity > 0).ToList();

            if (books.Count != selectedBookIDs.Distinct().Count())
            {
                ModelState.AddModelError(
                    "SelectedBookIDs",
                    "Có sách không tồn tại, đã ngừng hoạt động hoặc hết hàng");
            }

            // Nếu có lỗi thì quay lại form
            if (!ModelState.IsValid)
            {
                LoadCreateLists(model);
                return View(model);
            }

            try
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    // Tạo phiếu mượn
                    var loan = new Loan
                    {
                        MemberID = model.MemberID.Value,
                        LoanDate = DateTime.Today,
                        DueDate = model.DueDate.Value.Date,
                        ReturnDate = null,
                        TotalFine = 0
                    };

                    // Thêm các sách vào chi tiết phiếu
                    foreach (var book in books)
                    {
                        loan.LoanDetails.Add(new LoanDetail
                        {
                            BookID = book.BookID,
                            Quantity = 1,
                            FineAmount = 0
                        });
                    }

                    // Lưu dữ liệu
                    db.Loans.Add(loan);
                    db.SaveChanges();

                    transaction.Commit();
                }

                TempData["Success"] = "Tạo phiếu mượn thành công!";

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "",
                    "Không thể tạo phiếu mượn. Vui lòng kiểm tra lại.");

                LoadCreateLists(model);
                return View(model);
            }
        }

        // GET: Loans/Details/5
        public ActionResult Details(int id)
        {
            var loan = db.Loans
                .Include(l => l.Member)
                .Include(l => l.LoanDetails.Select(d => d.Book.Author))
                .FirstOrDefault(l => l.LoanID == id);

            if (loan == null)
            {
                return HttpNotFound();
            }

            return View(loan);
        }

        // GET: Loans/Cancel/5
        public ActionResult Cancel(int id)
        {
            var loan = db.Loans
                .Include(l => l.Member)
                .Include(l => l.LoanDetails)
                .FirstOrDefault(l => l.LoanID == id);

            if (loan == null)
            {
                return HttpNotFound();
            }

            if (loan.ReturnDate != null)
            {
                TempData["Error"] = "Không thể hủy phiếu đã trả.";
                return RedirectToAction("Index");
            }

            return View(loan);
        }

        // POST: Loans/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelConfirmed(int id)
        {
            var loan = db.Loans
                .Include(l => l.LoanDetails)
                .FirstOrDefault(l => l.LoanID == id);

            if (loan == null)
            {
                return HttpNotFound();
            }

            if (loan.ReturnDate != null)
            {
                TempData["Error"] = "Không thể hủy phiếu đã trả.";
                return RedirectToAction("Index");
            }

            db.LoanDetails.RemoveRange(loan.LoanDetails);
            db.Loans.Remove(loan);
            db.SaveChanges();

            TempData["Success"] = "Hủy phiếu mượn thành công.";

            return RedirectToAction("Index");
        }



        // Tải danh sách thành viên và sách cho form
        private void LoadCreateLists(LoanCreateViewModel model)
        {
            // Danh sách thành viên đủ điều kiện mượn
            model.Members = db.Members
                .Where(m =>
                    m.IsLocked == false &&
                    (!m.CardExpiryDate.HasValue ||
                     m.CardExpiryDate.Value >= DateTime.Today))
                .OrderBy(m => m.FullName)
                .Select(m => new SelectListItem
                {
                    Value = m.MemberID.ToString(),
                    Text = m.FullName,
                    Selected = model.MemberID == m.MemberID
                })
                .ToList();

            // Danh sách sách đang hoạt động và còn hàng
            var selectedBookIDs = model.SelectedBookIDs == null
                ? new int[0]
                : model.SelectedBookIDs.ToArray();

            var availableBooks = db.Books
                .Where(b => b.IsActive == true && b.StockQuantity > 0)
                .OrderBy(b => b.Title)
                .ToList();

            model.Books = availableBooks
                .Select(b => new SelectListItem
                {
                    Value = b.BookID.ToString(),
                    Text = b.Title,
                    Selected = selectedBookIDs.Contains(b.BookID)
                })
                .ToList();
        }

    }
}