using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Library.Models;

namespace Library.Controllers
{
    public class ReservationsController : Controller
    {
        private LibraryManagementEntities db = new LibraryManagementEntities();

        // GET: Reservations
        public ActionResult Index(
            string search,
            string status,
            string sort = "newest",
            int page = 1)
        {
            ViewBag.Title = "Quản lý đặt trước";

            int pageSize = 10;

            var query = db.Reservations
                .Include(r => r.Book)
                .Include(r => r.Member)
                .AsQueryable();

            // Tìm theo tên thành viên hoặc tên sách
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(r =>
                    r.Member.FullName.Contains(search) ||
                    r.Book.Title.Contains(search));
            }

            // Lọc trạng thái
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.Status == status);
            }

            // Sắp xếp theo ngày, sau đó theo mã để phân trang ổn định
            sort = sort == "oldest" ? "oldest" : "newest";

            var ordered = sort == "oldest"
                ? query.OrderBy(r => r.ReserveDate)
                       .ThenBy(r => r.ReserveID)
                : query.OrderByDescending(r => r.ReserveDate)
                       .ThenByDescending(r => r.ReserveID);

            int totalItems = query.Count();
            int totalPages = Math.Max(
                1, (int)Math.Ceiling(totalItems / (double)pageSize));

            page = Math.Max(1, Math.Min(page, totalPages));

            ViewBag.Search = search;
            ViewBag.CurrentStatus = status;
            ViewBag.Sort = sort;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList());
        }

        // GET: Reservations/Details/5
        public ActionResult Details(int? id)
        {
            ViewBag.Title = "Chi tiết đặt trước";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var reservation = db.Reservations
                .Include(r => r.Member)
                .Include(r => r.Book)
                .SingleOrDefault(r => r.ReserveID == id.Value);

            if (reservation == null)
            {
                return HttpNotFound();
            }

            return View(reservation);
        }

        // GET: Reservations/Create
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm đặt trước";

            ViewBag.MemberID = new SelectList(
                db.Members
                    .Where(m => m.IsLocked == false)
                    .OrderBy(m => m.FullName),
                "MemberID",
                "FullName");

            ViewBag.BookID = new SelectList(
                db.Books.OrderBy(b => b.Title),
                "BookID",
                "Title");

            return View(new Reservation
            {
                ReserveDate = DateTime.Today,
                Status = "Chờ"
            });
        }

        // POST: Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "MemberID,BookID")] Reservation reservation)
        {
            ViewBag.Title = "Thêm đặt trước";

            // Tự gán ở server, không lấy ngày và trạng thái từ form.
            reservation.ReserveDate = DateTime.Today;
            reservation.Status = "Chờ";

            ModelState.Remove("ReserveDate");
            ModelState.Remove("Status");

            // Thay thông báo bắt buộc mặc định của MVC bằng tiếng Việt.
            // Vẫn tự xác thực cả hai trường phía server bên dưới.
            ModelState.Remove("MemberID");
            ModelState.Remove("BookID");

            // Kiểm tra thành viên tồn tại và không bị khóa.
            bool memberValid = db.Members.Any(m =>
                m.MemberID == reservation.MemberID &&
                m.IsLocked == false);

            if (!memberValid)
            {
                ModelState.AddModelError(
                    "MemberID",
                    "Vui lòng chọn thành viên đang hoạt động.");
            }

            // Kiểm tra sách tồn tại.
            bool bookValid = db.Books.Any(b =>
                b.BookID == reservation.BookID);

            if (!bookValid)
            {
                ModelState.AddModelError(
                    "BookID",
                    "Vui lòng chọn sách hợp lệ.");
            }

            // Chặn cùng thành viên đặt cùng sách khi đang Chờ.
            bool duplicate = memberValid && bookValid && db.Reservations.Any(r =>
                r.MemberID == reservation.MemberID &&
                r.BookID == reservation.BookID &&
                r.Status == "Chờ");

            if (duplicate)
            {
                ModelState.AddModelError(
                    "",
                    "Thành viên đã có yêu cầu Chờ cho sách này.");
            }

            if (ModelState.IsValid)
            {
                db.Reservations.Add(reservation);
                db.SaveChanges();

                TempData["Success"] = "Thêm yêu cầu đặt trước thành công.";
                return RedirectToAction("Index");
            }

            // Nạp lại dropdown khi nhập sai.
            ViewBag.MemberID = new SelectList(
                db.Members
                    .Where(m => m.IsLocked == false)
                    .OrderBy(m => m.FullName),
                "MemberID",
                "FullName",
                reservation.MemberID);

            ViewBag.BookID = new SelectList(
                db.Books.OrderBy(b => b.Title),
                "BookID",
                "Title",
                reservation.BookID);

            return View(reservation);
        }

        // GET: Reservations/Edit/5
        public ActionResult Edit(int? id)
        {
            ViewBag.Title = "Cập nhật đặt trước";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var reservation = db.Reservations
                .Include(r => r.Member)
                .Include(r => r.Book)
                .SingleOrDefault(r => r.ReserveID == id.Value);

            if (reservation == null)
            {
                return HttpNotFound();
            }

            return View(reservation);
        }

        // POST: Reservations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "ReserveID,Status")] Reservation input)
        {
            ViewBag.Title = "Cập nhật đặt trước";

            // Lấy bản ghi gốc, không cập nhật thành viên/sách từ form.
            var reservation = db.Reservations
                .Include(r => r.Member)
                .Include(r => r.Book)
                .SingleOrDefault(r => r.ReserveID == input.ReserveID);

            if (reservation == null)
            {
                return HttpNotFound();
            }

            // Form sửa chỉ nhận mã và trạng thái.
            ModelState.Remove("MemberID");
            ModelState.Remove("BookID");
            ModelState.Remove("ReserveDate");
            // Xóa lỗi Required mặc định để thông báo trạng thái được Việt hóa.
            ModelState.Remove("Status");

            string newStatus = input.Status;

            if (newStatus != "Chờ" &&
                newStatus != "Đã nhận" &&
                newStatus != "Hủy")
            {
                ModelState.AddModelError(
                    "Status",
                    "Vui lòng chọn trạng thái hợp lệ.");
            }

            if ((reservation.Status == "Hủy" ||
                 reservation.Status == "Đã nhận") &&
                newStatus == "Chờ")
            {
                ModelState.AddModelError(
                    "Status",
                    "Không được chuyển từ Hủy hoặc Đã nhận về Chờ.");
            }

            if (ModelState.IsValid)
            {
                // Chỉ thay đổi trạng thái của bản ghi gốc.
                reservation.Status = newStatus;

                db.SaveChanges();

                TempData["Success"] = "Cập nhật đặt trước thành công.";
                return RedirectToAction("Index");
            }

            return View(reservation);
        }

        // GET: Reservations/Delete/5
        public ActionResult Delete(int? id)
        {
            ViewBag.Title = "Xóa đặt trước";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var reservation = db.Reservations
                .Include(r => r.Member)
                .Include(r => r.Book)
                .SingleOrDefault(r => r.ReserveID == id.Value);

            if (reservation == null)
            {
                return HttpNotFound();
            }

            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var reservation = db.Reservations
                .Include(r => r.Member)
                .Include(r => r.Book)
                .SingleOrDefault(r => r.ReserveID == id);

            if (reservation == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra lại ở server trước khi xóa.
            if (reservation.Status != "Hủy")
            {
                TempData["Error"] =
                    "Chỉ được xóa yêu cầu đã Hủy. " +
                    "Hãy sửa trạng thái sang Hủy trước.";

                return RedirectToAction("Delete", new { id });
            }

            db.Reservations.Remove(reservation);

            try
            {
                db.SaveChanges();

                TempData["Success"] = "Xóa yêu cầu đặt trước thành công.";
                return RedirectToAction("Index");
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                // Khôi phục trạng thái theo dõi khi xóa thất bại.
                db.Entry(reservation).State = EntityState.Unchanged;

                ViewBag.Title = "Xóa đặt trước";

                ModelState.AddModelError(
                    "",
                    "Không thể xóa yêu cầu này. " +
                    "Dữ liệu có thể đang được sử dụng hoặc đã thay đổi.");

                return View("Delete", reservation);
            }
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
