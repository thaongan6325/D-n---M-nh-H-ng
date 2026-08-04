package tests;

import base.BaseTest;
import org.testng.Assert;
import org.testng.annotations.Test;
import pages.LoginPage;
import pages.OrderPage;
import utils.ConfigReader;

public class OrderTest extends BaseTest {

    @Test
    public void TC_ORD_14_DeliverVehicleSuccessfully() {

        LoginPage loginPage = new LoginPage(driver);
        OrderPage orderPage = new OrderPage(driver);

        String baseUrl = ConfigReader.get("base.url");
        String username = ConfigReader.get("admin.username");
        String password = ConfigReader.get("admin.password");

        // Bước 1: Đăng nhập Admin
        loginPage.openLoginPage(baseUrl);
        loginPage.login(username, password);

        // Bước 2: Mở trang quản lý đơn thuê
        orderPage.openOrderManagementPage(baseUrl);

        // Bước 3: Chọn đơn có trạng thái Đã đặt cọc
        orderPage.clickProcessFirstDepositOrder();

        // Bước 4: Nhấn Giao xe ngay
        orderPage.clickDeliverVehicleNow();

        // Bước 5: Kiểm tra alert xác nhận thanh toán và nhấn OK
        String confirmAlertText =
                orderPage.getAndAcceptBrowserAlert();

        Assert.assertTrue(
                confirmAlertText.contains("XÁC NHẬN THANH TOÁN"),
                "Alert không hiển thị nội dung xác nhận thanh toán."
        );

        Assert.assertTrue(
                confirmAlertText.contains("Xác nhận GIAO XE"),
                "Alert không hiển thị nội dung xác nhận giao xe."
        );

        // Bước 6: Kiểm tra alert thông báo giao xe thành công và nhấn OK
        String successAlertText =
                orderPage.getAndAcceptBrowserAlert();

        Assert.assertTrue(
                successAlertText.contains(
                        "Giao xe thành công! Đã ghi nhận thanh toán."
                ),
                "Thông báo giao xe thành công không đúng. Thực tế: "
                        + successAlertText
        );

        // Bước 7: Chờ quay lại trang danh sách đơn
        orderPage.waitForOrderManagementPage();

        // Bước 8: Kiểm tra đơn đã chuyển sang trạng thái Đang đi
        Assert.assertTrue(
                orderPage.hasGoingStatus(),
                "Đơn chưa chuyển sang trạng thái Đang đi."
        );
    }
    @Test
    public void TC_ORD_15_CancelDepositOrderSuccessfully() {

        LoginPage loginPage = new LoginPage(driver);
        OrderPage orderPage = new OrderPage(driver);

        String baseUrl = ConfigReader.get("base.url");
        String username = ConfigReader.get("admin.username");
        String password = ConfigReader.get("admin.password");

        String cancellationReason =
                "Khách hàng yêu cầu hủy đơn thuê xe";

        // Bước 1: Đăng nhập Admin
        loginPage.openLoginPage(baseUrl);
        loginPage.login(username, password);

        // Bước 2: Mở trang quản lý đơn thuê
        orderPage.openOrderManagementPage(baseUrl);

        // Bước 3: Chọn đơn có trạng thái Đã đặt cọc
        orderPage.clickProcessFirstDepositOrder();

        // Bước 4: Nhấn Hủy đơn
        orderPage.clickCancelOrder();

        // Bước 5: Nhập lý do vào JavaScript Prompt
        String promptText =
                orderPage.enterReasonAndAcceptPrompt(
                        cancellationReason
                );

        Assert.assertTrue(
                promptText.contains("Lý do hủy đơn"),
                "Prompt nhập lý do hủy không đúng. Thực tế: "
                        + promptText
        );

        // Bước 6: Xử lý alert thông báo hủy thành công
        String successAlert =
                orderPage.getAndAcceptBrowserAlert();

        Assert.assertTrue(
                successAlert.contains(
                        "Đã hủy đơn hàng và ghi nhận hoàn tiền thành công"
                ),
                "Thông báo hủy đơn không đúng. Thực tế: "
                        + successAlert
        );

        // Bước 7: Chờ quay lại danh sách đơn
        orderPage.waitForOrderManagementPage();

        // Bước 8: Kiểm tra trạng thái Đã hủy
        Assert.assertTrue(
                orderPage.hasCancelledStatus(),
                "Đơn chưa chuyển sang trạng thái Đã hủy."
        );
    }
    @Test
    public void TC_ORD_16_ReturnVehicleSuccessfully() {

        LoginPage loginPage = new LoginPage(driver);
        OrderPage orderPage = new OrderPage(driver);

        String baseUrl = ConfigReader.get("base.url");
        String username = ConfigReader.get("admin.username");
        String password = ConfigReader.get("admin.password");

        // Bước 1: Đăng nhập Admin
        loginPage.openLoginPage(baseUrl);
        loginPage.login(username, password);

        // Bước 2: Mở trang quản lý đơn thuê
        orderPage.openOrderManagementPage(baseUrl);

        // Bước 3: Chọn đơn có trạng thái Đang đi
        orderPage.clickProcessFirstGoingOrder();

        // Bước 4: Nhấn Trả xe
        orderPage.clickReturnVehicle();

        // Bước 5: Xử lý alert xác nhận trả xe
        String confirmAlert =
                orderPage.getAndAcceptBrowserAlert();

        Assert.assertTrue(
                confirmAlert.toLowerCase().contains("trả xe"),
                "Alert xác nhận trả xe không đúng. Thực tế: "
                        + confirmAlert
        );

        // Bước 6: Xử lý alert trả xe thành công
        String successAlert =
                orderPage.getAndAcceptBrowserAlert();

        Assert.assertTrue(
                successAlert.toLowerCase().contains("trả xe")
                        && successAlert.toLowerCase().contains("thành công"),
                "Thông báo trả xe thành công không đúng. Thực tế: "
                        + successAlert
        );

        // Bước 7: Chờ quay lại danh sách quản lý đơn
        orderPage.waitForOrderManagementPage();

        // Bước 8: Kiểm tra trạng thái Hoàn thành
        Assert.assertTrue(
                orderPage.hasCompletedStatus(),
                "Đơn chưa chuyển sang trạng thái Hoàn thành."
        );
    }
}