
document.addEventListener("DOMContentLoaded", function () {

    
    const hamburger = document.getElementById("hamburger-btn");
    const navbarMenu = document.getElementById("navbar-menu");

    if (hamburger && navbarMenu) {
        hamburger.addEventListener("click", () => {
            navbarMenu.classList.toggle("active");
        });
    }

   
    const dropdownToggles = document.querySelectorAll(".dropdown > a");

    dropdownToggles.forEach(toggle => {
        toggle.addEventListener("click", function (e) {
            e.preventDefault(); // Ngăn không cho thẻ a chuyển trang

            // Tìm menu con (ul.dropdown-menu) nằm ngay kế bên
            const menu = this.nextElementSibling;

            if (menu) {
                // Đóng tất cả các menu khác đang mở (để tránh bị rối mắt)
                document.querySelectorAll(".dropdown-menu").forEach(otherMenu => {
                    if (otherMenu !== menu) {
                        otherMenu.classList.remove("show");
                    }
                });

                // Bật/Tắt (Toggle) menu hiện tại
                menu.classList.toggle("show");
            }
        });
    });

    document.addEventListener("click", function (e) {
        // Nếu click không trúng vào bất kỳ thành phần nào có class .dropdown
        if (!e.target.closest(".dropdown")) {
            document.querySelectorAll(".dropdown-menu").forEach(menu => {
                menu.classList.remove("show");
            });
        }
    });
});
function toggleFavorite(maXe, btnElement) {
    // Gọi Ajax xuống Server
    $.ajax({
        url: '/Ajax/ToggleFavorite',
        type: 'POST',
        data: { maXe: maXe },
        success: function (response) {
            if (response.success) {
                // TRƯỜNG HỢP: ĐÃ ĐĂNG NHẬP
                const icon = btnElement.querySelector('i');

                if (response.status === "added") {
                    // Server bảo đã thêm -> Đổi thành tim đỏ
                    btnElement.classList.add('active');
                    icon.classList.remove('far'); // Bỏ rỗng
                    icon.classList.add('fas');    // Thêm đặc

                    // Toast thông báo nhỏ góc phải
                    Swal.fire({
                        toast: true,
                        position: 'top-end',
                        icon: 'success',
                        title: 'Đã lưu vào xe yêu thích',
                        showConfirmButton: false,
                        timer: 1500
                    });
                } else {
                    // Server bảo đã xóa -> Đổi về tim xám
                    btnElement.classList.remove('active');
                    icon.classList.remove('fas');
                    icon.classList.add('far');
                }
            } else {
                // TRƯỜNG HỢP: CHƯA ĐĂNG NHẬP
                if (response.message === "LoginRequired") {
                    Swal.fire({
                        title: 'Bạn chưa đăng nhập!',
                        text: "Vui lòng đăng nhập để lưu xe yêu thích.",
                        icon: 'info',
                        showCancelButton: true,
                        confirmButtonColor: '#035FA5', // Màu xanh chủ đạo
                        cancelButtonColor: '#d33',
                        confirmButtonText: 'Đăng nhập ngay',
                        cancelButtonText: 'Để sau'
                    }).then((result) => {
                        if (result.isConfirmed) {
                            // Chuyển hướng sang trang Login
                            window.location.href = "/Account/Login";
                        }
                    });
                }
            }
        },
        error: function () {
            Swal.fire('Lỗi', 'Không thể kết nối đến server', 'error');
        }
    });
}