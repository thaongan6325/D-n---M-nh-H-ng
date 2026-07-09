using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vivu_Xe.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VivuXe.Models
{
    [Table("ThanhToan")]
    public class ThanhToan
    {
        [Key]
        public int MaThanhToan { get; set; }

        public int MaDon { get; set; }

        public DateTime? NgayThanhToan { get; set; }
        public decimal SoTien { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public string LoaiThanhToan { get; set; }
        public string MaGiaoDichUnique { get; set; }
        public string TrangThai { get; set; }
        public string GhiChu { get; set; }

        // Khóa ngoại trỏ về DatXe
        [ForeignKey("MaDon")]
        public virtual DonDatXe DonDatXe { get; set; }
    }
}