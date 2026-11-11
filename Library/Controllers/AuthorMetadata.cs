using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Library.Controllers
{
    [MetadataType(typeof(AuthorMetadata))]
    public partial class Author // Hoặc tên thực tế trong EDMX của bạn (vd: TacGia)
    {
    }
    public class AuthorMetadata
    {
        [Display(Name = "Mã tác giả")]
        public int AuthorID { get; set; }

        [Display(Name = "Tên tác giả")]
        [Required(ErrorMessage = "Tên tác giả không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên tác giả tối đa 100 ký tự.")]
        public string AuthorName { get; set; }

        [Display(Name = "Tiểu sử / Mô tả")]
        [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự.")]
        public string Bio { get; set; }
    }

}