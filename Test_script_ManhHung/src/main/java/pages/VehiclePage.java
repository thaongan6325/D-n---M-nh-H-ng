package pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.support.ui.Select;
import utils.WaitUtils;
import org.openqa.selenium.WebElement;

public class VehiclePage {

    private final WebDriver driver;

    // ===================== Locator =====================

    // Nút Thêm xe mới
    private final By btnAddVehicle =
            By.cssSelector("a[href*='Create']");


    // Form thêm xe
    private final By txtTenXe = By.name("TenXe");
    private final By txtBienSo = By.name("BienSo");
    private final By cboHangXe = By.name("MaHang");
    private final By cboLoaiXe = By.name("MaLoai");
    private final By txtMauSac = By.name("MauSac");
    private final By txtNamSanXuat = By.name("NamSanXuat");
    private final By cboHopSo = By.name("HopSo");
    private final By cboNhienLieu = By.name("NhienLieu");
    private final By txtGiaThue = By.name("GiaThueNgay");
    private final By txtTienCoc = By.name("TienCoc");
    private final By cboTrangThai = By.name("TrangThai");
    private final By txtMoTa = By.name("MoTa");
    private final By uploadImage = By.name("uploadHinhAnhs");
    private final By btnEditFirstVehicle =
            By.cssSelector("a[title='Sửa']");
    private final By btnDeleteFirstVehicle =
            By.cssSelector("a[title='Xóa']");
    private final By btnConfirmDelete =
            By.cssSelector("button[type='submit']");

    // Nút Lưu
    private final By btnSave =
            By.cssSelector("button[type='submit']");
    private final By btnViewFirstVehicle =
            By.cssSelector("a[title='Chi tiết']");

    // ===================== Constructor =====================

    public VehiclePage(WebDriver driver) {
        this.driver = driver;
    }

    // ===================== Methods =====================

    // Mở trang quản lý xe
    public void openVehicleManagementPage(String baseUrl) {
        try {
            driver.get(baseUrl + "/Admin/Xe");
        } catch (org.openqa.selenium.TimeoutException e) {
            // Chrome đã mở trang nhưng renderer phản hồi chậm
            driver.navigate().refresh();
        }

        WaitUtils.waitForUrlContains(driver, "/Admin/Xe");
    }

    // Mở trực tiếp trang thêm xe
    public void openAddVehiclePage(String baseUrl) {
        driver.get(baseUrl + "/Admin/Xe/Create");
        WaitUtils.waitForVisible(driver, txtTenXe);
    }

    // Nhấn nút Thêm xe mới
    public void clickAddVehicle() {
        WaitUtils.waitForClickable(driver, btnAddVehicle).click();
    }
    public void clickEditFirstVehicle() {
        WaitUtils.waitForClickable(driver, btnEditFirstVehicle).click();
    }

    public void clearAndEnterTenXe(String tenXe) {
        var element = WaitUtils.waitForVisible(driver, txtTenXe);
        element.clear();
        element.sendKeys(tenXe);
    }

    // ===================== Nhập dữ liệu =====================

    public void enterTenXe(String tenXe) {
        WaitUtils.waitForVisible(driver, txtTenXe).sendKeys(tenXe);
    }

    public void enterBienSo(String bienSo) {
        WaitUtils.waitForVisible(driver, txtBienSo).sendKeys(bienSo);
    }

    public void enterMauSac(String mauSac) {
        WaitUtils.waitForVisible(driver, txtMauSac).sendKeys(mauSac);
    }

    public void enterNamSanXuat(String nam) {
        WaitUtils.waitForVisible(driver, txtNamSanXuat).sendKeys(nam);
    }

    public void enterGiaThue(String giaThue) {
        WaitUtils.waitForVisible(driver, txtGiaThue).sendKeys(giaThue);
    }

    public void enterTienCoc(String tienCoc) {
        WaitUtils.waitForVisible(driver, txtTienCoc).sendKeys(tienCoc);
    }

    public void enterMoTa(String moTa) {
        WaitUtils.waitForVisible(driver, txtMoTa).sendKeys(moTa);
    }

    // ===================== Chọn dropdown =====================

    public void selectHangXe(String hangXe) {
        Select select = new Select(
                WaitUtils.waitForVisible(driver, cboHangXe)
        );

        select.selectByVisibleText(hangXe);
    }

    public void selectLoaiXe(String loaiXe) {
        Select select = new Select(
                WaitUtils.waitForVisible(driver, cboLoaiXe)
        );

        select.selectByVisibleText(loaiXe);
    }

    public void selectHopSo(String hopSo) {
        Select select = new Select(
                WaitUtils.waitForVisible(driver, cboHopSo)
        );

        select.selectByVisibleText(hopSo);
    }

    public void selectNhienLieu(String nhienLieu) {
        Select select = new Select(
                WaitUtils.waitForVisible(driver, cboNhienLieu)
        );

        select.selectByVisibleText(nhienLieu);
    }

    public void selectTrangThai(String trangThai) {
        Select select = new Select(
                WaitUtils.waitForVisible(driver, cboTrangThai)
        );

        select.selectByVisibleText(trangThai);
    }

    // ===================== Upload và lưu =====================

    public void uploadImage(String imagePath) {
        WaitUtils.waitForVisible(driver, uploadImage).sendKeys(imagePath);
    }

    public void clickSave() {
        WaitUtils.waitForClickable(driver, btnSave).click();
    }
    public void clickDeleteFirstVehicle() {
        WaitUtils.waitForClickable(driver, btnDeleteFirstVehicle).click();
    }
    public void clickConfirmDelete() {
        WaitUtils.waitForClickable(driver, btnConfirmDelete).click();
    }
    public void clickDeleteVehicleByName(String vehicleName) {

        By deleteButton = By.xpath(
                "//tr[.//td[contains(normalize-space(), '" +
                        vehicleName +
                        "')]]//a[@title='Xóa']"
        );

        WaitUtils.waitForClickable(driver, deleteButton).click();
    }
    public void clickViewFirstVehicle() {
        WaitUtils.waitForClickable(driver, btnViewFirstVehicle).click();
    }
}