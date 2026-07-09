using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace Vivu_Xe.Filters
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserID");
            var roleId = context.HttpContext.Session.GetInt32("RoleID");

            // TRƯỜNG HỢP 1: Chưa đăng nhập -> Về Login
            if (userId == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
                return;
            }

            // TRƯỜNG HỢP 2: Đã đăng nhập nhưng không phải Admin (1) hay Nhân viên (2)
            if (roleId != 1 && roleId != 2)
            {   
                // Gán thông báo vào TempData để hiển thị ở trang đích
                if (context.Controller is Controller controller)
                {
                    // Đặt tên key là "SystemNotification" để dùng chung cho nhiều chỗ
                    controller.TempData["Type"] = "error"; // Loại icon: error, success, warning
                    controller.TempData["Message"] = "Bạn không có quyền truy cập vào khu vực quản trị!";
                }

                // Đá về Trang chủ (Home)
                context.Result = new RedirectToActionResult("Index", "Home", new { area = "" });
            }

            base.OnActionExecuting(context);
        }
    }
}