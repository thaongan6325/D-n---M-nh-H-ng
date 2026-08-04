package utils;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;

import java.time.Duration;

public final class WaitUtils {

    private static final Duration DEFAULT_TIMEOUT = Duration.ofSeconds(15);

    private WaitUtils() {
        // Không cho tạo đối tượng từ class tiện ích này.
    }

    public static WebElement waitForVisible(WebDriver driver, By locator) {
        WebDriverWait wait = new WebDriverWait(driver, DEFAULT_TIMEOUT);

        return wait.until(
                ExpectedConditions.visibilityOfElementLocated(locator)
        );
    }

    public static WebElement waitForClickable(WebDriver driver, By locator) {
        WebDriverWait wait = new WebDriverWait(driver, DEFAULT_TIMEOUT);

        return wait.until(
                ExpectedConditions.elementToBeClickable(locator)
        );
    }

    public static boolean waitForUrlContains(WebDriver driver, String value) {
        WebDriverWait wait = new WebDriverWait(driver, DEFAULT_TIMEOUT);

        return wait.until(
                ExpectedConditions.urlContains(value)
        );
    }

    public static boolean waitForTextVisible(
            WebDriver driver,
            By locator,
            String expectedText
    ) {
        WebDriverWait wait = new WebDriverWait(driver, DEFAULT_TIMEOUT);

        return wait.until(
                ExpectedConditions.textToBePresentInElementLocated(
                        locator,
                        expectedText
                )
        );
    }

    public static void waitForPageLoaded(WebDriver driver) {
        WebDriverWait wait = new WebDriverWait(driver, DEFAULT_TIMEOUT);

        wait.until(webDriver ->
                "complete".equals(
                        ((org.openqa.selenium.JavascriptExecutor) webDriver)
                                .executeScript("return document.readyState")
                )
        );
    }
}