using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Library.Models
{
    [MetadataType(typeof(CategoryMetadata))]
    public partial class Category
    {
        // Class nối kết với entity Category sinh từ EF
    }
    public class CategoryMetadata
    {
        [Display(Name = "Mã thể loại")]
        public int CategoryID { get; set; }

        [Display(Name = "Tên thể loại")]
        [Required(ErrorMessage = "Tên thể loại không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên thể loại tối đa 100 ký tự.")]
        public string CategoryName { get; set; }
    }
}