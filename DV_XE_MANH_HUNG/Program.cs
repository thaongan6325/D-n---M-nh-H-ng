using Microsoft.EntityFrameworkCore;
using Vivu_Xe.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
var connectionString = "Data Source=DESKTOP-VKF78LH;Initial Catalog=VivuXeDB;Integrated Security=True;Encrypt=False";

//Đăng ký dịch vụ Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Hết hạn sau 30 phút không dùng
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddDbContext<VivuXeContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddHttpContextAccessor();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
//Kích hoạt Session
app.UseSession();
app.UseAuthorization();

//Route cho Admin
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

//Route mặc định cho Khách
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
