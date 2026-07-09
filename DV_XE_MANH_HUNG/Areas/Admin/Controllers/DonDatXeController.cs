using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vivu_Xe.Data;
using Vivu_Xe.Filters;
using Vivu_Xe.Models;
using VivuXe.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Vivu_Xe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class DonDatXeController : Controller
    {
        private readonly VivuXeContext _context;

        public DonDatXeController(VivuXeContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách đơn hàng
        public async Task<IActionResult> Index(string status)
        {
            var query = _context.DonDatXes
                .Include(d => d.MaXeNavigation)
                .Include(d => d.MaNguoiDungNavigation)
                .OrderByDescending(d => d.NgayDat)
                .AsQueryable();

            // --- XỬ LÝ LỌC THEO STATUS TỪ DASHBOARD ---
            if (!string.IsNullOrEmpty(status))
            {
                switch (status)
                {
                    case "DaDatCoc":
                        // Lọc các đơn có trạng thái là "Đã đặt cọc" (để chuẩn bị giao xe)
                        query = query.Where(d => d.TrangThaiDon == "Đã đặt cọc");
                        ViewBag.CurrentStatus = "Đơn chờ giao xe"; // Để hiển thị tiêu đề nếu cần
                        break;

                    case "HoanThanh":
                        query = query.Where(d => d.TrangThaiDon == "Hoàn thành");
                        break;

                    case "Huy":
                        query = query.Where(d => d.TrangThaiDon == "Đã hủy");
                        break;

                        // Thêm các case khác nếu cần
                }
            }

            return View(await query.ToListAsync());
        }
        // Action trang chi tiết
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var donDatXe = await _context.DonDatXes
                .Include(d => d.MaNguoiDungNavigation)
                .Include(d => d.MaXeNavigation)
                    .ThenInclude(x => x.HinhAnhXes)
                .Include(d => d.SuCoPhatSinhs)//Link thêm sự cố để xuất thông tin ra view
                .FirstOrDefaultAsync(m => m.MaDon == id);

            if (donDatXe == null) return NotFound();

            return View(donDatXe);
        }
        // 3. API Giao xe & Tự động thanh toán
        [HttpPost]
        [Route("Admin/DonDatXe/Handover/{id}")]
        public async Task<IActionResult> Handover(int id)
        {
            var don = await _context.DonDatXes
                .Include(d => d.MaXeNavigation)
                .FirstOrDefaultAsync(d => d.MaDon == id);

            if (don == null) return NotFound(new { success = false, message = "Không tìm thấy đơn" });

            if (don.TrangThaiDon == "Đã đặt cọc")
            {
                // 1. TÍNH TOÁN TIỀN
                decimal tongTienThue = don.TongTien ?? 0;
                decimal tienCocYeuCau = don.MaXeNavigation?.TienCoc ?? 0;
                decimal daCocTruoc = don.TienCocDaDong ?? 0;

                // Thường khách cọc 500k giữ chỗ. Tiền này được trừ vào tiền thuê.
                // Vậy lúc lấy xe, khách phải đóng: (Tiền Thuê - 500k) + Tiền Cọc Thế Chân
                decimal tienThueCanThuThem = tongTienThue - daCocTruoc;

                // 2. TẠO PHIẾU THU TRONG BẢNG THANH TOÁN
                // Phiếu 1: Thu nốt tiền thuê xe (Doanh thu)
                if (tienThueCanThuThem > 0)
                {
                    _context.ThanhToans.Add(new ThanhToan
                    {
                        MaDon = don.MaDon,
                        NgayThanhToan = DateTime.Now,
                        SoTien = tienThueCanThuThem,
                        PhuongThucThanhToan = "Tiền mặt / Chuyển khoản",
                        LoaiThanhToan = "Thanh toán tiền thuê",
                        TrangThai = "Thành công",
                        GhiChu = "Thu nốt tiền thuê lúc bàn giao xe"
                    });
                }

                // Phiếu 2: Thu tiền cọc thế chân (Giữ hộ)
                _context.ThanhToans.Add(new ThanhToan
                {
                    MaDon = don.MaDon,
                    NgayThanhToan = DateTime.Now,
                    SoTien = tienCocYeuCau,
                    PhuongThucThanhToan = "Tiền mặt / Chuyển khoản",
                    LoaiThanhToan = "Cọc thế chân",
                    TrangThai = "Thành công",
                    GhiChu = "Thu tiền cọc đảm bảo tài sản"
                });

                // 3. CẬP NHẬT DATABASE BẢNG ĐƠN ĐẶT XE
                don.TienCocDaDong = tienCocYeuCau;
                don.TrangThaiDon = "Đang đi";
                if (don.MaXeNavigation != null) don.MaXeNavigation.TrangThai = "Đang thuê";
                don.GhiChuNhanVien = (don.GhiChuNhanVien ?? "") +
                                     $" | Giao xe: Thu thuê {tienThueCanThuThem:N0}đ + Thu cọc {tienCocYeuCau:N0}đ.";

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Giao xe thành công! Đã ghi nhận thanh toán." });
            }
            return BadRequest(new { success = false, message = "Trạng thái đơn hàng không hợp lệ." });
        }

        // 4. API Hủy đơn có lý do
        [HttpPost]
        [Route("Admin/DonDatXe/CancelOrder")]
        public async Task<IActionResult> CancelOrder(int id, string lyDo)
        {
            var don = await _context.DonDatXes.FindAsync(id);
            if (don == null) return NotFound(new { success = false, message = "Đơn hàng không tồn tại" });

            if (don.TrangThaiDon == "Hoàn thành" || don.TrangThaiDon == "Đang đi")
            {
                return BadRequest(new { success = false, message = "Xe đang chạy hoặc đã xong, không thể hủy!" });
            }

            // TÍCH HỢP THANH TOÁN: Kiểm tra xem khách đã cọc đồng nào chưa
            decimal tienDaThu = don.TienCocDaDong ?? 0;
            if (tienDaThu > 0)
            {
                // Lập phiếu chi hoàn trả 100% tiền khách đã cọc vì lỗi do cửa hàng hủy
                _context.ThanhToans.Add(new ThanhToan
                {
                    MaDon = don.MaDon,
                    NgayThanhToan = DateTime.Now,
                    SoTien = -tienDaThu, // Số âm để xuất quỹ
                    PhuongThucThanhToan = "Chuyển khoản",
                    LoaiThanhToan = "Hoàn cọc giữ chỗ",
                    TrangThai = "Thành công",
                    GhiChu = $"Hoàn tiền do Admin hủy đơn. Lý do: {lyDo}"
                });

                // Trả số cọc đang giữ về 0
                don.TienCocDaDong = 0;
            }

            don.TrangThaiDon = "Đã hủy";
            don.GhiChuNhanVien = (don.GhiChuNhanVien ?? "") + $" | Hủy bởi Admin: {lyDo}. (Đã hoàn lại {tienDaThu:N0}đ)";

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã hủy đơn hàng và ghi nhận hoàn tiền thành công." });
        }
        //TRẢ XE HOÀN THÀNH
        [HttpPost]
        [Route("Admin/DonDatXe/HoanThanh/{id}")]
        public async Task<IActionResult> HoanThanh(int id)
        {
            var don = await _context.DonDatXes
                .Include(d => d.MaXeNavigation)
                .FirstOrDefaultAsync(d => d.MaDon == id);

            if (don != null && don.TrangThaiDon == "Đang đi")
            {
                decimal tienCocDangGiu = don.TienCocDaDong ?? 0;

                // TÍCH HỢP THANH TOÁN: Lập phiếu chi hoàn cọc (Lưu số tiền ÂM)
                _context.ThanhToans.Add(new ThanhToan
                {
                    MaDon = don.MaDon,
                    NgayThanhToan = DateTime.Now,
                    SoTien = -tienCocDangGiu,
                    PhuongThucThanhToan = "Tiền mặt / Chuyển khoản",
                    LoaiThanhToan = "Hoàn cọc thế chân",
                    TrangThai = "Thành công",
                    GhiChu = "Khách trả xe nguyên vẹn, hoàn cọc 100%"
                });

                // Cập nhật trạng thái
                don.TrangThaiDon = "Hoàn thành";
                don.NgayTraThucTe = DateTime.Now;
                don.TienCocDaDong = 0; // Trả cọc về 0

                if (don.MaXeNavigation != null) don.MaXeNavigation.TrangThai = "Sẵn sàng";

                don.GhiChuNhanVien = (don.GhiChuNhanVien ?? "") + $" | Hoàn tất: Đã trả cọc {tienCocDangGiu:N0}đ.";

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = $"Trả xe thành công!\nHãy hoàn lại {tienCocDangGiu:N0}đ tiền cọc cho khách." });
            }
            return BadRequest(new { success = false, message = "Lỗi trạng thái đơn hàng." });
        }

        // TRẢ XE CÓ SỰ CỐ
        [HttpPost]
        [Route("Admin/DonDatXe/ReportIncident")]
        public async Task<IActionResult> ReportIncident(int maDon, string[] loaiSuCo, string[] moTa, decimal[] phiPhat)
        {
            var don = await _context.DonDatXes
                .Include(d => d.MaXeNavigation)
                .FirstOrDefaultAsync(d => d.MaDon == maDon);

            if (don == null) return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });

            decimal tongTienPhat = 0;

            // 1. LẶP QUA DANH SÁCH SỰ CỐ ĐỂ LƯU
            if (loaiSuCo != null && phiPhat != null)
            {
                for (int i = 0; i < loaiSuCo.Length; i++)
                {
                    // Chỉ lưu những dòng có chọn loại sự cố
                    if (!string.IsNullOrEmpty(loaiSuCo[i]))
                    {
                        var suCo = new SuCoPhatSinh
                        {
                            MaDon = maDon,
                            LoaiSuCo = loaiSuCo[i],
                            MoTaChiTiet = (moTa != null && moTa.Length > i) ? moTa[i] : "",
                            PhiPhat = (phiPhat != null && phiPhat.Length > i) ? phiPhat[i] : 0,
                            NgayGhiNhan = DateTime.Now
                        };
                        _context.SuCoPhatSinhs.Add(suCo);

                        // Cộng dồn tổng tiền phạt
                        tongTienPhat += suCo.PhiPhat ?? 0;
                    }
                }
            }

            // TÍCH HỢP THANH TOÁN
            decimal tienCocDangGiu = don.TienCocDaDong ?? 0; // VD: Đang giữ 5.000.000đ

            // Phiếu 1: Ghi nhận thu nhập từ tiền phạt
            if (tongTienPhat > 0)
            {
                _context.ThanhToans.Add(new ThanhToan
                {
                    MaDon = don.MaDon,
                    NgayThanhToan = DateTime.Now,
                    SoTien = tongTienPhat,
                    PhuongThucThanhToan = "Cấn trừ cọc",
                    LoaiThanhToan = "Phụ phí phát sinh",
                    TrangThai = "Thành công",
                    GhiChu = "Thu phí phạt sự cố cấn trừ vào cọc"
                });
            }

            // Phiếu 2: Lập phiếu chi hoàn trả tiền cọc CÒN DƯ (Ví dụ: 5.000.000 - 300.000 = 4.700.000)
            decimal tienCocHoanLai = tienCocDangGiu - tongTienPhat;

            // Nếu tiền phạt lớn hơn tiền cọc, khách nợ tiền, nhưng để đơn giản, ta cứ lưu khoản cần trả (có thể là âm nếu khách nợ)
            _context.ThanhToans.Add(new ThanhToan
            {
                MaDon = don.MaDon,
                NgayThanhToan = DateTime.Now,
                SoTien = -tienCocHoanLai, // Ghi số âm để xuất quỹ
                PhuongThucThanhToan = "Tiền mặt / Chuyển khoản",
                LoaiThanhToan = "Hoàn cọc thế chân",
                TrangThai = "Thành công",
                GhiChu = "Hoàn cọc sau khi đã trừ phí phạt"
            });

            don.TienCocDaDong = 0;
            don.TrangThaiDon = "Hoàn thành";
            don.NgayTraThucTe = DateTime.Now;

            if (don.MaXeNavigation != null) don.MaXeNavigation.TrangThai = "Bảo dưỡng";

            don.GhiChuNhanVien = (don.GhiChuNhanVien ?? "") +
                                 $" | Có sự cố. Phạt: {tongTienPhat:N0}đ. Hoàn cọc còn lại: {tienCocHoanLai:N0}đ.";

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = $"Đã ghi nhận sự cố! Hoàn lại {tienCocHoanLai:N0}đ cho khách." });
        }
    }
}
