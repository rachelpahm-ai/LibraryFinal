using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Library.Models
{
    // ViewModel tạm chứa dữ liệu hiển thị trên Book Card
    public class BookCardViewModel
    {
        // Mã sách
        public int BookID { get; set; }

        // Tên sách
        public string Title { get; set; }

        // Tên thể loại (lấy từ bảng Category)
        public string CategoryName { get; set; }

        // Mô tả ngắn gọn về sách
        public string ShortDescription { get; set; }

        // Tên tác giả (lấy từ bảng Author)
        public string AuthorName { get; set; }

        // Giá sách
        public decimal Price { get; set; }

        // Số lượng sách trong kho
        public int StockQuantity { get; set; }

        // Tên file ảnh trong Content/images
        public string ImageUrl { get; set; }

        // Mô tả sách
        public string Description { get; set; }

        // Trạng thái hoạt động
        public bool? IsActive { get; set; }
    }

}