using HashidsNet; //Thư viện để mã hóa url
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vivu_Xe.Data;
using Vivu_Xe.Models;
using VivuXe.Models;

namespace Vivu_Xe.Controllers
{
    public class BookingController : Controller
    {
        private readonly VivuXeContext _context;
        private readonly Hashids _hashids = new Hashids("ChuoiMaHoaVivuXe", 8);
        public BookingController(VivuXeContext context)
        {
            _context = context;
        }


        // 1. Action Tìm kiếm
        [HttpGet]
        public async Task<IActionResult> Search(string location, DateTime? startDate, DateTime? endDate, int? maLoai)
        {
            if (startDate == null || endDate == null)
            {
                startDate = DateTime.Now;
                endDate = DateTime.Now.AddDays(1);
            }

            if (startDate >= endDate)
            {
                TempData["Error"] = "Ngày trả xe phải sau ngày nhận xe!";
                return RedirectToAction("Index", "Home");
            }

            var xeBanIds = await _context.DonDatXes
                .Where(d => d.TrangThaiDon != "Đã hủy" && d.TrangThaiDon != "Hoàn thành")
                .Where(d => d.NgayNhanDuKien < endDate && d.NgayTraDuKien > startDate)
                .Select(d => d.MaXe)
                .ToListAsync();

            var query = _context.Xes
                .Include(x => x.HinhAnhXes)
                .Include(x => x.MaLoaiNavigation)
                .Where(x => !xeBanIds.Contains(x.MaXe))
                .Where(x => x.TrangThai == "Sẵn sàng")
                .AsQueryable();

            if (maLoai.HasValue) query = query.Where(x => x.MaLoai == maLoai);

            var ketQuaTimKiem = await query.ToListAsync();

            // Xử lý viewbag...
            ViewBag.Location = location;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.MaLoai = maLoai;

            return View(ketQuaTimKiem);
        }

        // 2. Action Hiển thị form Tạo đơn
        [HttpGet]
        public async Task<IActionResult> Create(int id, string start, string end)
        {
            var xe = await _context.Xes.Include(x => x.HinhAnhXes).FirstOrDefaultAsync(x => x.MaXe == id);
            if (xe == null) return NotFound();

            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Account", new { returnUrl = $"/Booking/Create/{id}?start={start}&end={end}" });

            var user = await _context.NguoiDungs.FindAsync(userId);
            ViewBag.User = user;

            if (!DateTime.TryParse(start, out DateTime startDate) || !DateTime.TryParse(end, out DateTime endDate))
                return RedirectToAction("Details", "Xe", new { id = id });

            // Tính toán view hiển thị
            double hours = Math.Ceiling((endDate - startDate).TotalHours);
            if (hours < 1) hours = 24;
            decimal giaThueMoiGio = (xe.GiaThueNgay) / 24m;
            decimal phiThueXe = giaThueMoiGio * (decimal)hours;
            decimal phiBaoHiem = phiThueXe * 0.1m;
            decimal tongTienThue = phiThueXe + phiBaoHiem;

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.PhiThueXe = phiThueXe;
            ViewBag.PhiBaoHiem = phiBaoHiem;
            ViewBag.TongTienThue = tongTienThue;

            // Các khoản tiền
            ViewBag.TienCocXe = xe.TienCoc ?? 5000000m;
            ViewBag.TienGiuCho = 500000m;
            ViewBag.TienThanhToanSau = tongTienThue - 500000m;

            return View(xe);
        }

        // 3. Action Xử lý đặt xe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBooking(int MaXe, DateTime NgayNhan, DateTime NgayTra, string GhiChu, string DiaChiGiaoXe, string PaymentMethod)
        {
            // A. Bảo mật & Lấy thông tin
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _context.NguoiDungs.FindAsync(userId);
            var xe = await _context.Xes.FindAsync(MaXe);
            if (xe == null) return NotFound();

            // B. Tính lại tiền (Backend Calculation - Bảo mật)
            double hours = Math.Ceiling((NgayTra - NgayNhan).TotalHours);
            if (hours < 1) hours = 24;
            decimal giaThueMoiGio = (xe.GiaThueNgay) / 24m;
            decimal phiThueXe = giaThueMoiGio * (decimal)hours;
            decimal phiBaoHiem = phiThueXe * 0.1m;
            decimal tongTienThue = phiThueXe + phiBaoHiem;
            // Làm tròn tiền
            tongTienThue = Math.Round(tongTienThue, 0);

            // C. Tạo nội dung ghi chú tổng hợp
            string diaChiText = string.IsNullOrEmpty(DiaChiGiaoXe) ? "Tại cửa hàng" : DiaChiGiaoXe;
            string noiDungGhiChu = $"KH: {user.HoTen} - SĐT: {user.SoDienThoai}. " +
                                   $"Giao: {diaChiText}. " +
                                   $"Pay: {PaymentMethod ?? "Banking"}";

            // D. Lưu vào Database
            var donDatXe = new DonDatXe
            {
                MaNguoiDung = userId.Value,
                MaXe = MaXe,
                NgayNhanDuKien = NgayNhan,
                NgayTraDuKien = NgayTra,
                NgayDat = DateTime.Now,

                TongTien = tongTienThue,
                TienCocDaDong = 0,               // Mới tạo chưa đóng tiền
                TrangThaiDon = "Chờ thanh toán", // Status khởi tạo

                GhiChuNhanVien = noiDungGhiChu
            };

            _context.Add(donDatXe);
            await _context.SaveChangesAsync();

            // E. Chuyển hướng sang Thanh toán
            return RedirectToAction("Payment", new { id = donDatXe.MaDon });
        }

