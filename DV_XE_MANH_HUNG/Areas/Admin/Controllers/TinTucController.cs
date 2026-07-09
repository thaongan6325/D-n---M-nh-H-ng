using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vivu_Xe.Data;
using Vivu_Xe.Filters;
using Vivu_Xe.Models;

namespace Vivu_Xe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class TinTucController : Controller
    {
        private readonly VivuXeContext _context;
        private readonly IWebHostEnvironment _env;

        public TinTucController(VivuXeContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Admin/TinTuc
        public async Task<IActionResult> Index()
        {
            var news = await _context.TinTucs
                .Include(t => t.DanhMucTin)
                .Include(t => t.TacGia)
                .OrderByDescending(t => t.NgayDang)
                .ToListAsync();
            return View(news);
        }

        // API: Ẩn/Hiện bài viết nhanh bằng AJAX
        [HttpPost]
        [Route("Admin/TinTuc/ToggleStatus/{id}")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var tin = await _context.TinTucs.FindAsync(id);
            if (tin == null) return NotFound();

            // Đảo ngược trạng thái BIT (1 -> 0, 0 -> 1)
            tin.TrangThai = !(tin.TrangThai ?? true);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, isPublic = tin.TrangThai });
        }

        // POST: Admin/TinTuc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tinTuc = await _context.TinTucs.FindAsync(id);
            if (tinTuc != null)
            {
                // Xóa file ảnh đại diện trong ổ cứng để tránh rác server
                if (!string.IsNullOrEmpty(tinTuc.HinhAnhDaiDien) && !tinTuc.HinhAnhDaiDien.Contains("default"))
                {
                    var path = Path.Combine(_env.WebRootPath, tinTuc.HinhAnhDaiDien.TrimStart('/'));
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                }

                _context.TinTucs.Remove(tinTuc);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        // TẠO MỚI (CREATE)
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.MaDanhMuc = new SelectList(_context.DanhMucTins.Where(d => d.TrangThai == true), "MaDanhMuc", "TenDanhMuc");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Nhận đối tượng TinTuc từ CKEditor và file ảnh Thumbnail
        public async Task<IActionResult> Create(TinTuc tinTuc, IFormFile HinhAnhDaiDien)
        {
            ModelState.Remove("TacGia");
            ModelState.Remove("DanhMucTin");
            ModelState.Remove("HinhAnhDaiDien");
            if (ModelState.IsValid)
            {
                // 1. Gán các giá trị mặc định
                tinTuc.NgayDang = DateTime.Now;
                tinTuc.LuotXem = 0;
                // Lấy ID người đang đăng nhập (Tác giả)
                tinTuc.MaNguoiDung = HttpContext.Session.GetInt32("UserID");

                // 2. Xử lý Upload Ảnh Thumbnail
                if (HinhAnhDaiDien != null && HinhAnhDaiDien.Length > 0)
                {
                    string uploadFolder = Path.Combine(_env.WebRootPath, "images", "news");
                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + HinhAnhDaiDien.FileName;
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await HinhAnhDaiDien.CopyToAsync(fileStream);
                    }
                    tinTuc.HinhAnhDaiDien = "/images/news/" + uniqueFileName;
                }
                else
                {
                    // Nếu không có ảnh, dùng ảnh mặc định
                    tinTuc.HinhAnhDaiDien = "/images/news-default.jpg";
                }

                _context.Add(tinTuc);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đăng bài viết mới thành công!";
                return RedirectToAction(nameof(Index)); // Chuyển về trang danh sách (Bạn tự tạo hàm Index nhé)
            }

            ViewBag.MaDanhMuc = new SelectList(_context.DanhMucTins.Where(d => d.TrangThai == true), "MaDanhMuc", "TenDanhMuc", tinTuc.MaDanhMuc);
            return View(tinTuc);
        }
        // ==========================================
        // API NHẬN ẢNH TỪ CKEDITOR 5
        // ==========================================
        [HttpPost]
        [Route("Admin/TinTuc/UploadImageCKEditor")]
        public async Task<IActionResult> UploadImageCKEditor(IFormFile upload)
        {
            // CKEditor mặc định gửi file lên qua biến tên là 'upload'
            if (upload != null && upload.Length > 0)
            {
                // Tạo thư mục riêng cho ảnh trong bài viết
                string uploadFolder = Path.Combine(_env.WebRootPath, "images", "news", "uploads");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                // Tạo tên file ngẫu nhiên
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + upload.FileName;
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                // Lưu file vào ổ cứng
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(fileStream);
                }

                // Tạo đường dẫn trả về cho CKEditor hiển thị
                var url = "/images/news/uploads/" + uniqueFileName;

                // CKEditor 5 bắt buộc server phải trả về JSON theo đúng format này:
                return Json(new
                {
                    uploaded = 1,
                    fileName = uniqueFileName,
                    url = url
                });
            }

            // Trả về lỗi nếu upload thất bại
            return Json(new { uploaded = 0, error = new { message = "Lỗi tải ảnh lên server." } });
        }
    }
}