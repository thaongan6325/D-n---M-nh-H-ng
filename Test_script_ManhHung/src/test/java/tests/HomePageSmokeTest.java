package tests;

import base.BaseTest;
import org.testng.Assert;
import org.testng.annotations.Test;
import utils.ConfigReader;

public class HomePageSmokeTest extends BaseTest {

    @Test
    public void openHomePageSuccessfully() {
        driver.get(ConfigReader.get("base.url"));

        Assert.assertTrue(
                driver.getCurrentUrl().contains("localhost:5252"),
                "Website chưa mở đúng URL local."
        );

        Assert.assertFalse(
                driver.getTitle().isBlank(),
                "Tiêu đề trang không được để trống."
        );

        System.out.println("Đã mở website thành công.");
        System.out.println("URL: " + driver.getCurrentUrl());
        System.out.println("Title: " + driver.getTitle());
    }
}