        // 4. Action Trang Thanh Toán (Hiển thị QR)
        public async Task<IActionResult> Payment(int id)
        {
            var donHang = await _context.DonDatXes
                .Include(d => d.MaXeNavigation)
                .FirstOrDefaultAsync(d => d.MaDon == id);

            if (donHang == null) return NotFound();

            ViewBag.SoTienCanDong = 500000m;
            ViewBag.NoiDungCK = $"VIVUXE ORD#{donHang.MaDon}";

            return View(donHang);
        }

        // 5. Action Trang Hoàn tất (Success Page)
        [HttpGet]
        public async Task<IActionResult> Success(string id)
        {
            // Lấy Session User để đảm bảo đúng người
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Account");
            //Dịch ngược lại code
            var decodedIds = _hashids.Decode(id);
            if (decodedIds.Length == 0)
            {
                return NotFound("Mã đơn hàng không hợp lệ.");
            }
            int originalId = decodedIds[0];
            var donHang = await _context.DonDatXes
                .Include(d => d.MaXeNavigation) // Include để lấy tên xe, hình ảnh
                    .ThenInclude(x => x.HinhAnhXes)
                .Include(d => d.MaNguoiDungNavigation) // Include lấy thông tin người dùng nếu cần
                .FirstOrDefaultAsync(d => d.MaDon == originalId);

            // Kiểm tra đơn hàng tồn tại và phải đúng của user đó
            if (donHang == null || donHang.MaNguoiDung != userId)
            {
                return NotFound();
            }

            return View(donHang);
        }

        // 6. API Update trạng thái
        [HttpPost]
        public async Task<IActionResult> FinishPayment([FromBody] PaymentRequest req)
        {
            // 1. Kiểm tra đăng nhập
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return Unauthorized(new { success = false, message = "Vui lòng đăng nhập lại." });

            // 2. Tìm đơn hàng
            var donHang = await _context.DonDatXes.FindAsync(req.Id);

            // 3. Kiểm tra logic
            if (donHang != null && donHang.MaNguoiDung == userId && donHang.TrangThaiDon == "Chờ thanh toán")
            {
                // Cập nhật trạng thái đơn hàng (Có thể giữ cột TienCocDaDong làm snapshot cho View load nhanh)
                donHang.TrangThaiDon = "Đã đặt cọc";
                donHang.TienCocDaDong = 500000m;

                // TÍCH HỢP BẢNG THANH TOÁN: Ghi nhận khoản thu "Cọc giữ chỗ"
                var phieuThuCoc = new ThanhToan
                {
                    MaDon = donHang.MaDon,
                    NgayThanhToan = DateTime.Now,
                    SoTien = 500000m,
                    PhuongThucThanhToan = "Chuyển khoản (QR)",
                    LoaiThanhToan = "Cọc giữ chỗ",
                    TrangThai = "Thành công",
                    GhiChu = "Khách hàng thanh toán tiền cọc giữ chỗ qua mã QR."
                };

                _context.ThanhToans.Add(phieuThuCoc);

                // Lưu tất cả vào DB trong 1 transaction
                await _context.SaveChangesAsync();

                // MÃ HÓA ID
                string hashedId = _hashids.Encode(donHang.MaDon);

                // Trả về URL chuyển hướng cho Client
                return Ok(new
                {
                    success = true,
                    message = "Thanh toán thành công!",
                    redirectUrl = $"/Booking/Success/{hashedId}" // Server chỉ định nơi đến
                });
            }

            return BadRequest(new { success = false, message = "Đơn hàng lỗi hoặc đã được xử lý trước đó." });
        }

        // Class nhận dữ liệu JSON từ JS
        public class PaymentRequest
        {
            public int Id { get; set; }
        }
        // API Hủy đơn dành cho Khách hàng (Client)
        [HttpPost]
        [Route("Booking/CancelBooking")]
        public async Task<IActionResult> CancelBooking([FromBody] PaymentRequest req)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return Unauthorized(new { success = false, message = "Vui lòng đăng nhập lại." });

            var donHang = await _context.DonDatXes.FindAsync(req.Id);

