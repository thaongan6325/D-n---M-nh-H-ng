using System;
using System.Collections.Generic;

namespace Vivu_Xe.Models;

public partial class GiayTo
{
    public int MaGiayTo { get; set; }

    public int? MaNguoiDung { get; set; }

    public string? LoaiGiayTo { get; set; }

    public string? SoGiayTo { get; set; }

    public string? AnhMatTruoc { get; set; }

    public string? AnhMatSau { get; set; }

    public bool? DaXacThuc { get; set; }

    public DateTime? NgayTaiLen { get; set; }

    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }
}
