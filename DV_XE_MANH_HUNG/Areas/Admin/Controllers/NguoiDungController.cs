using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vivu_Xe.Data;
using Vivu_Xe.Filters;

namespace Vivu_Xe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class NguoiDungController : Controller
    {
        private readonly VivuXeContext _context;

        public NguoiDungController(VivuXeContext context)
        {
            _context = context;
        }

        // 1. Danh sách người dùng
        // URL: /Admin/NguoiDung?status=cho_duyet
        public async Task<IActionResult> Index(string status)
        {
            var query = _context.NguoiDungs
                .Include(u => u.GiayTos) // Include bảng Giấy tờ để đếm
                .AsQueryable();

            if (status == "cho_duyet")
            {
                // Logic: Lấy những người có ít nhất 1 giấy tờ chưa xác thực
                query = query.Where(u => u.GiayTos.Any(g => g.DaXacThuc == false));
                ViewBag.FilterStatus = "cho_duyet";
            }

            var users = await query.OrderByDescending(u => u.NgayTao).ToListAsync();
            return View(users);
        }

        // 2. Xem chi tiết & Duyệt hồ sơ
        // URL: /Admin/NguoiDung/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.NguoiDungs
                .Include(u => u.GiayTos)
                .FirstOrDefaultAsync(u => u.MaNguoiDung == id);

            if (user == null) return NotFound();

            return View(user);
        }

        // 3. API Duyệt giấy tờ
        // URL đổi thành: Admin/NguoiDung/ApproveDocument/...
        [HttpPost]
        [Route("Admin/NguoiDung/ApproveDocument/{id}")]
        public async Task<IActionResult> ApproveDocument(int id)
        {
            var giayTo = await _context.GiayTos.FindAsync(id);
            if (giayTo == null) return NotFound(new { success = false, message = "Không tìm thấy giấy tờ" });

            giayTo.DaXacThuc = true; // Đánh dấu đã duyệt


            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã duyệt giấy tờ thành công!" });
        }
        // 4. Khóa tài khoản
        [HttpPost]
        [Route("Admin/NguoiDung/ToggleLock/{id}")]
        public async Task<IActionResult> ToggleLock(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound(new { success = false, message = "Không tìm thấy người dùng." });

            // TrangThai là BIT (bool). Đảo ngược trạng thái hiện tại. 
            // Dùng (user.TrangThai ?? true) để phòng hờ trường hợp dữ liệu cũ bị NULL.
            user.TrangThai = !(user.TrangThai ?? true);

            await _context.SaveChangesAsync();

            // isLocked sẽ là true nếu TrangThai == false (tức là = 0)
            bool isLocked = (user.TrangThai == false);
            return Ok(new { success = true, isLocked = isLocked, message = "Cập nhật trạng thái thành công." });
        }
        // Từ chối giấy tờ
        [HttpPost]
        [Route("Admin/NguoiDung/RejectDocument/{id}")]
        public async Task<IActionResult> RejectDocument(int id)
        {
            var giayTo = await _context.GiayTos.FindAsync(id);

            if (giayTo == null)
            {
                return Json(new { success = false, message = "Không tìm thấy giấy tờ này trong hệ thống!" });
            }

            try
            {
                // Tùy chọn: Nếu bạn có lưu file vật lý trong wwwroot/uploads thì nên viết code xóa file ảnh ở đây để đỡ nặng server

                // Xóa record trong Database
                _context.GiayTos.Remove(giayTo);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}