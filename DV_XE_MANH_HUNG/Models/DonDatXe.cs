using System;
using System.Collections.Generic;
using VivuXe.Models;

namespace Vivu_Xe.Models;

public partial class DonDatXe
{
    public int MaDon { get; set; }

    public int? MaNguoiDung { get; set; }

    public int? MaXe { get; set; }

    public DateTime? NgayDat { get; set; }

    public DateTime NgayNhanDuKien { get; set; }

    public DateTime NgayTraDuKien { get; set; }

    public DateTime? NgayTraThucTe { get; set; }

    public decimal? TongTien { get; set; }

    public decimal? TienCocDaDong { get; set; }

    public string? TrangThaiDon { get; set; }

    public string? GhiChuNhanVien { get; set; }

    public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();

    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }

    public virtual Xe? MaXeNavigation { get; set; }

    public virtual ICollection<SuCoPhatSinh> SuCoPhatSinhs { get; set; } = new List<SuCoPhatSinh>();
    public virtual ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();
}
