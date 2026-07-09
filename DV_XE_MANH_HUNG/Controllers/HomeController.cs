using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Vivu_Xe.Data;
using Vivu_Xe.Models;
using Vivu_Xe.Models.ViewModels;

namespace Vivu_Xe.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly VivuXeContext _context; // 1. Khai báo Context

        // 2. Inject Context vào Constructor
        public HomeController(ILogger<HomeController> logger, VivuXeContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Lấy TẤT CẢ xe đang sẵn sàng
            var allXe = await _context.Xes
                                     .Include(x => x.HinhAnhXes)
                                     .Include(x => x.MaLoaiNavigation) // Join bảng Loại xe để lọc tên
                                     .Where(x => x.TrangThai == "Sẵn sàng")
                                     .OrderByDescending(x => x.MaXe)
                                     .ToListAsync();

            // 2. Khởi tạo ViewModel và chia danh sách
            var model = new HomeViewModel();

            // Lọc lấy 4 xe Ô tô 

            model.DanhSachOto = allXe
                                .Where(x => x.MaLoaiNavigation.TenLoai.ToLower().Contains("chỗ")) // xe 4 chỗ - xe 7 chỗ
                                .Take(4)
                                .ToList();
            // --- LOGIC KIỂM TRA XE ĐÃ THÍCH ---
            var userId = HttpContext.Session.GetInt32("UserID");
            var likedCarIds = new List<int>();

            if (userId != null)
            {
                // Lấy danh sách ID các xe mà user này đã thích
                likedCarIds = await _context.XeYeuThiches
                                    .Where(y => y.MaNguoiDung == userId)
                                    .Select(y => y.MaXe)
                                    .ToListAsync();
            }

            // Truyền danh sách này sang View qua ViewBag
            ViewBag.LikedCarIds = likedCarIds;
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}