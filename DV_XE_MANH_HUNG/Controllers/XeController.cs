using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vivu_Xe.Data;
using Vivu_Xe.Models;

namespace Vivu_Xe.Controllers
{
    public class XeController : Controller
    {
        private readonly VivuXeContext _context;

        public XeController(VivuXeContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> Index(string loai, string nhienlieu)
        {
            // 1. Khởi tạo query cơ bản (Lấy xe sẵn sàng + kèm ảnh + loại)
            var query = _context.Xes
                .Include(x => x.HinhAnhXes)
                .Include(x => x.MaLoaiNavigation)
                .Where(x => x.TrangThai == "Sẵn sàng")
                .AsQueryable();

            // 2. Tiêu đề mặc định
            string pageTitle = "TẤT CẢ XE";

            // 3. Xử lý Lọc theo LOẠI
            if (!string.IsNullOrEmpty(loai))
            {
                switch (loai)
                {
                    case "4cho":
                        //MaLoai=1 là 4 chỗ 
                        query = query.Where(x => x.MaLoai == 1);
                        pageTitle = "XE SEDAN 4 CHỖ";
                        break;
                    case "7cho":
                        //MaLoai=2 là 7 chỗ
                        query = query.Where(x => x.MaLoai == 2);
                        pageTitle = "XE SUV 7 CHỖ";
                        break;
               
                    case "oto":
                        // Lọc các loại có chữ "chỗ" trong tên (VD: 4 chỗ, 7 chỗ...)
                        query = query.Where(x => x.MaLoaiNavigation.TenLoai.Contains("chỗ"));
                        pageTitle = "XE Ô TÔ TỰ LÁI";
                        break;
                }
            }

            // 4. Xử lý Lọc theo NHIÊN LIỆU (Xe điện)
            if (!string.IsNullOrEmpty(nhienlieu))
            {
                // Chuyển tham số về chữ thường để so sánh (đề phòng link là "Dien" hoặc "dien")
                if (nhienlieu.ToLower() == "dien")
                {
                    // Lọc trong Database: Tìm xe có chữ "Điện" (Có dấu)
                    query = query.Where(x => x.NhienLieu.Contains("Điện"));
                    pageTitle = "XE ĐIỆN VINFAST";
                }
            }

            // 5. Lấy danh sách Yêu thích (Để hiển thị tim đỏ)
            var userId = HttpContext.Session.GetInt32("UserID");
            var likedCarIds = new List<int>();
            if (userId != null)
            {
                likedCarIds = await _context.XeYeuThiches
                                    .Where(y => y.MaNguoiDung == userId)
                                    .Select(y => y.MaXe)
                                    .ToListAsync();
            }
            ViewBag.LikedCarIds = likedCarIds;
            ViewBag.PageTitle = pageTitle; // Truyền tiêu đề ra View

            // 6. Thực thi truy vấn
            var result = await query.OrderByDescending(x => x.MaXe).ToListAsync();
            return View(result);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var xe = await _context.Xes
                .Include(x => x.MaHangNavigation)
                .Include(x => x.MaLoaiNavigation)
                .Include(x => x.HinhAnhXes)
                .FirstOrDefaultAsync(m => m.MaXe == id);

            if (xe == null) return NotFound();

            // --- 1. LẤY USER ID TỪ SESSION ---
            var userId = HttpContext.Session.GetInt32("UserID");

            // --- 2. CHECK YÊU THÍCH ---
            ViewBag.IsLiked = false;
            if (userId != null)
            {
                var like = await _context.XeYeuThiches.FindAsync(userId, id);
                if (like != null) ViewBag.IsLiked = true;
            }

            // --- 3. CHECK XÁC THỰC GIẤY TỜ ---
            bool isVerified = false; // Mặc định là chưa xác thực

            if (userId != null)
            {
                // Lấy toàn bộ giấy tờ
                var listGiayTo = await _context.GiayTos
                                    .Where(g => g.MaNguoiDung == userId)
                                    .ToListAsync();

                // Kiểm tra điều kiện:
                // 1. Phải có GPLX và Đã được duyệt (DaXacThuc == true)
                bool coGPLX = listGiayTo.Any(g => g.LoaiGiayTo == "GPLX" && g.DaXacThuc == true);

                // 2. Phải có CCCD và Đã được duyệt (DaXacThuc == true)
                bool coCCCD = listGiayTo.Any(g => g.LoaiGiayTo == "CCCD" && g.DaXacThuc == true);

                // Nếu có đủ cả 2 món thì OK
                if (coGPLX && coCCCD)
                {
                    isVerified = true;
                }
            }
            var listDanhGia = await _context.DanhGias
            .Include(d => d.MaNguoiDungNavigation) // JOIN để lấy tên người dùng
            .Where(d => d.MaXe == id)
            .OrderByDescending(d => d.NgayDanhGia) // Mới nhất lên đầu
            .ToListAsync();

            // Tính điểm trung bình
            double diemTB = 0;
            if (listDanhGia.Count > 0)
            {
                diemTB = listDanhGia.Average(x => x.SoSao ?? 0);
            }

            // Truyền dữ liệu sang View bằng ViewBag
            ViewBag.ListDanhGia = listDanhGia;
            ViewBag.DiemTrungBinh = diemTB;
            ViewBag.TongDanhGia = listDanhGia.Count;
            ViewBag.IsVerified = isVerified;
            return View(xe); // Model chính vẫn là Xe
        }
        //Action gửi đánh giá
        [HttpPost]
        public async Task<IActionResult> GuiDanhGia(int MaDon, int MaXe, int SoSao, string NhanXet)
        {
            // 1. Lấy UserID từ Session để đảm bảo bảo mật
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // 2. Kiểm tra xem đơn hàng này đã được đánh giá chưa (tránh spam)
            var daDanhGia = await _context.DanhGias
                .AnyAsync(d => d.MaDon == MaDon && d.MaNguoiDung == userId);

            if (daDanhGia)
            {
                // Có thể thông báo lỗi hoặc bỏ qua
                return RedirectToAction("History", "Account");
            }

            // 3. Tạo đối tượng đánh giá mới
            var danhGia = new DanhGia
            {
                MaDon = MaDon,
                MaXe = MaXe,
                MaNguoiDung = userId,
                SoSao = SoSao,
                NhanXet = NhanXet,
                NgayDanhGia = DateTime.Now
            };

            _context.DanhGias.Add(danhGia);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cảm ơn bạn đã đánh giá!";

            return RedirectToAction("History", "Account");
        }
    }
}