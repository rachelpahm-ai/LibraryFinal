using System;
using System.ComponentModel.DataAnnotations;

namespace Library.Models
{
    [MetadataType(typeof(MemberMetadata))]
    public partial class Member
    {
    }

    public class MemberMetadata
    {
        [Key]
        public int MemberID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên thành viên.")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng số 0 và đúng 10 chữ số.")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [Display(Name = "Địa chỉ")]
        [StringLength(250, ErrorMessage = "Địa chỉ không được vượt quá 250 ký tự.")]
        public string Address { get; set; }

        [Display(Name = "Ngày hết hạn thẻ")]
        public DateTime? CardExpiryDate { get; set; }

        [Display(Name = "Trạng thái khóa")]
        public bool IsLocked { get; set; }
    }
}