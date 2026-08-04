package pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import utils.WaitUtils;

public class LoginPage {

    private final WebDriver driver;

    private final By usernameInput = By.name("TaiKhoan");
    private final By passwordInput = By.name("MatKhau");
    private final By loginButton = By.cssSelector("button.btn-auth");

    public LoginPage(WebDriver driver) {
        this.driver = driver;
    }

    public void openLoginPage(String baseUrl) {
        driver.get(baseUrl + "/Account/Login");
        WaitUtils.waitForPageLoaded(driver);
    }

    public void enterUsername(String username) {
        WaitUtils.waitForVisible(driver, usernameInput).clear();
        driver.findElement(usernameInput).sendKeys(username);
    }

    public void enterPassword(String password) {
        WaitUtils.waitForVisible(driver, passwordInput).clear();
        driver.findElement(passwordInput).sendKeys(password);
    }

    public void clickLogin() {
        WaitUtils.waitForClickable(driver, loginButton).click();
    }

    public void login(String username, String password) {
        enterUsername(username);
        enterPassword(password);
        clickLogin();

        WaitUtils.waitForUrlContains(driver, "/Admin");
    }
}