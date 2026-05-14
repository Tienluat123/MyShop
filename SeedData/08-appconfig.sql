-- AppConfigs Seed Data
-- Người sửa: PhamVanMinhBinhThuan
-- Cập nhật lần cuối: 2026-05-14

-- Xóa dữ liệu cũ trước khi insert để đảm bảo idempotent (chạy lại nhiều lần vẫn đúng)
DELETE FROM "AppConfigs";

INSERT INTO "AppConfigs" ("Id", "SettingKey", "SettingValue", "Description") VALUES
(1, 'ShopName', 'MyShop', 'Tên cửa hàng'),
(2, 'Phone', '0123456789', 'Số điện thoại liên hệ'),
(3, 'Address', '123 Đường ABC, Quận XYZ, TP.HCM', 'Địa chỉ cửa hàng'),
(4, 'ShopCode', 'MS001', 'Mã cửa hàng'),
(5, 'ContactEmail', 'contact@myshop.com', 'Email liên hệ'),
(6, 'ItemsPerPage', '20', 'Số lượng mục hiển thị trên mỗi trang');

SELECT setval('"AppConfigs_Id_seq"', COALESCE((SELECT MAX("Id") FROM "AppConfigs"), 1), TRUE);
