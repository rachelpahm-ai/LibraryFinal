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
    public class MembersController : Controller
    {
        private LibraryManagementEntities db = new LibraryManagementEntities();

        // GET: Members
        public ActionResult Index(string searchTerm, string statusFilter)
        {
            var members = db.Members.AsQueryable();

            // Tìm kiếm gộp trên 3 cột: Họ tên, Email, Số điện thoại
            if (!string.IsNullOrEmpty(searchTerm))
            {
                members = members.Where(m => m.FullName.Contains(searchTerm) ||
                                           m.Email.Contains(searchTerm) ||
                                           m.Phone.Contains(searchTerm));
            }

            // Lọc theo trạng thái (Xử lý an toàn cho cả trường hợp giá trị NULL trong DB)
            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "True")
                {
                    members = members.Where(m => m.IsLocked == true);
                }
                else if (statusFilter == "False")
                {
                    members = members.Where(m => m.IsLocked == false || m.IsLocked == null);
                }
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.StatusFilter = statusFilter;

            return View(members.OrderBy(m => m.FullName).ToList());
        }

        // GET: Members/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Nạp kèm lịch sử mượn sách an toàn bằng Include chuỗi
            Member member = db.Members
                              .Include("Loans")
                              .FirstOrDefault(m => m.MemberID == id);

            if (member == null)
            {
                return HttpNotFound();
            }
            return View(member);
        }

        // GET: Members/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Members/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MemberID,FullName,Email,Phone,Address,IsLocked")] Member member)
        {
            if (ModelState.IsValid)
            {
                member.Email = member.Email?.Trim().ToLower();
                member.Phone = member.Phone?.Trim();
                member.FullName = member.FullName?.Trim();
                member.Address = member.Address?.Trim();

                // Kiểm tra không trùng lặp Email hoặc SĐT
                if (db.Members.Any(m => m.Email == member.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã tồn tại trong hệ thống.");
                    return View(member);
                }
                if (db.Members.Any(m => m.Phone == member.Phone))
                {
                    ModelState.AddModelError("Phone", "Số điện thoại này đã tồn tại trong hệ thống.");
                    return View(member);
                }

                db.Members.Add(member);
                db.SaveChanges();
                TempData["Success"] = "Thêm thành viên thành công!";
                return RedirectToAction("Index");
            }

            return View(member);
        }

        // GET: Members/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Member member = db.Members.Find(id);
            if (member == null)
            {
                return HttpNotFound();
            }
            return View(member);
        }

        // POST: Members/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MemberID,FullName,Email,Phone,Address,CardExpiryDate,IsLocked")] Member member)
        {
            if (ModelState.IsValid)
            {
                member.Email = member.Email?.Trim().ToLower();
                member.Phone = member.Phone?.Trim();
                member.FullName = member.FullName?.Trim();
                member.Address = member.Address?.Trim();

                // Kiểm tra trùng lặp email/SĐT nhưng loại trừ chính ID hiện tại
                if (db.Members.Any(m => m.Email == member.Email && m.MemberID != member.MemberID))
                {
                    ModelState.AddModelError("Email", "Email này đã thuộc về thành viên khác.");
                    return View(member);
                }
                if (db.Members.Any(m => m.Phone == member.Phone && m.MemberID != member.MemberID))
                {
                    ModelState.AddModelError("Phone", "Số điện thoại này đã thuộc về thành viên khác.");
                    return View(member);
                }

                db.Entry(member).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Index");
            }
            return View(member);
        }

        // POST: Members/ToggleStatus/5 (Khóa / Mở khóa tài khoản)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            var member = db.Members.Find(id);
            if (member != null)
            {
                bool currentStatus = member.IsLocked ?? false;
                member.IsLocked = !currentStatus;

                db.Entry(member).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = $"Đã cập nhật trạng thái cho tài khoản {member.FullName}.";
            }
            return RedirectToAction("Index");
        }

        // GET: Members/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Member member = db.Members.Find(id);
            if (member == null)
            {
                return HttpNotFound();
            }
            return View(member);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Member member = db.Members.Find(id);

            // Chặn xóa nếu thành viên đã có lịch sử phiếu mượn
            if (member.Loans != null && member.Loans.Count > 0)
            {
                TempData["Error"] = "Không thể xóa thành viên đã có lịch sử mượn sách. Vui lòng sử dụng tính năng Khóa tài khoản.";
                return RedirectToAction("Index");
            }

            try
            {
                db.Members.Remove(member);
                db.SaveChanges();
                TempData["Success"] = "Xóa thành viên thành công!";
            }
            catch (Exception)
            {
                TempData["Error"] = "Lỗi khi xóa thành viên do vướng ràng buộc cơ sở dữ liệu.";
            }
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