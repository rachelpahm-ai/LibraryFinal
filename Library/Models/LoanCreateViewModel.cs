using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Library.Models
{
    public class LoanCreateViewModel
    {
        // Thành viên được chọn để mượn sách
        [Required(ErrorMessage = "Vui lòng chọn thành viên")]
        public int? MemberID { get; set; }

        // Danh sách mã sách được chọn
        [Required(ErrorMessage = "Vui lòng chọn ít nhất một sách")]
        public List<int> SelectedBookIDs { get; set; }

        // Ngày mượn
        public DateTime LoanDate { get; set; }

        // Hạn trả
        [Required(ErrorMessage = "Vui lòng chọn hạn trả")]
        public DateTime? DueDate { get; set; }

        // Danh sách thành viên hiển thị trên dropdown
        public IEnumerable<SelectListItem> Members { get; set; }

        // Danh sách sách hiển thị trên form
        public IEnumerable<SelectListItem> Books { get; set; }

        // Khởi tạo danh sách khi tạo ViewModel
        public LoanCreateViewModel()
        {
            SelectedBookIDs = new List<int>();
            Members = new List<SelectListItem>();
            Books = new List<SelectListItem>();
            LoanDate = DateTime.Today;
            DueDate = DateTime.Today.AddDays(7);
        }
    }
}