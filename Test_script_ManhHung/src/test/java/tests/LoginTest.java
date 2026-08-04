package tests;

import base.BaseTest;
import org.testng.Assert;
import org.testng.annotations.Test;
import pages.LoginPage;
import utils.ConfigReader;
import utils.WaitUtils;

public class LoginTest extends BaseTest {

    @Test
    public void loginAdminSuccessfully() {

        LoginPage loginPage = new LoginPage(driver);

        String baseUrl = ConfigReader.get("base.url");
        String username = ConfigReader.get("admin.username");
        String password = ConfigReader.get("admin.password");

        loginPage.openLoginPage(baseUrl);
        loginPage.login(username, password);

        boolean loginSuccess =
                WaitUtils.waitForUrlContains(driver, "/Admin");

        Assert.assertTrue(
                loginSuccess,
                "Đăng nhập admin không thành công."
        );
    }
}