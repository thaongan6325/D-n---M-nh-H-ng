package pages;

import org.openqa.selenium.Alert;
import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;
import utils.WaitUtils;

import java.time.Duration;

public class OrderPage {

    private final WebDriver driver;

    // Nút Xử lý của đơn có trạng thái Đã đặt cọc
    private final By processDepositOrderButton = By.xpath(
            "//tr[.//*[contains(normalize-space(),'Đã đặt cọc')]]" +
                    "//*[self::a or self::button][contains(normalize-space(),'Xử lý')]"
    );

    // Nút Giao xe ngay
    private final By deliverVehicleButton = By.xpath(
            "//*[self::a or self::button]" +
                    "[contains(normalize-space(),'Giao xe ngay')]"
    );

    // Nút Hủy đơn
    private final By cancelOrderButton = By.xpath(
            "//*[self::a or self::button]" +
                    "[contains(normalize-space(),'Hủy đơn')]"
    );

    // Trạng thái Đang đi
    private final By goingStatus = By.xpath(
            "//*[contains(normalize-space(),'Đang đi')]"
    );

    // Trạng thái Đã hủy
    private final By cancelledStatus = By.xpath(
            "//*[contains(normalize-space(),'Đã hủy')]"
    );
    // Nút Xử lý của đơn có trạng thái Đang đi
    private final By processGoingOrderButton = By.xpath(
            "//tr[.//*[contains(normalize-space(),'Đang đi')]]" +
                    "//*[self::a or self::button][contains(normalize-space(),'Xử lý')]"
    );

    // Nút Trả xe
    private final By returnVehicleButton = By.xpath(
            "//*[self::a or self::button]" +
                    "[contains(normalize-space(),'Trả xe')]"
    );

    // Trạng thái Hoàn thành
    private final By completedStatus = By.xpath(
            "//*[contains(normalize-space(),'Hoàn thành')]"
    );

    public OrderPage(WebDriver driver) {
        this.driver = driver;
    }

    /**
     * Mở trang quản lý đơn thuê xe.
     */
    public void openOrderManagementPage(String baseUrl) {

        driver.get(baseUrl + "/Admin/DonDatXe");

        WaitUtils.waitForUrlContains(
                driver,
                "/Admin/DonDatXe"
        );

        WaitUtils.waitForPageLoaded(driver);
    }

    /**
     * Chọn nút Xử lý của đơn đầu tiên có trạng thái Đã đặt cọc.
     */
    public void clickProcessFirstDepositOrder() {

        WaitUtils.waitForClickable(
                driver,
                processDepositOrderButton
        ).click();
    }

    /**
     * Nhấn nút Giao xe ngay.
     */
    public void clickDeliverVehicleNow() {

        WaitUtils.waitForClickable(
                driver,
                deliverVehicleButton
        ).click();
    }

    /**
     * Nhấn nút Hủy đơn.
     */
    public void clickCancelOrder() {

        WaitUtils.waitForClickable(
                driver,
                cancelOrderButton
        ).click();
    }

    /**
     * Chờ browser alert xuất hiện, lấy nội dung và nhấn OK.
     */
    public String getAndAcceptBrowserAlert() {

        WebDriverWait wait =
                new WebDriverWait(driver, Duration.ofSeconds(15));

        Alert alert = wait.until(
                ExpectedConditions.alertIsPresent()
        );

        String alertText = alert.getText().trim();

        alert.accept();

        return alertText;
    }

    /**
     * Chờ browser prompt xuất hiện,
     * nhập lý do và nhấn OK.
     */
    public String enterReasonAndAcceptPrompt(String reason) {

        WebDriverWait wait =
                new WebDriverWait(driver, Duration.ofSeconds(15));

        Alert prompt = wait.until(
                ExpectedConditions.alertIsPresent()
        );

        String promptText = prompt.getText().trim();

        prompt.sendKeys(reason);
        prompt.accept();

        return promptText;
    }

    /**
     * Chờ quay lại trang quản lý đơn thuê.
     */
    public void waitForOrderManagementPage() {

        WaitUtils.waitForUrlContains(
                driver,
                "/Admin/DonDatXe"
        );

        WaitUtils.waitForPageLoaded(driver);
    }

    /**
     * Kiểm tra danh sách có trạng thái Đang đi.
     */
    public boolean hasGoingStatus() {

        WebElement statusElement =
                WaitUtils.waitForVisible(driver, goingStatus);

        return statusElement.isDisplayed();
    }

    /**
     * Kiểm tra danh sách có trạng thái Đã hủy.
     */
    public boolean hasCancelledStatus() {

        WebElement statusElement =
                WaitUtils.waitForVisible(driver, cancelledStatus);

        return statusElement.isDisplayed();
    }
    /**
     * Chọn nút Xử lý của đơn đầu tiên có trạng thái Đang đi.
     */
    public void clickProcessFirstGoingOrder() {

        WebDriverWait wait =
                new WebDriverWait(driver, Duration.ofSeconds(15));

        try {
            WebElement processButton = wait.until(
                    ExpectedConditions.elementToBeClickable(
                            processGoingOrderButton
                    )
            );

            processButton.click();

        } catch (Exception e) {
            throw new AssertionError(
                    "Không tìm thấy đơn có trạng thái 'Đang đi'. "
                            + "Hãy giao xe thành công trước khi chạy test trả xe.",
                    e
            );
        }
    }

    /**
     * Nhấn nút Trả xe.
     */
    public void clickReturnVehicle() {

        WaitUtils.waitForClickable(
                driver,
                returnVehicleButton
        ).click();
    }

    /**
     * Kiểm tra danh sách có trạng thái Hoàn thành.
     */
    public boolean hasCompletedStatus() {

        WebElement statusElement =
                WaitUtils.waitForVisible(driver, completedStatus);

        return statusElement.isDisplayed();
    }
}