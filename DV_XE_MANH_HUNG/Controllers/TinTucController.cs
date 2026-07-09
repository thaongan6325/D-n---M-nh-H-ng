using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Vivu_Xe.Data;
using Vivu_Xe.Models;

namespace Vivu_Xe.Controllers
{
    public class TinTucController : Controller
    {
        private readonly VivuXeContext _context;

        public TinTucController(VivuXeContext context)
        {
            _context = context;
        }

        // GET: /TinTuc (Trang danh sách)
        public async Task<IActionResult> Index(int? maDanhMuc)
        {
            // 1. Kéo danh sách danh mục ra để làm menu bên trái
            ViewBag.DanhMucs = await _context.DanhMucTins
                .Where(d => d.TrangThai == true)
                .ToListAsync();

            ViewBag.CurrentCategory = maDanhMuc;

            // 2. Kéo danh sách bài viết (Chỉ lấy bài Đã xuất bản)
            var query = _context.TinTucs
                .Include(t => t.DanhMucTin)
                .Include(t => t.TacGia)
                .Where(t => t.TrangThai == true); // Chỉ lấy bài viết công khai

            // Nếu khách click vào 1 danh mục, thì lọc theo danh mục đó
            if (maDanhMuc.HasValue)
            {
                query = query.Where(t => t.MaDanhMuc == maDanhMuc.Value);
            }

            var danhSachTin = await query
                .OrderByDescending(t => t.NgayDang) // Mới nhất xếp trên
                .ToListAsync();

            return View(danhSachTin);
        }

        // GET: /TinTuc/Details/5 (Trang đọc bài viết)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tinTuc = await _context.TinTucs
                .Include(t => t.TacGia)
                .Include(t => t.DanhMucTin)
                .FirstOrDefaultAsync(m => m.MaTinTuc == id && m.TrangThai == true);

            if (tinTuc == null) return NotFound();

            // 1. Tự động tăng lượt xem lên 1
            tinTuc.LuotXem = (tinTuc.LuotXem ?? 0) + 1;
            _context.Update(tinTuc);
            await _context.SaveChangesAsync();

            // 2. Lấy 3 bài viết liên quan (Cùng danh mục) để hiển thị cuối bài
            ViewBag.TinLienQuan = await _context.TinTucs
                .Where(t => t.MaDanhMuc == tinTuc.MaDanhMuc && t.MaTinTuc != id && t.TrangThai == true)
                .OrderByDescending(t => t.NgayDang)
                .Take(3)
                .ToListAsync();

            return View(tinTuc);
        }
    }
}