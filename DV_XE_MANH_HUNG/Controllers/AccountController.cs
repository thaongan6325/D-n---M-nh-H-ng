using Microsoft.AspNetCore.Mvc;
using Vivu_Xe.Data;
using Vivu_Xe.Models;
using Vivu_Xe.Helpers; // Gọi thư mục chứa hàm mã hóa
using Microsoft.EntityFrameworkCore;


namespace Vivu_Xe.Controllers
{
    public class AccountController : Controller
    {
        private readonly VivuXeContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment; //Lưu ảnh
        public AccountController(VivuXeContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        
        //ĐĂNG KÝ
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(NguoiDung nguoiDung, string XacNhanMatKhau)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra Email đã tồn tại chưa
                var checkEmail = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == nguoiDung.Email);
                if (checkEmail != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng!");
                    return View(nguoiDung);
                }

                // Kiểm tra xác nhận mật khẩu
                if (nguoiDung.MatKhau != XacNhanMatKhau)
                {
                    ModelState.AddModelError("XacNhanMatKhau", "Mật khẩu xác nhận không khớp!");
                    return View(nguoiDung);
                }

                // Cấu hình thông tin mặc định
                nguoiDung.MatKhau = MyUtil.ToMd5(nguoiDung.MatKhau); // Mã hóa mật khẩu
                nguoiDung.MaVaiTro = 3; // Mặc định là Khách hàng (ID=3)
                nguoiDung.TrangThai = true; // Hoạt động
                nguoiDung.NgayTao = DateTime.Now;

                // Nếu chưa có ảnh đại diện thì set ảnh mặc định
                if (string.IsNullOrEmpty(nguoiDung.AnhDaiDien))
                {
                    nguoiDung.AnhDaiDien = "/images/avatars/default.png";
                }

                _context.Add(nguoiDung);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            return View(nguoiDung);
        }

        //ĐĂNG NHẬP
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // Lưu lại cái link cũ vào ViewBag để tí nữa truyền vào Form
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string TaiKhoan, string MatKhau, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Mã hóa mật khẩu nhập vào để so sánh với DB
                string passHash = MyUtil.ToMd5(MatKhau);

                // Tìm User trong DB
                // (Giả sử bạn dùng chung cột Email để lưu tài khoản Admin như 'admin', 
                // nếu DB có cột riêng thì thêm: u.Email == TaiKhoan || u.TenDangNhap == TaiKhoan)
                var user = await _context.NguoiDungs
                    .FirstOrDefaultAsync(u => u.Email == TaiKhoan && u.MatKhau == passHash);

