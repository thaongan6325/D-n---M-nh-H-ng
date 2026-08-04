package tests;

import base.BaseTest;
import org.testng.annotations.Test;
import pages.LoginPage;
import pages.VehiclePage;
import utils.ConfigReader;
import org.openqa.selenium.By;
import org.openqa.selenium.WebElement;
import org.testng.Assert;
import utils.WaitUtils;

public class VehicleTest extends BaseTest {

    @Test
    public void TC_VEH_01_AddVehicleSuccessfully() {

        LoginPage loginPage = new LoginPage(driver);
        VehiclePage vehiclePage = new VehiclePage(driver);

        String baseUrl = ConfigReader.get("base.url");
        String username = ConfigReader.get("admin.username");
        String password = ConfigReader.get("admin.password");

        // Bước 1: Đăng nhập
        loginPage.openLoginPage(baseUrl);
        loginPage.login(username, password);

        // Bước 2: Mở trang Quản lý xe
        vehiclePage.openVehicleManagementPage(baseUrl);

        // Bước 3: Nhấn nút Thêm xe mới
        vehiclePage.clickAddVehicle();
        vehiclePage.enterTenXe("Toyota Vios VIP");
        vehiclePage.enterBienSo("74A-28297");
        vehiclePage.enterMauSac("Trắng");
        vehiclePage.enterNamSanXuat("2020");
        vehiclePage.selectHangXe("Toyota");
        vehiclePage.selectLoaiXe("Xe 4 chỗ");
        vehiclePage.selectHopSo("Tự động");
        vehiclePage.selectNhienLieu("Xăng");
        vehiclePage.selectTrangThai("Sẵn sàng (Cho thuê)");
        vehiclePage.enterGiaThue("1500000");
        vehiclePage.enterTienCoc("500000");
        vehiclePage.enterMoTa("Phù hợp cho gia đình du lịch vivu.");
        vehiclePage.uploadImage("C:\\Users\\DELL\\Pictures\\images.jfif");
        vehiclePage.clickSave();
        By successMessage = By.className("swal2-title");

        WebElement messageElement =
                WaitUtils.waitForVisible(driver, successMessage);

        String actualMessage = messageElement.getText();

        Assert.assertEquals(
                actualMessage,
                "Thêm xe mới thành công!",
                "Thông báo thêm xe thành công không đúng."
        );
    }
    @Test
    public void TC_VEH_02_AddVehicleWithoutName() {

        LoginPage loginPage = new LoginPage(driver);
        VehiclePage vehiclePage = new VehiclePage(driver);

        String baseUrl = ConfigReader.get("base.url");
        String username = ConfigReader.get("admin.username");
        String password = ConfigReader.get("admin.password");

        // Đăng nhập Admin
        loginPage.openLoginPage(baseUrl);
        loginPage.login(username, password);

        // Mở trang thêm xe
        vehiclePage.openVehicleManagementPage(baseUrl);
        vehiclePage.clickAddVehicle();

        // Cố ý không nhập tên xe
        vehiclePage.enterBienSo("74A-88158");

        vehiclePage.selectHangXe("Toyota");
        vehiclePage.selectLoaiXe("Xe 4 chỗ");

        vehiclePage.enterMauSac("Trắng");
        vehiclePage.enterNamSanXuat("2024");

        vehiclePage.selectHopSo("Tự động");
        vehiclePage.selectNhienLieu("Xăng");

        vehiclePage.enterGiaThue("1200000");
        vehiclePage.enterTienCoc("5000000");

        vehiclePage.selectTrangThai("Sẵn sàng (Cho thuê)");
        vehiclePage.enterMoTa("Kiểm tra bỏ trống tên xe.");

        // Nhấn lưu
        vehiclePage.clickSave();

        // Kiểm tra thông báo lỗi của trường Tên xe
        By tenXeError =
                By.cssSelector("span[data-valmsg-for='TenXe']");

        WebElement errorElement =
                WaitUtils.waitForVisible(driver, tenXeError);

        Assert.assertEquals(
                errorElement.getText().trim(),
                "Yêu cầu nhập tên xe",
                "Thông báo khi bỏ trống tên xe không đúng."
        );
    }
    @Test
    public void TC_VEH_06_AddVehicleWithoutBrand() {

        LoginPage loginPage = new LoginPage(driver);
        VehiclePage vehiclePage = new VehiclePage(driver);

        String baseUrl = ConfigReader.get("base.url");
        String username = ConfigReader.get("admin.username");
        String password = ConfigReader.get("admin.password");

        // Bước 1: Đăng nhập Admin
        loginPage.openLoginPage(baseUrl);
        loginPage.login(username, password);

        // Bước 2: Mở trực tiếp trang thêm xe
        vehiclePage.openAddVehiclePage(baseUrl);

        // Bước 3: Nhập dữ liệu hợp lệ
        vehiclePage.enterTenXe("Toyota T");
        vehiclePage.enterBienSo("74A-26262");

        // Cố ý không chọn hãng xe
        // vehiclePage.selectHangXe("Toyota");

        vehiclePage.selectLoaiXe("Xe 4 chỗ");
        vehiclePage.enterMauSac("Trắng");
        vehiclePage.enterNamSanXuat("2020");
        vehiclePage.selectHopSo("Tự động");
        vehiclePage.selectNhienLieu("Xăng");
        vehiclePage.enterGiaThue("1900000");
        vehiclePage.enterTienCoc("5000000");
        vehiclePage.selectTrangThai("Sẵn sàng (Cho thuê)");
        vehiclePage.enterMoTa("Kiểm tra không chọn hãng xe.");

        // Bước 4: Nhấn lưu
        vehiclePage.clickSave();

        // Bước 5: Kiểm tra thông báo lỗi hãng xe
        By hangXeError =
                By.cssSelector("span[data-valmsg-for='MaHang']");

        WebElement errorElement =
                WaitUtils.waitForVisible(driver, hangXeError);

        Assert.assertEquals(
                errorElement.getText().trim(),
                "Yêu cầu chọn hãng xe",
                "Thông báo khi không chọn hãng xe không đúng."
        );
    }
    @Test
    public void TC_VEH_11_UpdateVehicleSuccessfully() {

        LoginPage loginPage = new LoginPage(driver);
        VehiclePage vehiclePage = new VehiclePage(driver);

        String baseUrl = ConfigReader.get("base.url");
        String username = ConfigReader.get("admin.username");
        String password = ConfigReader.get("admin.password");

        // Đăng nhập
        loginPage.openLoginPage(baseUrl);
        loginPage.login(username, password);

        // Mở trang quản lý xe
        vehiclePage.openVehicleManagementPage(baseUrl);

        // Chọn sửa xe đầu tiên
        vehiclePage.clickEditFirstVehicle();

        // Đổi tên xe
        vehiclePage.clearAndEnterTenXe("Toyota Vios 3");

        // Lưu
        vehiclePage.clickSave();

        // Kiểm tra thông báo thành công
        By successMessage = By.className("swal2-title");

        WebElement message =
                WaitUtils.waitForVisible(driver, successMessage);

        Assert.assertEquals(
                message.getText(),
                "Cập nhật thông tin xe thành công!"
        );
    }
    @Test
    public void TC_VEH_15_DeleteVehicleSuccessfully() {

        LoginPage loginPage = new LoginPage(driver);
        VehiclePage vehiclePage = new VehiclePage(driver);

        String baseUrl = ConfigReader.get("base.url");
        String username = ConfigReader.get("admin.username");
        String password = ConfigReader.get("admin.password");

        // Đăng nhập
        loginPage.openLoginPage(baseUrl);
        loginPage.login(username, password);

        // Mở quản lý xe
        vehiclePage.openVehicleManagementPage(baseUrl);

        // Chọn xe đầu tiên để xóa
        vehiclePage.clickDeleteFirstVehicle();

        // Xác nhận xóa
        vehiclePage.clickConfirmDelete();

        // Kiểm tra thông báo
        By successMessage = By.className("swal2-title");

        WebElement message =
                WaitUtils.waitForVisible(driver, successMessage);

        Assert.assertEquals(
                message.getText(),
                "Xóa xe thành công!"
        );
    }
    @Test
    public void TC_VEH_16_CannotDeleteVehicleWithRentalOrder() {

        LoginPage loginPage = new LoginPage(driver);
        VehiclePage vehiclePage = new VehiclePage(driver);

        String baseUrl = ConfigReader.get("base.url");
        String username = ConfigReader.get("admin.username");
        String password = ConfigReader.get("admin.password");

        // Bước 1: Đăng nhập Admin
        loginPage.openLoginPage(baseUrl);
        loginPage.login(username, password);

        // Bước 2: Mở trang quản lý xe
        vehiclePage.openVehicleManagementPage(baseUrl);

        // Bước 3: Chọn xe đã từng có đơn thuê
        vehiclePage.clickDeleteVehicleByName("VinFast VF 5 Plus");

        // Bước 4: Xác nhận xóa
        vehiclePage.clickConfirmDelete();

        // Bước 5: Kiểm tra thông báo
        By title = By.className("swal2-title");
        By content = By.className("swal2-html-container");

        String actualTitle =
                WaitUtils.waitForVisible(driver, title).getText().trim();

        String actualContent =
                WaitUtils.waitForVisible(driver, content).getText().trim();

        Assert.assertEquals(
                actualTitle,
                "Không thể thực hiện"
        );

        Assert.assertEquals(
                actualContent,
                "Không thể xóa xe vì xe đã có đơn thuê trong hệ thống."
        );
    }
    @Test
    public void TC_VEH_11_ViewVehicleDetailsSuccessfully() {

        LoginPage loginPage = new LoginPage(driver);
        VehiclePage vehiclePage = new VehiclePage(driver);

        String baseUrl = ConfigReader.get("base.url");
        String username = ConfigReader.get("admin.username");
        String password = ConfigReader.get("admin.password");

        // Bước 1: Đăng nhập Admin
        loginPage.openLoginPage(baseUrl);
        loginPage.login(username, password);

        // Bước 2: Mở trang quản lý xe
        vehiclePage.openVehicleManagementPage(baseUrl);

        // Bước 3: Nhấn xem chi tiết xe đầu tiên
        vehiclePage.clickViewFirstVehicle();

        // Bước 4: Kiểm tra đã chuyển sang trang chi tiết
        Assert.assertTrue(
                driver.getCurrentUrl().contains("/Admin/Xe/Details"),
                "Hệ thống chưa chuyển đến trang chi tiết xe."
        );
    }
}