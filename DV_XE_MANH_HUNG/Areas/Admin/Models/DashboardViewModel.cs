using Vivu_Xe.Models;

namespace Vivu_Xe.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        public int TongSoXe { get; set; }
        public int XeDangSanSang { get; set; }
        public int TongDonHang { get; set; }
        public decimal TongDoanhThu { get; set; }
        public List<DonDatXe> DonHangMoiNhat { get; set; }
        public int SoUserChoDuyet { get; set; } // Số tài khoản up giấy tờ chờ duyệt
        public int SoDonDaCoc { get; set; }     // Số đơn chờ giao xe
        public int TongCanXuLy => SoUserChoDuyet + SoDonDaCoc; // Tổng cộng
    }
}