package tests;

import org.testng.Assert;
import org.testng.annotations.Test;

public class SetupTest {

    @Test
    public void verifyTestNgSetup() {
        Assert.assertTrue(true);
        System.out.println("TestNG đã được cấu hình thành công.");
    }
}