            // Cho phép khách hủy đơn ở 2 trạng thái: "Chờ thanh toán" (chưa cọc) và "Đã đặt cọc"
            if (donHang != null && donHang.MaNguoiDung == userId &&
               (donHang.TrangThaiDon == "Chờ thanh toán" || donHang.TrangThaiDon == "Đã đặt cọc"))
            {
                string ghiChuHuy = "Khách tự hủy đơn.";
                decimal tienDaThu = donHang.TienCocDaDong ?? 0;

                // NẾU KHÁCH ĐÃ CỌC -> TÍNH TOÁN HOÀN TIỀN THEO QUY ĐỊNH
                if (tienDaThu > 0)
                {
                    // Tính số ngày chênh lệch từ hôm nay đến ngày nhận xe
                    int soNgayLech = (donHang.NgayNhanDuKien.Date - DateTime.Now.Date).Days;

                    // Hàm kiểm tra ngày nhận xe có rơi vào ngày lễ không
                    bool isNgayLe = KiemTraNgayLe(donHang.NgayNhanDuKien);

                    decimal tyLeHoan = 0m;

                    if (isNgayLe)
                    {
                        // QUY ĐỊNH NGÀY LỄ
                        if (soNgayLech > 30) tyLeHoan = 0.30m; // Hoàn 30%
                        else tyLeHoan = 0m; // Dưới 30 ngày -> Không hoàn
                    }
                    else
                    {
                        // QUY ĐỊNH NGÀY THƯỜNG
                        if (soNgayLech > 10) tyLeHoan = 1.0m; // Hoàn 100%
                        else if (soNgayLech > 5) tyLeHoan = 0.30m; // Hoàn 30%
                        else tyLeHoan = 0m; // Trong vòng 5 ngày -> Không hoàn
                    }

                    decimal tienHoanLai = tienDaThu * tyLeHoan;
                    decimal tienPhatMatCoc = tienDaThu - tienHoanLai;

                    // Ghi chú chi tiết để Admin nắm rõ
                    ghiChuHuy += $" Báo trước {soNgayLech} ngày ({(isNgayLe ? "Ngày lễ" : "Ngày thường")})." +
                                 $" Tỷ lệ hoàn: {tyLeHoan * 100}%. Phạt: {tienPhatMatCoc:N0}đ.";

                    // TÍCH HỢP THANH TOÁN: Lập phiếu chi trả lại tiền cho khách (Nếu có)
                    if (tienHoanLai > 0)
                    {
                        _context.ThanhToans.Add(new ThanhToan
                        {
                            MaDon = donHang.MaDon,
                            NgayThanhToan = DateTime.Now,
                            SoTien = -tienHoanLai, // Ghi số âm để xuất quỹ hoàn tiền
                            PhuongThucThanhToan = "Chuyển khoản",
                            LoaiThanhToan = "Hoàn cọc hủy đơn",
                            TrangThai = "Chờ xử lý", // Đặt là Chờ xử lý để Kế toán biết đường CK lại cho khách
                            GhiChu = $"Hoàn {tyLeHoan * 100}% cọc do khách hủy sớm."
                        });
                    }

                    // Ghi nhận số cọc còn lại (nếu có) thành doanh thu phạt
                    if (tienPhatMatCoc > 0)
                    {
                        _context.ThanhToans.Add(new ThanhToan
                        {
                            MaDon = donHang.MaDon,
                            NgayThanhToan = DateTime.Now,
                            SoTien = tienPhatMatCoc,
                            PhuongThucThanhToan = "Cấn trừ cọc",
                            LoaiThanhToan = "Phí phạt hủy đơn",
                            TrangThai = "Thành công",
                            GhiChu = "Tịch thu tiền cọc do hủy sát ngày."
                        });
                    }

                    donHang.TienCocDaDong = 0; // Đã cấn trừ và hoàn trả xong
                }

                // Cập nhật trạng thái
                donHang.TrangThaiDon = "Đã hủy";
                donHang.GhiChuNhanVien = (donHang.GhiChuNhanVien ?? "") + $" | {ghiChuHuy}";

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Đã hủy đơn hàng thành công." });
            }

            return BadRequest(new { success = false, message = "Không thể hủy đơn hàng này." });
        }
        private bool KiemTraNgayLe(DateTime ngayKiemTra)
        {
            // Lấy ngày và tháng
            int day = ngayKiemTra.Day;
            int month = ngayKiemTra.Month;

            // Danh sách các ngày lễ cố định (Dương lịch)
            if (day == 1 && month == 1) return true;   // Tết Dương lịch
            if (day == 30 && month == 4) return true;  // Giải phóng miền Nam
            if (day == 1 && month == 5) return true;   // Quốc tế Lao động
            if (day == 2 && month == 9) return true;   // Quốc khánh
            if (ngayKiemTra.Year == 2026 && month == 2 && day >= 16 && day <= 22)
            {
                return true;
            }
            return false;
        }
    }
}