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
                (m.IsLocked == false || m.IsLocked == null) &&
                (!m.CardExpiryDate.HasValue ||
                 m.CardExpiryDate.Value >= DateTime.Today));

            if (member == null)
            {
                ModelState.AddModelError(
                    "MemberID", "Thành viên không hợp lệ hoặc thẻ đã hết hạn");
            }

            // BUSINESS LOGIC: GIỚI HẠN TỐI ĐA 5 QUYỂN
            if (member != null)
            {
                // Tổng số sách đang mượn và chưa trả
                int currentlyBorrowed = db.Loans
                    .Where(l => l.MemberID == member.MemberID
                             && l.ReturnDate == null)
                    .SelectMany(l => l.LoanDetails)
                    .Select(d => (int?)d.Quantity)
                    .Sum() ?? 0;
                
                // Mỗi sách được chọn tương ứng 1 quyển
                int newBorrowCount = model.SelectedBookIDs == null
                    ? 0
                    : model.SelectedBookIDs.Count;

                if (currentlyBorrowed + newBorrowCount > 5)
                {
                    ModelState.AddModelError(
                        "SelectedBookIDs",
                        "Độc giả đang mượn " + currentlyBorrowed +
                        " quyển. Chỉ được mượn tối đa 5 quyển chưa trả.");
                }
            }



            // BUSINESS LOGIC: CHẶN MƯỢN KHI CÓ SÁCH QUÁ HẠN
            if (member != null)
            {
                bool hasOverdueBooks = db.Loans.Any(l =>
                    l.MemberID == member.MemberID &&
                    l.ReturnDate == null &&
                    l.DueDate < DateTime.Today);

                if (hasOverdueBooks)
                {
                    ModelState.AddModelError(
                        "",
                        "Không thể mượn sách! Độc giả đang có " +
                        "sách quá hạn chưa trả. Vui lòng trả sách " +
                        "trước khi tạo phiếu mượn mới.");
                }
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
            
                    // Thêm sách vào chi tiết phiếu và giảm tồn kho
                    foreach (var book in books)
                    {
                        // Kiểm tra sách còn trong kho
                        if (book.StockQuantity < 1)
                        {
                            throw new InvalidOperationException(
                                "Sách đã hết hàng: " + book.Title);
                        }

                        // Thêm chi tiết phiếu mượn
                        loan.LoanDetails.Add(new LoanDetail
                        {
                            BookID = book.BookID,
                            Quantity = 1,
                            FineAmount = 0
                        });

                        // Giảm số lượng tồn kho
                        book.StockQuantity -= 1;
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


        // GET: Loans/Return/5

        // GET: Loans/Return/5
        // GET không được thay đổi dữ liệu.
        public ActionResult Return(int id)
        {
            return RedirectToAction("Details", new { id = id });
        }

        // POST: Loans/ReturnConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReturnConfirmed(int id)
        {
            try
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    var loan = db.Loans
                        .Include(l => l.LoanDetails)
                        .FirstOrDefault(l => l.LoanID == id);

                    if (loan == null)
                    {
                        return HttpNotFound();
                    }

                    // Không cho trả trùng
                    if (loan.ReturnDate != null)
                    {
                        TempData["Error"] = "Phiếu này đã được trả.";
                        return RedirectToAction("Details",
                            new { id = id });
                    }

                    DateTime returnDate = DateTime.Today;

                    int overdueDays = Math.Max(
                        0,
                        (returnDate - loan.DueDate.Date).Days
                    );

                    decimal totalFine = 0;

                    foreach (var detail in loan.LoanDetails)
                    {
                        var book = db.Books
                            .FirstOrDefault(b => b.BookID == detail.BookID);

                        if (book == null)
                        {
                            throw new InvalidOperationException(
                                "Không tìm thấy sách mã " + detail.BookID);
                        }

                        // Hoàn lại số lượng đã mượn
                        book.StockQuantity += detail.Quantity;

                        // Tiền phạt: 5.000 VNĐ/ngày/quyển
                        decimal fine =
                            overdueDays * 5000m * detail.Quantity;

                        detail.FineAmount = fine;
                        totalFine += fine;
                    }

                    // Cập nhật phiếu mượn
                    loan.ReturnDate = returnDate;
                    loan.TotalFine = totalFine;

                    // Lưu cùng một giao dịch
                    db.SaveChanges();
                    transaction.Commit();
                }

                TempData["Success"] =
                    "Trả sách thành công. Đã cập nhật tồn kho.";

                return RedirectToAction("Details",
                    new { id = id });
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Không thể trả sách. Dữ liệu chưa được cập nhật.";

                return RedirectToAction("Details",
                    new { id = id });
            }
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
            try
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    var loan = db.Loans
                        .Include(l => l.LoanDetails)
                        .FirstOrDefault(l => l.LoanID == id);

                    // Kiểm tra phiếu tồn tại
                    if (loan == null)
                    {
                        return HttpNotFound();
                    }

                    // Không được hủy phiếu đã trả
                    if (loan.ReturnDate != null)
                    {
                        TempData["Error"] = "Không thể hủy phiếu đã trả.";
                        return RedirectToAction("Index");
                    }

                    // Hoàn lại tồn kho cho từng sách trong phiếu
                    foreach (var detail in loan.LoanDetails)
                    {
                        var book = db.Books
                            .FirstOrDefault(b => b.BookID == detail.BookID);

                        if (book == null)
                        {
                            throw new InvalidOperationException(
                                "Không tìm thấy sách mã " + detail.BookID);
                        }

                        book.StockQuantity += detail.Quantity;
                    }

                    // Xóa chi tiết phiếu mượn
                    db.LoanDetails.RemoveRange(
                        loan.LoanDetails.ToList());

                    // Xóa phiếu mượn
                    db.Loans.Remove(loan);

                    // Lưu toàn bộ thay đổi
                    db.SaveChanges();
                    transaction.Commit();
                }

                TempData["Success"] =
                    "Hủy phiếu thành công. Đã hoàn lại số lượng sách vào kho.";

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Không thể hủy phiếu. Dữ liệu chưa được cập nhật.";

                return RedirectToAction("Index");
            }
        }



        // Tải danh sách thành viên và sách cho form
        private void LoadCreateLists(LoanCreateViewModel model)
        {
            // Danh sách thành viên đủ điều kiện mượn
            model.Members = db.Members
                .Where(m =>
                    (m.IsLocked == false || m.IsLocked == null) &&
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