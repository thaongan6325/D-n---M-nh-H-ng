using Microsoft.AspNetCore.Mvc;
using Vivu_Xe.Data;
using Vivu_Xe.Models;
using Microsoft.EntityFrameworkCore;

namespace Vivu_Xe.Controllers
{
    public class AjaxController : Controller
    {
        private readonly VivuXeContext _context;

        public AjaxController(VivuXeContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int maXe)
        {
            // 1. Kiểm tra Session đăng nhập
            var userId = HttpContext.Session.GetInt32("UserID"); // Lấy ID từ Session lúc Login
            if (userId == null)
            {
                // Trả về báo lỗi chưa đăng nhập để JS hiện popup
                return Json(new { success = false, message = "LoginRequired" });
            }

            // 2. Tìm trong DB xem đã thích chưa (Dùng FindAsync với 2 khóa)
            var existingLike = await _context.XeYeuThiches.FindAsync(userId, maXe);

            if (existingLike == null)
            {
                // Chưa thích -> Thêm mới
                var newLike = new XeYeuThich
                {
                    MaNguoiDung = (int)userId,
                    MaXe = maXe,
                    NgayLuu = DateTime.Now
                };
                _context.XeYeuThiches.Add(newLike);
                await _context.SaveChangesAsync();
                return Json(new { success = true, status = "added" }); // Trả về "added" để tô đỏ tim
            }
            else
            {
                // Đã thích -> Xóa đi
                _context.XeYeuThiches.Remove(existingLike);
                await _context.SaveChangesAsync();
                return Json(new { success = true, status = "removed" }); // Trả về "removed" để bỏ đỏ tim
            }
        }
    }
}