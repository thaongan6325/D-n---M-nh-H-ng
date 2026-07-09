using System;
using System.Collections.Generic;

namespace Vivu_Xe.Models;

public partial class LoaiXe
{
    public int MaLoai { get; set; }

    public string TenLoai { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<Xe> Xes { get; set; } = new List<Xe>();
}
