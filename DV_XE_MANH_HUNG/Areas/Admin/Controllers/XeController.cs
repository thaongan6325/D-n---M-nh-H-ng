using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vivu_Xe.Data;
using Vivu_Xe.Filters;
using Vivu_Xe.Models;

namespace Vivu_Xe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class XeController : Controller
    {
        private readonly VivuXeContext _context;
        private readonly IWebHostEnvironment _env;

        public XeController(
            VivuXeContext context,
            IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // =====================================================
        // DANH SÁCH XE
        // GET: Admin/Xe
        // =====================================================
        public async Task<IActionResult> Index()
        {
            var danhSachXe = await _context.Xes
                .Include(x => x.MaHangNavigation)
                .Include(x => x.MaLoaiNavigation)
                .OrderByDescending(x => x.MaXe)
                .ToListAsync();

            return View(danhSachXe);
        }

        // =====================================================
        // CHI TIẾT XE
        // GET: Admin/Xe/Details/5
        // =====================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var xe = await _context.Xes
                .Include(x => x.MaHangNavigation)
                .Include(x => x.MaLoaiNavigation)
                .Include(x => x.HinhAnhXes)
                .FirstOrDefaultAsync(x => x.MaXe == id);

            if (xe == null)
            {
                return NotFound();
            }

            return View(xe);
        }

        // =====================================================
        // HIỂN THỊ FORM THÊM XE
        // GET: Admin/Xe/Create
        // =====================================================
        public IActionResult Create()
        {
            LoadSelectLists();
            return View();
        }

        // =====================================================
        // XỬ LÝ THÊM XE
        // POST: Admin/Xe/Create
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "MaXe,TenXe,BienSo,MaHang,MaLoai,MauSac," +
                "NamSanXuat,HopSo,NhienLieu,GiaThueNgay," +
                "TienCoc,MoTa,TrangThai,NgayTao"
            )]
            Xe xe,
            List<IFormFile> uploadHinhAnhs)
        {
            // Chuẩn hóa dữ liệu
            xe.TenXe = xe.TenXe?.Trim() ?? string.Empty;
            xe.BienSo = xe.BienSo?.Trim().ToUpperInvariant() ?? string.Empty;
            xe.MauSac = xe.MauSac?.Trim();
            xe.MoTa = xe.MoTa?.Trim();

            // Kiểm tra biển số trùng
            if (!string.IsNullOrWhiteSpace(xe.BienSo))
            {
                bool bienSoTonTai = await _context.Xes
                    .AnyAsync(x => x.BienSo == xe.BienSo);

                if (bienSoTonTai)
                {
                    ModelState.AddModelError(
                        nameof(xe.BienSo),
                        "Biển số xe đã tồn tại trong hệ thống."
                    );
                }
            }

            if (!ModelState.IsValid)
            {
                LoadSelectLists(xe.MaHang, xe.MaLoai);
                return View(xe);
            }

            xe.NgayTao ??= DateTime.Now;

            if (string.IsNullOrWhiteSpace(xe.TrangThai))
            {
                xe.TrangThai = "Sẵn sàng";
            }

            try
            {
                _context.Xes.Add(xe);
                await _context.SaveChangesAsync();

                await SaveUploadedImagesAsync(
                    xe.MaXe,
                    uploadHinhAnhs
                );

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Thêm xe mới thành công!";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    nameof(xe.BienSo),
                    "Biển số xe đã tồn tại trong hệ thống."
                );

                LoadSelectLists(xe.MaHang, xe.MaLoai);
                return View(xe);
            }
        }

        // =====================================================
        // HIỂN THỊ FORM SỬA XE
        // GET: Admin/Xe/Edit/5
        // =====================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var xe = await _context.Xes
                .Include(x => x.HinhAnhXes)
                .FirstOrDefaultAsync(x => x.MaXe == id);

            if (xe == null)
            {
                return NotFound();
            }

            LoadSelectLists(xe.MaHang, xe.MaLoai);

            return View(xe);
        }

        // =====================================================
        // XỬ LÝ SỬA XE
        // POST: Admin/Xe/Edit/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(
                "MaXe,TenXe,BienSo,MaHang,MaLoai,MauSac," +
                "NamSanXuat,HopSo,NhienLieu,GiaThueNgay," +
                "TienCoc,MoTa,TrangThai,NgayTao"
            )]
            Xe xe,
            List<IFormFile> uploadHinhAnhs)
        {
            if (id != xe.MaXe)
            {
                return NotFound();
            }

            xe.TenXe = xe.TenXe?.Trim() ?? string.Empty;
            xe.BienSo = xe.BienSo?.Trim().ToUpperInvariant() ?? string.Empty;
            xe.MauSac = xe.MauSac?.Trim();
            xe.MoTa = xe.MoTa?.Trim();

            // Kiểm tra biển số trùng với xe khác
            if (!string.IsNullOrWhiteSpace(xe.BienSo))
            {
                bool bienSoTonTai = await _context.Xes
                    .AnyAsync(x =>
                        x.BienSo == xe.BienSo &&
                        x.MaXe != xe.MaXe
                    );

                if (bienSoTonTai)
                {
                    ModelState.AddModelError(
                        nameof(xe.BienSo),
                        "Biển số xe đã được sử dụng cho xe khác."
                    );
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadExistingImagesAsync(xe);
                LoadSelectLists(xe.MaHang, xe.MaLoai);

                return View(xe);
            }

            try
            {
                _context.Xes.Update(xe);

                await SaveUploadedImagesAsync(
                    xe.MaXe,
                    uploadHinhAnhs
                );

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Cập nhật thông tin xe thành công!";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!XeExists(xe.MaXe))
                {
                    return NotFound();
                }

                throw;
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    nameof(xe.BienSo),
                    "Biển số xe đã được sử dụng cho xe khác."
                );

                await LoadExistingImagesAsync(xe);
                LoadSelectLists(xe.MaHang, xe.MaLoai);

                return View(xe);
            }
        }

        // =====================================================
        // XÓA MỘT ẢNH XE
        // POST: Admin/Xe/DeleteImage/5
        // =====================================================
        [HttpPost]
        [Route("Admin/Xe/DeleteImage/{imgId}")]
        public async Task<IActionResult> DeleteImage(int imgId)
        {
            var img = await _context.HinhAnhXes
                .FindAsync(imgId);

            if (img == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không tìm thấy hình ảnh."
                });
            }

            try
            {
                DeletePhysicalImage(img.DuongDan);

                _context.HinhAnhXes.Remove(img);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Xóa hình ảnh thành công."
                });
            }
            catch
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể xóa hình ảnh."
                });
            }
        }

        // =====================================================
        // HIỂN THỊ TRANG XÓA XE
        // GET: Admin/Xe/Delete/5
        // =====================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var xe = await _context.Xes
                .Include(x => x.MaHangNavigation)
                .Include(x => x.MaLoaiNavigation)
                .FirstOrDefaultAsync(x => x.MaXe == id);

            if (xe == null)
            {
                return NotFound();
            }

            return View(xe);
        }

        // =====================================================
        // XỬ LÝ XÓA XE
        // POST: Admin/Xe/Delete/5
        // =====================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var xe = await _context.Xes
                .Include(x => x.HinhAnhXes)
                .FirstOrDefaultAsync(x => x.MaXe == id);

            if (xe == null)
            {
                TempData["ErrorMessage"] =
                    "Không tìm thấy xe cần xóa.";

                return RedirectToAction(nameof(Index));
            }

            // Không cho xóa xe đã có đơn thuê
            bool daCoDonThue = await _context.DonDatXes
                .AnyAsync(d => d.MaXe == id);

            if (daCoDonThue)
            {
                TempData["ErrorMessage"] =
                    "Không thể xóa xe vì xe đã có đơn thuê trong hệ thống.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Chỉ xóa file ảnh sau khi chắc chắn xe được phép xóa
                foreach (var img in xe.HinhAnhXes)
                {
                    DeletePhysicalImage(img.DuongDan);
                }

                _context.HinhAnhXes.RemoveRange(xe.HinhAnhXes);
                _context.Xes.Remove(xe);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Xóa xe thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] =
                    "Không thể xóa xe vì xe đang có dữ liệu liên quan.";
            }
            catch
            {
                TempData["ErrorMessage"] =
                    "Đã xảy ra lỗi khi xóa xe.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // LƯU ẢNH XE
        // =====================================================
        private async Task SaveUploadedImagesAsync(
            int maXe,
            List<IFormFile> uploadHinhAnhs)
        {
            if (uploadHinhAnhs == null ||
                uploadHinhAnhs.Count == 0)
            {
                return;
            }

            string uploadFolder = Path.Combine(
                _env.WebRootPath,
                "images",
                "cars"
            );

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            foreach (var file in uploadHinhAnhs)
            {
                if (file == null || file.Length <= 0)
                {
                    continue;
                }

                string extension =
                    Path.GetExtension(file.FileName);

                string uniqueFileName =
                    Guid.NewGuid().ToString("N") + extension;

                string filePath = Path.Combine(
                    uploadFolder,
                    uniqueFileName
                );

                await using (var fileStream =
                    new FileStream(
                        filePath,
                        FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                _context.HinhAnhXes.Add(
                    new HinhAnhXe
                    {
                        MaXe = maXe,
                        DuongDan =
                            "/images/cars/" +
                            uniqueFileName
                    });
            }
        }

        // =====================================================
        // NẠP LẠI ẢNH CŨ KHI FORM EDIT CÓ LỖI
        // =====================================================
        private async Task LoadExistingImagesAsync(Xe xe)
        {
            xe.HinhAnhXes = await _context.HinhAnhXes
                .Where(x => x.MaXe == xe.MaXe)
                .ToListAsync();
        }

        // =====================================================
        // XÓA FILE ẢNH VẬT LÝ
        // =====================================================
        private void DeletePhysicalImage(string? duongDan)
        {
            if (string.IsNullOrWhiteSpace(duongDan))
            {
                return;
            }

            string relativePath = duongDan
                .TrimStart('/')
                .Replace('/', Path.DirectorySeparatorChar);

            string fullPath = Path.Combine(
                _env.WebRootPath,
                relativePath
            );

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        // =====================================================
        // NẠP DANH SÁCH HÃNG XE VÀ LOẠI XE
        // =====================================================
        private void LoadSelectLists(
            int? maHang = null,
            int? maLoai = null)
        {
            ViewData["MaHang"] = new SelectList(
                _context.HangXes,
                "MaHang",
                "TenHang",
                maHang
            );

            ViewData["MaLoai"] = new SelectList(
                _context.LoaiXes,
                "MaLoai",
                "TenLoai",
                maLoai
            );
        }

        // =====================================================
        // KIỂM TRA XE CÓ TỒN TẠI
        // =====================================================
        private bool XeExists(int id)
        {
            return _context.Xes.Any(x => x.MaXe == id);
        }
    }
}