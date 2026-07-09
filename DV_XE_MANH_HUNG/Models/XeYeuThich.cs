using System;
using System.Collections.Generic;

namespace Vivu_Xe.Models;

public partial class XeYeuThich
{
    public int MaNguoiDung { get; set; }

    public int MaXe { get; set; }

    public DateTime? NgayLuu { get; set; }

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;

    public virtual Xe MaXeNavigation { get; set; } = null!;
}
