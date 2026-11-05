using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Library.Models
{
    public class LoanCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn thành viên")]
        public int? MemberID { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ít nhất một sách")]
        public List<int> SelectedBookIDs { get; set; }

        public DateTime LoanDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hạn trả")]
        public DateTime? DueDate { get; set; }

        public IEnumerable<SelectListItem> Members { get; set; }

        public IEnumerable<SelectListItem> Books { get; set; }

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