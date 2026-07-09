using System;
using System.Collections.Generic;

namespace Vivu_Xe.Models;

public partial class SuCoPhatSinh
{
    public int MaSuCo { get; set; }

    public int? MaDon { get; set; }

    public string? LoaiSuCo { get; set; }

    public string? MoTaChiTiet { get; set; }

    public decimal? PhiPhat { get; set; }

    public DateTime? NgayGhiNhan { get; set; }

    public virtual DonDatXe? MaDonNavigation { get; set; }
}
