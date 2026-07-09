using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vivu_Xe.Models;
[Table("DanhGia")]
public partial class DanhGia
{
    public int MaDanhGia { get; set; }

    public int? MaDon { get; set; }

    public int? MaXe { get; set; }

    public int? MaNguoiDung { get; set; }

    public int? SoSao { get; set; }

    public string? NhanXet { get; set; }

    public DateTime? NgayDanhGia { get; set; }

    public virtual DonDatXe? MaDonNavigation { get; set; }

    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }

    public virtual Xe? MaXeNavigation { get; set; }
}
