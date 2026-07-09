using System;
using System.Collections.Generic;

namespace Vivu_Xe.Models;

public partial class HinhAnhXe
{
    public int MaHinh { get; set; }

    public int? MaXe { get; set; }

    public string DuongDan { get; set; } = null!;

    public virtual Xe? MaXeNavigation { get; set; }
}
