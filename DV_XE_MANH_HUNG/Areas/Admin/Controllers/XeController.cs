using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Vivu_Xe.Data;
using Vivu_Xe.Filters;
using Vivu_Xe.Models;

namespace Vivu_Xe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize] //Check role Admin
    public class XeController : Controller
    {
        private readonly VivuXeContext _context;
        private readonly IWebHostEnvironment _env; // 1. Thêm biến môi trường để xử lý đường dẫn lưu file

        // 2. Inject IWebHostEnvironment vào constructor
        public XeController(VivuXeContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Admin/Xe
        public async Task<IActionResult> Index()
        {
            var vivuXeContext = _context.Xes
                .Include(x => x.MaHangNavigation)
                .Include(x => x.MaLoaiNavigation)
                .OrderByDescending(x => x.MaXe); // Xếp xe mới thêm lên đầu
            return View(await vivuXeContext.ToListAsync());
        }

        // GET: Admin/Xe/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Include các bảng liên quan để hiển thị thông tin chi tiết
            var xe = await _context.Xes
                .Include(x => x.MaHangNavigation)
                .Include(x => x.MaLoaiNavigation)
                .Include(x => x.HinhAnhXes) // Cực kỳ quan trọng để xem được ảnh
                .FirstOrDefaultAsync(m => m.MaXe == id);

            if (xe == null) return NotFound();

            return View(xe);
        }

        // GET: Admin/Xe/Create
        public IActionResult Create()
        {
            // 3. Sửa lại tham số thứ 3 thành "TenHang" và "TenLoai" để giao diện hiện chữ thay vì hiện số
            ViewData["MaHang"] = new SelectList(_context.HangXes, "MaHang", "TenHang");
            ViewData["MaLoai"] = new SelectList(_context.LoaiXes, "MaLoai", "TenLoai");
            return View();
        }

        // POST: Admin/Xe/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 4. Thêm tham số List<IFormFile> uploadHinhAnhs để nhận file từ giao diện
        public async Task<IActionResult> Create([Bind("MaXe,TenXe,BienSo,MaHang,MaLoai,MauSac,NamSanXuat,HopSo,NhienLieu,GiaThueNgay,TienCoc,MoTa,TrangThai,NgayTao")] Xe xe, List<IFormFile> uploadHinhAnhs)
        {
            if (ModelState.IsValid)
            {
                // Gán ngày tạo mặc định nếu rỗng
                if (xe.NgayTao == null) xe.NgayTao = DateTime.Now;
                if (string.IsNullOrEmpty(xe.TrangThai)) xe.TrangThai = "Sẵn sàng";

                _context.Add(xe);
                await _context.SaveChangesAsync(); // Lưu xe để lấy MaXe trước

                // 5. XỬ LÝ LƯU HÌNH ẢNH
                if (uploadHinhAnhs != null && uploadHinhAnhs.Count > 0)
                {
                    // Đường dẫn đến thư mục wwwroot/images/cars
                    string uploadFolder = Path.Combine(_env.WebRootPath, "images", "cars");
                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                    foreach (var file in uploadHinhAnhs)
                    {
                        if (file.Length > 0)
                        {
                            // Đổi tên file để không bị trùng (Guid)
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            string filePath = Path.Combine(uploadFolder, uniqueFileName);

                            // Copy file vào thư mục
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            // Lưu vào Database bảng HinhAnhXe
                            _context.HinhAnhXes.Add(new HinhAnhXe
                            {
                                MaXe = xe.MaXe,
                                DuongDan = "/images/cars/" + uniqueFileName
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["MaHang"] = new SelectList(_context.HangXes, "MaHang", "TenHang", xe.MaHang);
            ViewData["MaLoai"] = new SelectList(_context.LoaiXes, "MaLoai", "TenLoai", xe.MaLoai);
            return View(xe);
        }

        // GET: Admin/Xe/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Include hình ảnh để ném ra View cho Admin xem ảnh cũ
            var xe = await _context.Xes
                .Include(x => x.HinhAnhXes)
                .FirstOrDefaultAsync(x => x.MaXe == id);

            if (xe == null) return NotFound();

            ViewData["MaHang"] = new SelectList(_context.HangXes, "MaHang", "TenHang", xe.MaHang);
            ViewData["MaLoai"] = new SelectList(_context.LoaiXes, "MaLoai", "TenLoai", xe.MaLoai);
            return View(xe);
        }

        // POST: Admin/Xe/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 6. Nhận thêm uploadHinhAnhs giống hàm Create
        public async Task<IActionResult> Edit(int id, [Bind("MaXe,TenXe,BienSo,MaHang,MaLoai,MauSac,NamSanXuat,HopSo,NhienLieu,GiaThueNgay,TienCoc,MoTa,TrangThai,NgayTao")] Xe xe, List<IFormFile> uploadHinhAnhs)
        {
            if (id != xe.MaXe) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(xe);

                    // XỬ LÝ UPLOAD THÊM ẢNH MỚI
                    if (uploadHinhAnhs != null && uploadHinhAnhs.Count > 0)
                    {
                        string uploadFolder = Path.Combine(_env.WebRootPath, "images", "cars");
                        if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                        foreach (var file in uploadHinhAnhs)
                        {
                            if (file.Length > 0)
                            {
                                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(fileStream);
                                }

                                _context.HinhAnhXes.Add(new HinhAnhXe
                                {
                                    MaXe = xe.MaXe,
                                    DuongDan = "/images/cars/" + uniqueFileName
                                });
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!XeExists(xe.MaXe)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaHang"] = new SelectList(_context.HangXes, "MaHang", "TenHang", xe.MaHang);
            ViewData["MaLoai"] = new SelectList(_context.LoaiXes, "MaLoai", "TenLoai", xe.MaLoai);
            return View(xe);
        }

        // ==========================================
        // 7. API XÓA ẢNH (Dùng khi sửa xe)
        // ==========================================
        [HttpPost]
        [Route("Admin/Xe/DeleteImage/{imgId}")]
        public async Task<IActionResult> DeleteImage(int imgId)
        {
            var img = await _context.HinhAnhXes.FindAsync(imgId);
            if (img != null)
            {
                // Xóa file vật lý trong ổ cứng
                var path = Path.Combine(_env.WebRootPath, img.DuongDan.TrimStart('/'));
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

                // Xóa trong DB
                _context.HinhAnhXes.Remove(img);
                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }
            return BadRequest();
        }

        // GET: Admin/Xe/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var xe = await _context.Xes
                .Include(x => x.MaHangNavigation)
                .Include(x => x.MaLoaiNavigation)
                .FirstOrDefaultAsync(m => m.MaXe == id);

            if (xe == null) return NotFound();

            return View(xe);
        }

        // POST: Admin/Xe/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Lấy xe kèm theo danh sách hình ảnh để xóa luôn file trong ổ cứng
            var xe = await _context.Xes
                .Include(x => x.HinhAnhXes)
                .FirstOrDefaultAsync(x => x.MaXe == id);

            if (xe != null)
            {
                // Xóa các file vật lý
                foreach (var img in xe.HinhAnhXes)
                {
                    var path = Path.Combine(_env.WebRootPath, img.DuongDan.TrimStart('/'));
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                }

                _context.HinhAnhXes.RemoveRange(xe.HinhAnhXes); // Xóa ảnh trong DB
                _context.Xes.Remove(xe); // Xóa xe
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool XeExists(int id)
        {
            return _context.Xes.Any(e => e.MaXe == id);
        }
    }
}