                if (user != null)
                {
                    // --- CHỐT CHẶN BẢO MẬT: KIỂM TRA ĐỊNH DẠNG MAIL ---
                    bool isEmailFormat = TaiKhoan.Contains("@");

                    // Nếu nhập không có @ MÀ chức vụ KHÔNG PHẢI Admin (MaVaiTro != 1) -> Từ chối!
                    if (!isEmailFormat && user.MaVaiTro != 1)
                    {
                        ViewBag.Error = "Nhân viên và Khách hàng bắt buộc phải đăng nhập bằng Email (có chứa @)!";
                        ViewBag.ReturnUrl = returnUrl;
                        return View();
                    }

                    // Kiểm tra tài khoản có bị khóa không
                    if (user.TrangThai == false)
                    {
                        ViewBag.Error = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Admin.";
                        ViewBag.ReturnUrl = returnUrl;
                        return View();
                    }

                    // Đăng nhập thành công -> Lưu Session
                    HttpContext.Session.SetString("UserEmail", user.Email ?? "");
                    HttpContext.Session.SetString("UserName", user.HoTen ?? "");
                    HttpContext.Session.SetInt32("UserID", user.MaNguoiDung);
                    HttpContext.Session.SetInt32("RoleID", user.MaVaiTro ?? 3);

                    // Chuyển hướng dựa trên vai trò
                    if (user.MaVaiTro == 1 || user.MaVaiTro == 2) // Admin hoặc Nhân viên
                    {
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    }

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl); //Ưu tiên về link cũ
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Error = "Tài khoản hoặc Mật khẩu không đúng!";
                }
            }

            // Gửi lại returnUrl ra view nếu đăng nhập sai (để khách nhập lại không bị mất link)
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //ĐĂNG XUẤT
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa hết session
            return RedirectToAction("Index", "Home");
        }
        // Action xem Lịch sử đặt xe của khách hàng
        public async Task<IActionResult> History()
        {
            // 1. Kiểm tra đăng nhập
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login");

            // 2. Lấy danh sách đơn hàng của Khách này
            var myOrders = await _context.DonDatXes
                .Include(d => d.MaXeNavigation)
                    .ThenInclude(x => x.HinhAnhXes)
                .Include(d => d.DanhGias)
                .Where(d => d.MaNguoiDung == userId)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            return View(myOrders);
        }
        //ACTION check trạng thái 
        private async Task<int> CheckTrangThaiGiayTo(int userId)
        {
            // Lấy tất cả giấy tờ của user này
            var listGiayTo = await _context.GiayTos
                                     .Where(g => g.MaNguoiDung == userId)
                                     .ToListAsync();

            // Kiểm tra xem có GPLX và CCCD chưa
            var gplx = listGiayTo.FirstOrDefault(g => g.LoaiGiayTo == "GPLX");
            var cccd = listGiayTo.FirstOrDefault(g => g.LoaiGiayTo == "CCCD");

            // TRƯỜNG HỢP 1: Chưa xác thực (Thiếu 1 trong 2 hoặc cả 2)
            if (gplx == null || cccd == null)
            {
                return 0; // Chưa xác thực
            }

            // TRƯỜNG HỢP 2: Đã xác thực (Cả 2 đều đã được duyệt = true)
            // Lưu ý: DaXacThuc là bool? nên cần so sánh == true
            if (gplx.DaXacThuc == true && cccd.DaXacThuc == true)
            {
                return 2; // Đã xác thực
            }

            // TRƯỜNG HỢP 3: Chờ duyệt (Đã đủ 2 cái, nhưng có cái chưa duyệt)
            return 1; // Chờ duyệt
        }
        //Action Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login");

            var user = await _context.NguoiDungs.FindAsync(userId);
            var listGiayTo = await _context.GiayTos
                            .Where(g => g.MaNguoiDung == userId)
                            .ToListAsync();

            // Tìm dòng có loại là GPLX
            var gplx = listGiayTo.FirstOrDefault(g => g.LoaiGiayTo == "GPLX");
            // Tìm dòng có loại là CCCD
            var cccd = listGiayTo.FirstOrDefault(g => g.LoaiGiayTo == "CCCD");

            // Truyền số giấy tờ sang View qua ViewBag
            ViewBag.SoGPLX = gplx?.SoGiayTo ?? "";
            ViewBag.SoCCCD = cccd?.SoGiayTo ?? "";
            // Gọi hàm check và truyền qua ViewBag
            ViewBag.TrangThaiXacThuc = await CheckTrangThaiGiayTo(userId.Value);

            return View(user);
        }

        //Action Update Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(NguoiDung model, IFormFile? fileAvatar, string? soGPLX, string? soCCCD)
        {
            // 1. Kiểm tra đăng nhập
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login");

            // 2. Lấy thông tin user hiện tại trong DB
            var userInDb = await _context.NguoiDungs.FindAsync(userId);
            if (userInDb == null) return NotFound();

            try
            {
                
                userInDb.HoTen = model.HoTen;
                userInDb.SoDienThoai = model.SoDienThoai;
                userInDb.DiaChi = model.DiaChi;

                // GIẤY PHÉP LÁI XE (GPLX)
                // Tìm xem user đã có dòng GPLX trong bảng GiayTo chưa
                var docGPLX = await _context.GiayTos
                    .FirstOrDefaultAsync(g => g.MaNguoiDung == userId && g.LoaiGiayTo == "GPLX");

                if (docGPLX != null)
                {
                    // Trường hợp ĐÃ CÓ -> Cập nhật số
                    docGPLX.SoGiayTo = soGPLX;
                    _context.Update(docGPLX);
                }
                else if (!string.IsNullOrEmpty(soGPLX))
                {
                    // Trường hợp CHƯA CÓ và người dùng có nhập số -> Tạo mới
                    var newGPLX = new GiayTo
                    {
                        MaNguoiDung = userId,
                        LoaiGiayTo = "GPLX",
                        SoGiayTo = soGPLX,
                        DaXacThuc = false, // Mới nhập số thì mặc định chưa xác thực
                        NgayTaiLen = DateTime.Now
                    };
                    _context.GiayTos.Add(newGPLX);
                }

                // --- XỬ LÝ 2: CĂN CƯỚC CÔNG DÂN (CCCD) ---
                var docCCCD = await _context.GiayTos
                    .FirstOrDefaultAsync(g => g.MaNguoiDung == userId && g.LoaiGiayTo == "CCCD");

                if (docCCCD != null)
                {
                    docCCCD.SoGiayTo = soCCCD;
                    _context.Update(docCCCD);
                }
                else if (!string.IsNullOrEmpty(soCCCD))
                {
                    var newCCCD = new GiayTo
                    {
                        MaNguoiDung = userId,
                        LoaiGiayTo = "CCCD",
                        SoGiayTo = soCCCD,
                        DaXacThuc = false,
                        NgayTaiLen = DateTime.Now
                    };
                    _context.GiayTos.Add(newCCCD);
                }

                //XỬ LÝ UPLOAD ẢNH ĐẠI DIỆN
                if (fileAvatar != null && fileAvatar.Length > 0)
                {
                    // Tạo tên file và đường dẫn
                    string fileName = $"avatar_{userId}_{Guid.NewGuid()}{Path.GetExtension(fileAvatar.FileName)}";
                    string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "avatars");

                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                    string filePath = Path.Combine(uploadFolder, fileName);

                    // Lưu file vật lý
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileAvatar.CopyToAsync(stream);
                    }

                    // Xóa ảnh cũ (nếu không phải default)
                    if (!string.IsNullOrEmpty(userInDb.AnhDaiDien) && !userInDb.AnhDaiDien.Contains("default"))
                    {
                        string oldFileName = Path.GetFileName(userInDb.AnhDaiDien);
                        string oldPath = Path.Combine(uploadFolder, oldFileName);
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    // Cập nhật đường dẫn vào DB
                    userInDb.AnhDaiDien = "/images/avatars/" + fileName;
                }

                //Lưu tất cả.

                await _context.SaveChangesAsync();

                // Cập nhật Session tên hiển thị
                HttpContext.Session.SetString("UserName", userInDb.HoTen);

                TempData["Success"] = "Cập nhật hồ sơ thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("Profile");
        }
        //Action xác thực
        [HttpGet]
        public async Task<IActionResult> Verification()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login");

            var user = await _context.NguoiDungs.FindAsync(userId);

            // 1. Lấy danh sách giấy tờ của user
            var listGiayTo = await _context.GiayTos
                                     .Where(g => g.MaNguoiDung == userId)
                                     .OrderByDescending(g => g.NgayTaiLen)
                                     .ToListAsync();

            // 2. Truyền sang View qua ViewBag
            ViewBag.ListGiayTo = listGiayTo;

            // 3. Logic trạng thái tổng (như bước trước)
            ViewBag.TrangThaiXacThuc = await CheckTrangThaiGiayTo(userId.Value);

            return View(user);
        }
        // 2. POST: Xử lý Upload giấy tờ
        [HttpPost]
        public async Task<IActionResult> UploadDocuments(string LoaiGiayTo, IFormFile MatTruoc, IFormFile MatSau)
        {
            // 1. Kiểm tra đăng nhập
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login");

            // 2. Kiểm tra file
            if (MatTruoc == null || MatSau == null)
            {
                TempData["Error"] = "Vui lòng tải lên đủ 2 mặt giấy tờ!";
                return RedirectToAction("Verification");
            }

            try
            {
                // 3. Chuẩn bị thư mục lưu
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "documents");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                // 4. Lưu ảnh MẶT TRƯỚC
                string fNameFront = $"doc_{userId}_{LoaiGiayTo}_front_{Guid.NewGuid()}{Path.GetExtension(MatTruoc.FileName)}";
                using (var stream = new FileStream(Path.Combine(uploadFolder, fNameFront), FileMode.Create))
                {
                    await MatTruoc.CopyToAsync(stream);
                }

                // 5. Lưu ảnh MẶT SAU
                string fNameBack = $"doc_{userId}_{LoaiGiayTo}_back_{Guid.NewGuid()}{Path.GetExtension(MatSau.FileName)}";
                using (var stream = new FileStream(Path.Combine(uploadFolder, fNameBack), FileMode.Create))
                {
                    await MatSau.CopyToAsync(stream);
                }

                // 6. LƯU VÀO DATABASE
                var giayTo = new GiayTo
                {
                    MaNguoiDung = userId,
                    LoaiGiayTo = LoaiGiayTo,

                    // Lưu đường dẫn ảnh
                    AnhMatTruoc = "/images/documents/" + fNameFront,
                    AnhMatSau = "/images/documents/" + fNameBack,

                    // Trạng thái xác thực (0: Chưa duyệt -> false)
                    DaXacThuc = false,

                    NgayTaiLen = DateTime.Now,

                    // SoGiayTo: Tạm thời để null vì form chưa có ô nhập số
                    SoGiayTo = null
                };

                _context.GiayTos.Add(giayTo);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Gửi hồ sơ thành công! Vui lòng chờ duyệt.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction("Verification");
        }
    }
}