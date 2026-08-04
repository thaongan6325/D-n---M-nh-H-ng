package utils;

import java.io.IOException;
import java.io.InputStream;
import java.util.Properties;

public final class ConfigReader {

    private static final Properties PROPERTIES = new Properties();

    static {
        try (InputStream inputStream = ConfigReader.class
                .getClassLoader()
                .getResourceAsStream("config.properties")) {

            if (inputStream == null) {
                throw new IllegalStateException(
                        "Không tìm thấy file config.properties trong src/test/resources."
                );
            }

            PROPERTIES.load(inputStream);

        } catch (IOException exception) {
            throw new ExceptionInInitializerError(
                    "Không thể đọc config.properties: " + exception.getMessage()
            );
        }
    }

    private ConfigReader() {
    }

    public static String get(String key) {
        String value = PROPERTIES.getProperty(key);

        if (value == null || value.isBlank()) {
            throw new IllegalArgumentException(
                    "Chưa cấu hình giá trị cho khóa: " + key
            );
        }

        return value.trim();
    }

    public static boolean getBoolean(String key) {
        return Boolean.parseBoolean(get(key));
    }
}