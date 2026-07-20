using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vivu_Xe.Models;

public partial class Xe
{
    public int MaXe { get; set; }

    [Required(ErrorMessage = "Yêu cầu nhập tên xe")]
    [StringLength(100, ErrorMessage = "Tên xe không được vượt quá 100 ký tự")]
    public string TenXe { get; set; } = null!;

    [Required(ErrorMessage = "Yêu cầu nhập biển số xe")]
    [StringLength(20, ErrorMessage = "Biển số xe không được vượt quá 20 ký tự")]
    [RegularExpression(@"^[0-9]{2}[A-Z]{1,2}-[0-9]{5}$",
    ErrorMessage = "Biển số xe không đúng định dạng. Ví dụ: 74A-12345")]
    public string BienSo { get; set; } = null!;

    [Required(ErrorMessage = "Yêu cầu chọn hãng xe")]
    public int? MaHang { get; set; }

    [Required(ErrorMessage = "Yêu cầu chọn loại xe")]
    public int? MaLoai { get; set; }

    // Cho phép để trống
    [StringLength(30, ErrorMessage = "Màu sắc không được vượt quá 30 ký tự")]
    public string? MauSac { get; set; }

    // Cho phép để trống, nhưng nếu nhập thì phải từ 1990 đến 2100
    [Range(1990, 2100, ErrorMessage = "Năm sản xuất không hợp lệ")]
    public int? NamSanXuat { get; set; }

    [Required(ErrorMessage = "Yêu cầu chọn hộp số")]
    public string? HopSo { get; set; }

    [Required(ErrorMessage = "Yêu cầu chọn nhiên liệu")]
    public string? NhienLieu { get; set; }

    [Required(ErrorMessage = "Yêu cầu nhập giá thuê")]
    [Range(
        typeof(decimal),
        "1",
        "999999999",
        ErrorMessage = "Giá thuê phải lớn hơn 0")]
    public decimal GiaThueNgay { get; set; }

    [Required(ErrorMessage = "Yêu cầu nhập tiền cọc")]
    [Range(
        typeof(decimal),
        "0",
        "999999999",
        ErrorMessage = "Tiền cọc không được nhỏ hơn 0")]
    public decimal? TienCoc { get; set; }

    // Cho phép để trống
    [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
    public string? MoTa { get; set; }

    [Required(ErrorMessage = "Yêu cầu chọn trạng thái")]
    public string? TrangThai { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual ICollection<DanhGia> DanhGia { get; set; }
        = new List<DanhGia>();

    public virtual ICollection<DonDatXe> DonDatXes { get; set; }
        = new List<DonDatXe>();

    public virtual ICollection<HinhAnhXe> HinhAnhXes { get; set; }
        = new List<HinhAnhXe>();

    public virtual HangXe? MaHangNavigation { get; set; }

    public virtual LoaiXe? MaLoaiNavigation { get; set; }
}