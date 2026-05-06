-- AppConfigs Seed Data
-- Người sửa: PhamVanMinhBinhThuan
-- Cập nhật lần cuối: 2026-04-09

-- Xóa dữ liệu cũ trước khi insert để đảm bảo idempotent (chạy lại nhiều lần vẫn đúng)
DELETE FROM "AppConfigs";

INSERT INTO "AppConfigs" ("SettingKey", "SettingValue", "Description") VALUES
('ShopName', 'MyShop', 'Tên cửa hàng'),
('Phone', '0123456789', 'Số điện thoại liên hệ'),
('Address', '123 Đường ABC,Quận XYZ, TP.HCM', 'Địa chỉ cửa hàng'),
('ShopCode', 'MS001', 'Mã cửa hàng'),
('ContactEmail', 'contact@myshop.com', 'Email liên hệ'),
('ItemsPerPage', '20', 'Số lượng mục hiển thị trên mỗi trang');
