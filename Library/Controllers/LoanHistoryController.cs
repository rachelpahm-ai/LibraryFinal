using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Library.Models;

namespace Library.Controllers
{
    // Tach khoi LoansController de khong anh huong nghiep vu muon/tra hien tai.
    [Authorize]
    public class LoanHistoryController : Controller
    {
        private readonly LibraryManagementEntities db = new LibraryManagementEntities();

        // GET: /LoanHistory               -> Reader xem lich su cua minh.
        // GET: /LoanHistory?memberId=123  -> Admin/Librarian xem lich su thanh vien 123.
        public ActionResult Index(int? memberId)
        {
            var account = db.Accounts.AsNoTracking()
                .FirstOrDefault(a => a.Username == User.Identity.Name);

            if (account == null)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            string role = (account.Role ?? "").Trim();
            bool isStaff = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "Librarian", StringComparison.OrdinalIgnoreCase);
            bool isReader = string.Equals(role, "Reader", StringComparison.OrdinalIgnoreCase);

            if (!isStaff && !isReader)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            int targetMemberId;
            if (isStaff)
            {
                // Thu thu va Admin phai chon thanh vien cu the.
                if (!memberId.HasValue)
                    return RedirectToAction("Index", "Members");

                targetMemberId = memberId.Value;
            }
            else
            {
                // Reader khong duoc sua URL de doc lich su cua nguoi khac.
                if (!account.MemberID.HasValue)
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden,
                        "Tai khoan chua duoc lien ket voi thanh vien.");

                targetMemberId = account.MemberID.Value;

                if (memberId.HasValue && memberId.Value != targetMemberId)
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var member = db.Members.AsNoTracking()
                .FirstOrDefault(m => m.MemberID == targetMemberId);
            if (member == null)
                return HttpNotFound();

            var history = db.Loans.AsNoTracking()
                .Where(l => l.MemberID == targetMemberId)
                .Include(l => l.LoanDetails.Select(d => d.Book))
                .OrderByDescending(l => l.LoanDate)
                .ThenByDescending(l => l.LoanID)
                .ToList();

            ViewBag.MemberName = member.FullName;
            ViewBag.IsStaff = isStaff;
            ViewBag.Title = "Lich su muon sach";

            return View(history);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
