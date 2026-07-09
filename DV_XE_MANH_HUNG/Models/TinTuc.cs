using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vivu_Xe.Models
{
    [Table("TinTuc")]
    public class TinTuc
    {
        [Key]
        public int MaTinTuc { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài viết")]
        [StringLength(255)]
        public string TieuDe { get; set; }

        [StringLength(500)]
        public string TomTat { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string NoiDung { get; set; }

        public string HinhAnhDaiDien { get; set; }

        public DateTime? NgayDang { get; set; }

        public int? LuotXem { get; set; }

        public bool? TrangThai { get; set; }

        // Khóa ngoại
        public int? MaDanhMuc { get; set; }
        public int? MaNguoiDung { get; set; }

        // Navigation properties
        [ForeignKey("MaDanhMuc")]
        public virtual DanhMucTin DanhMucTin { get; set; }

        [ForeignKey("MaNguoiDung")]
        public virtual NguoiDung TacGia { get; set; }
    }
}