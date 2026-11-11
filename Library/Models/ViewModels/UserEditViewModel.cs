using System.ComponentModel.DataAnnotations;

namespace Library.Models.ViewModels
{
    public class UserEditViewModel
    {
        public int AccountID { get; set; }

        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

        [Display(Name = "Vai trò")]
        [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
        [RegularExpression(@"^(Admin|Librarian)$",
            ErrorMessage = "Vai trò chỉ được là Admin hoặc Librarian.")]
        public string Role { get; set; }

        [Display(Name = "Mật khẩu mới")]
        [MinLength(6,
            ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = " Xác nhận mật khẩu mới.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword",
            ErrorMessage = "Xác nhận mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }
    }
}