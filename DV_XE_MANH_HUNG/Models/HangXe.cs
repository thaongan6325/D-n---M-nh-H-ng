using System;
using System.Collections.Generic;

namespace Vivu_Xe.Models;

public partial class HangXe
{
    public int MaHang { get; set; }

    public string TenHang { get; set; } = null!;

    public string? XuatXu { get; set; }

    public virtual ICollection<Xe> Xes { get; set; } = new List<Xe>();
}
