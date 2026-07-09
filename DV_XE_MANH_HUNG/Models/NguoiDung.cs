using System;
using System.Collections.Generic;

namespace Vivu_Xe.Models;

public partial class NguoiDung
{
    public int MaNguoiDung { get; set; }

    public string HoTen { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public string? DiaChi { get; set; }

    public int? MaVaiTro { get; set; }

    public string? AnhDaiDien { get; set; }

    public DateTime? NgayTao { get; set; }

    public bool? TrangThai { get; set; }

    public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();

    public virtual ICollection<DonDatXe> DonDatXes { get; set; } = new List<DonDatXe>();

    public virtual ICollection<GiayTo> GiayTos { get; set; } = new List<GiayTo>();

    public virtual VaiTro? MaVaiTroNavigation { get; set; }

    public virtual ICollection<XeYeuThich> XeYeuThiches { get; set; } = new List<XeYeuThich>();
}
