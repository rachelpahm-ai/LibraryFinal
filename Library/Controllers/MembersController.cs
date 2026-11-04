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

        public ActionResult Index(string searchTerm, string statusFilter, string sortOrder, int? page)
        {
            ViewBag.SearchTerm = searchTerm;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.CurrentSort = sortOrder;

            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            var members = db.Members.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                members = members.Where(m => m.FullName.Contains(searchTerm) ||
                                           m.Email.Contains(searchTerm) ||
                                           m.Phone.Contains(searchTerm));
            }

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

            switch (sortOrder)
            {
                case "name_desc":
                    members = members.OrderByDescending(m => m.FullName);
                    break;
                default:
                    members = members.OrderBy(m => m.FullName);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            int totalItemCount = members.Count();

            var pagedMembers = members.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItemCount / pageSize);
            ViewBag.TotalItems = totalItemCount;

            return View(pagedMembers);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Member member = db.Members
                              .Include("Loans")
                              .FirstOrDefault(m => m.MemberID == id);

            if (member == null)
            {
                return HttpNotFound();
            }
            return View(member);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MemberID,FullName,Email,Phone,Address,CardExpiryDate,IsLocked")] Member member)
        {
            if (ModelState.IsValid)
            {
                member.Email = member.Email?.Trim().ToLower();
                member.Phone = member.Phone?.Trim();
                member.FullName = member.FullName?.Trim();
                member.Address = member.Address?.Trim();

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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Member member = db.Members.Find(id);

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