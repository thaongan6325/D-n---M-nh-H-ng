using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vivu_Xe.Areas.Admin.Models;
using Vivu_Xe.Data;
using Vivu_Xe.Filters;

namespace Vivu_Xe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class DashboardController : Controller
    {
        private readonly VivuXeContext _context;

        public DashboardController(VivuXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Khởi tạo ViewModel
            var model = new DashboardViewModel();

            // 1. Thống kê XE
            model.TongSoXe = await _context.Xes.CountAsync();
            model.XeDangSanSang = await _context.Xes.CountAsync(x => x.TrangThai == "Sẵn sàng");

            // 2. Thống kê ĐƠN HÀNG
            model.TongDonHang = await _context.DonDatXes.CountAsync();
            model.SoDonDaCoc = await _context.DonDatXes.CountAsync(d => d.TrangThaiDon == "Đã đặt cọc");

            // 3. TÍNH DOANH THU (Cập nhật dùng bảng ThanhToan)
            // Kế toán: Doanh thu = Tiền thuê xe + Phụ phí + Cọc giữ chỗ (Của mọi đơn) 
            // TRỪ ĐI các khoản Hoàn cọc (Số tiền hoàn đang lưu là số ÂM nên cứ dùng hàm SUM là tự động cấn trừ)
            // BỎ QUA các khoản "Cọc thế chân" và "Phí phạt hủy đơn" (vì tiền phạt đã nằm sẵn trong cục Cọc giữ chỗ rồi, cộng thêm sẽ bị x2).
            model.TongDoanhThu = await _context.ThanhToans
                .Where(t => t.TrangThai == "Thành công") // Chỉ tính giao dịch đã tiền trao cháo múc
                .Where(t => t.LoaiThanhToan == "Thanh toán tiền thuê" ||
                            t.LoaiThanhToan == "Cọc giữ chỗ" ||
                            t.LoaiThanhToan == "Phụ phí phát sinh" ||
                            t.LoaiThanhToan == "Hoàn cọc giữ chỗ" ||
                            t.LoaiThanhToan == "Hoàn cọc hủy đơn")
                .SumAsync(t => t.SoTien);

            // 4. Đếm số người đang chờ duyệt giấy tờ
            model.SoUserChoDuyet = await _context.GiayTos
                .Where(g => g.DaXacThuc == false) // BIT 0
                .Select(g => g.MaNguoiDung)
                .Distinct()
                .CountAsync();

            // 5. Lấy 5 đơn hàng MỚI NHẤT để hiển thị ra bảng
            model.DonHangMoiNhat = await _context.DonDatXes
                .Include(d => d.MaNguoiDungNavigation) // Lấy tên khách
                .Include(d => d.MaXeNavigation)        // Lấy tên xe
                .OrderByDescending(d => d.NgayDat)     // Sắp xếp mới nhất
                .Take(5)                               // Chỉ lấy 5 đơn
                .ToListAsync();

            // 6. TÍNH DOANH THU 12 THÁNG CHO BIỂU ĐỒ (Của năm hiện tại)
            int namHienTai = DateTime.Now.Year;

            var doanhThuTheoThang = await _context.ThanhToans
                .Where(t => t.TrangThai == "Thành công" && t.NgayThanhToan.HasValue && t.NgayThanhToan.Value.Year == namHienTai)
                .Where(t => t.LoaiThanhToan == "Thanh toán tiền thuê" ||
                            t.LoaiThanhToan == "Cọc giữ chỗ" ||
                            t.LoaiThanhToan == "Phụ phí phát sinh" ||
                            t.LoaiThanhToan == "Hoàn cọc giữ chỗ" ||
                            t.LoaiThanhToan == "Hoàn cọc hủy đơn")
                // Nhóm dữ liệu theo tháng
                .GroupBy(t => t.NgayThanhToan.Value.Month)
                .Select(g => new { Thang = g.Key, TongTien = g.Sum(t => t.SoTien) })
                .ToListAsync();

            // Khởi tạo mảng 12 tháng (giá trị mặc định là 0)
            decimal[] mangDoanhThu = new decimal[12];

            // Đổ dữ liệu thật vào mảng
            foreach (var item in doanhThuTheoThang)
            {
                // Mảng bắt đầu từ index 0, nên Tháng 1 sẽ ở vị trí 0
                mangDoanhThu[item.Thang - 1] = item.TongTien;
            }

            // Chuyển mảng thành chuỗi JSON để ném sang Javascript
            ViewBag.ChartData = System.Text.Json.JsonSerializer.Serialize(mangDoanhThu);
            return View(model);
        }
    }
}