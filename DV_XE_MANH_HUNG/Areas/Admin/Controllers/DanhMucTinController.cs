using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Vivu_Xe.Data;
using Vivu_Xe.Models;
using Vivu_Xe.Filters;

namespace Vivu_Xe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class DanhMucTinController : Controller
    {
        private readonly VivuXeContext _context;

        public DanhMucTinController(VivuXeContext context)
        {
            _context = context;
        }

        // GET: Admin/DanhMucTin
        public async Task<IActionResult> Index()
        {
            // Dùng Include để kéo theo TinTucs nhằm đếm xem mỗi danh mục có bao nhiêu bài
            var danhMucs = await _context.DanhMucTins
                .Include(d => d.TinTucs)
                .OrderByDescending(d => d.MaDanhMuc)
                .ToListAsync();
            return View(danhMucs);
        }

        // GET: Admin/DanhMucTin/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/DanhMucTin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenDanhMuc,MoTa")] DanhMucTin danhMuc)
        {
            if (ModelState.IsValid)
            {
                danhMuc.TrangThai = true; // Mới tạo mặc định là Hiện
                _context.Add(danhMuc);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Thêm danh mục mới thành công!";
                TempData["Type"] = "success";
                return RedirectToAction(nameof(Index));
            }
            return View(danhMuc);
        }

        // GET: Admin/DanhMucTin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var danhMuc = await _context.DanhMucTins.FindAsync(id);
            if (danhMuc == null) return NotFound();

            return View(danhMuc);
        }

        // POST: Admin/DanhMucTin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDanhMuc,TenDanhMuc,MoTa,TrangThai")] DanhMucTin danhMuc)
        {
            if (id != danhMuc.MaDanhMuc) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(danhMuc);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Cập nhật danh mục thành công!";
                TempData["Type"] = "success";
                return RedirectToAction(nameof(Index));
            }
            return View(danhMuc);
        }

        // API: Ẩn/Hiện nhanh danh mục
        [HttpPost]
        [Route("Admin/DanhMucTin/ToggleStatus/{id}")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var dm = await _context.DanhMucTins.FindAsync(id);
            if (dm == null) return NotFound();

            dm.TrangThai = !(dm.TrangThai ?? true);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, isPublic = dm.TrangThai });
        }

        // POST: Admin/DanhMucTin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var danhMuc = await _context.DanhMucTins
                .Include(d => d.TinTucs)
                .FirstOrDefaultAsync(d => d.MaDanhMuc == id);

            if (danhMuc != null)
            {
                // KIỂM TRA RÀNG BUỘC: Nếu có bài viết thì cấm xóa
                if (danhMuc.TinTucs.Any())
                {
                    TempData["Message"] = $"Không thể xóa! Danh mục này đang chứa {danhMuc.TinTucs.Count} bài viết.";
                    TempData["Type"] = "error";
                    return RedirectToAction(nameof(Index));
                }

                _context.DanhMucTins.Remove(danhMuc);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Đã xóa danh mục thành công!";
                TempData["Type"] = "success";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}