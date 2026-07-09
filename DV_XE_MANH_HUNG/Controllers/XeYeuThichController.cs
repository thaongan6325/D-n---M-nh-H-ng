using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vivu_Xe.Data;
using Vivu_Xe.Models;

namespace Vivu_Xe.Controllers
{
    public class XeYeuThichController : Controller
    {
        private readonly VivuXeContext _context;

        public XeYeuThichController(VivuXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Account");

            // LOGIC: Lấy danh sách Xe từ bảng trung gian XeYeuThich
            var danhSachYeuThich = await _context.XeYeuThiches
                .Include(y => y.MaXeNavigation) // Join sang bảng Xe (lưu ý tên nav có thể là Xe hoặc MaXeNavigation tùy lúc scaffold)
                    .ThenInclude(x => x.HinhAnhXes) // Lấy ảnh
                .Include(y => y.MaXeNavigation.MaLoaiNavigation) // Lấy loại xe (4 chỗ,...)
                .Where(y => y.MaNguoiDung == userId)
                .OrderByDescending(y => y.NgayLuu) // Xe mới thích hiện lên đầu
                .ToListAsync();

            return View(danhSachYeuThich);
        }

        // API Toggle
        [HttpPost]
        [Route("XeYeuThich/Toggle/{maXe}")]
        public async Task<IActionResult> Toggle(int maXe)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return Unauthorized(new { success = false });

            var existing = await _context.XeYeuThiches.FirstOrDefaultAsync(x => x.MaNguoiDung == userId && x.MaXe == maXe);
            bool isLiked = false;

            if (existing != null)
            {
                _context.XeYeuThiches.Remove(existing);
                isLiked = false; // Đã bỏ thích
            }
            else
            {
                var newItem = new XeYeuThich { MaNguoiDung = userId.Value, MaXe = maXe };
                _context.XeYeuThiches.Add(newItem);
                isLiked = true; // Đã thích
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, isLiked = isLiked });
        }
    }
}