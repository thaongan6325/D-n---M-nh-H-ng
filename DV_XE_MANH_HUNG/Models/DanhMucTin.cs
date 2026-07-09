using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vivu_Xe.Models
{
    [Table("DanhMucTin")]
    public class DanhMucTin
    {
        [Key]
        public int MaDanhMuc { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(100)]
        public string TenDanhMuc { get; set; }

        [StringLength(255)]
        public string MoTa { get; set; }

        public bool? TrangThai { get; set; } // BIT trong SQL map sang bool?

        // Navigation property (Một danh mục có nhiều bài viết)
        public virtual ICollection<TinTuc> TinTucs { get; set; } = new List<TinTuc>();
    }
}