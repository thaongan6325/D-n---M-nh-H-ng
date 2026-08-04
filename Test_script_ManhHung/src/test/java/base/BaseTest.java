package base;

import org.openqa.selenium.PageLoadStrategy;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.chrome.ChromeDriver;
import org.openqa.selenium.chrome.ChromeOptions;
import org.testng.annotations.AfterMethod;
import org.testng.annotations.BeforeMethod;
import utils.ConfigReader;

import java.time.Duration;

public abstract class BaseTest {

    protected WebDriver driver;

    @BeforeMethod
    public void setUp() {
        String browser = ConfigReader.get("browser");

        if (!"chrome".equalsIgnoreCase(browser)) {
            throw new IllegalArgumentException(
                    "Project hiện chỉ hỗ trợ Chrome. Browser nhận được: "
                            + browser
            );
        }

        // Khởi tạo cấu hình Chrome
        ChromeOptions options = new ChromeOptions();

        // Không chờ toàn bộ ảnh và tài nguyên phụ tải xong
        options.setPageLoadStrategy(PageLoadStrategy.EAGER);

        // Cho phép truy cập localhost dùng HTTPS chưa có chứng chỉ hợp lệ
        options.setAcceptInsecureCerts(true);

        // Mở Chrome toàn màn hình
        options.addArguments("--start-maximized");

        // Chạy trình duyệt ẩn nếu config headless=true
        if (Boolean.parseBoolean(ConfigReader.get("headless"))) {
            options.addArguments("--headless=new");
            options.addArguments("--window-size=1920,1080");
        }

        // Chỉ tạo ChromeDriver một lần sau khi cấu hình xong
        driver = new ChromeDriver(options);

        // Thời gian tối đa chờ tải trang
        driver.manage()
                .timeouts()
                .pageLoadTimeout(Duration.ofSeconds(30));

        // Không dùng implicit wait vì project đang dùng WaitUtils
        driver.manage()
                .timeouts()
                .implicitlyWait(Duration.ofSeconds(0));
    }

    @AfterMethod(alwaysRun = true)
    public void tearDown() {
        if (driver != null) {
            driver.quit();
        }
    }
}