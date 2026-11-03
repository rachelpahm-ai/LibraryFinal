using System;
using System.ComponentModel.DataAnnotations;

namespace Library.Models
{
    [MetadataType(typeof(LoanMetadata))]
    public partial class Loan
    {
    }

    public class LoanMetadata
    {
        [Display(Name = "Mã phiếu mượn")]
        public int LoanID { get; set; }

        [Display(Name = "Mã thành viên")]
        [Required(ErrorMessage = "Vui lòng chọn thành viên")]
        public int MemberID { get; set; }

        [Display(Name = "Ngày mượn")]
        [DataType(DataType.Date)]
        public DateTime LoanDate { get; set; }

        [Display(Name = "Hạn trả")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Display(Name = "Ngày trả")]
        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "Tổng tiền phạt")]
        public decimal TotalFine { get; set; }
    }
}