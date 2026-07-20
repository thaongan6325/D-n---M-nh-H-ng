
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
