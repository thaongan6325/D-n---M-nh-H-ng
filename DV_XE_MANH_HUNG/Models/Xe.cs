using System;
using System.Collections.Generic;

namespace Vivu_Xe.Models;

public partial class Xe
{
    public int MaXe { get; set; }

    public string TenXe { get; set; } = null!;

    public string BienSo { get; set; } = null!;

    public int? MaHang { get; set; }

    public int? MaLoai { get; set; }

    public string? MauSac { get; set; }

    public int? NamSanXuat { get; set; }

    public string? HopSo { get; set; }

    public string? NhienLieu { get; set; }

    public decimal GiaThueNgay { get; set; }

    public decimal? TienCoc { get; set; }

    public string? MoTa { get; set; }

    public string? TrangThai { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual ICollection<DanhGia> DanhGia { get; set; } = new List<DanhGia>();

    public virtual ICollection<DonDatXe> DonDatXes { get; set; } = new List<DonDatXe>();

    public virtual ICollection<HinhAnhXe> HinhAnhXes { get; set; } = new List<HinhAnhXe>();

    public virtual HangXe? MaHangNavigation { get; set; }

    public virtual LoaiXe? MaLoaiNavigation { get; set; }

    public virtual ICollection<XeYeuThich> XeYeuThiches { get; set; } = new List<XeYeuThich>();
}
