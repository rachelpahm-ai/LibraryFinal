using System.ComponentModel.DataAnnotations;

namespace Library.Models.ViewModels
{
    public class UserCreateViewModel
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [MinLength(3,
            ErrorMessage = "Tên đăng nhập phải có ít nhất 3 ký tự.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$",
            ErrorMessage =
                "Tên đăng nhập chỉ gồm chữ không dấu, số và dấu gạch dưới.")]
        public string Username { get; set; }

        [Display(Name = "Vai trò")]
        [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
        [RegularExpression(@"^(Admin|Librarian)$",
            ErrorMessage = "Vai trò chỉ được là Admin hoặc Librarian.")]
        public string Role { get; set; }

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [MinLength(6,
            ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Xác nhận mật khẩu")]
        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
        [DataType(DataType.Password)]
        [Compare("Password",
            ErrorMessage = "Xác nhận mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }
    }
}