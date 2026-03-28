-- ============================================
-- Coffee Shop Cafe - Seed Data (PostgreSQL)
-- ============================================
-- Run: psql -U nguyentienluat -d myshop < seed-data.sql

-- ===== CATEGORIES =====
INSERT INTO "Categories" ("Id", "Name", "IconGlyph", "Description") VALUES
(1, 'Coffee', '☕', 'Coffee drinks - Espresso, Latte, Cappuccino, etc.'),
(2, 'Tea', '🍵', 'Tea beverages - Green Tea, Black Tea, Jasmine, etc.'),
(3, 'Cake & Pastry', '🍰', 'Cakes, pastries, and desserts'),
(4, 'Snacks', '🥐', 'Sandwiches, bagels, and light snacks')
ON CONFLICT DO NOTHING;

-- ===== PRODUCTS =====
INSERT INTO "Products" ("Id", "Name", "Price", "CategoryId") VALUES
-- Coffee (CategoryId = 1)
(1, 'Espresso', 30000, 1),
(2, 'Latte', 45000, 1),
(3, 'Cappuccino', 50000, 1),
(4, 'Americano', 40000, 1),
(5, 'Macchiato', 55000, 1),
(6, 'Cold Brew', 48000, 1),
(7, 'Mocha', 52000, 1),
(8, 'Flat White', 48000, 1),

-- Tea (CategoryId = 2)
(9, 'Green Tea', 30000, 2),
(10, 'Black Tea', 32000, 2),
(11, 'Jasmine Tea', 35000, 2),
(12, 'Oolong Tea', 38000, 2),
(13, 'Iced Tea', 28000, 2),

-- Cake & Pastry (CategoryId = 3)
(14, 'Chocolate Cake', 85000, 3),
(15, 'Cheesecake', 95000, 3),
(16, 'Tiramisu', 75000, 3),
(17, 'Croissant', 45000, 3),
(18, 'Muffin Chocolate', 40000, 3),
(19, 'Donut', 35000, 3),

-- Snacks (CategoryId = 4)
(20, 'Sandwich Chicken', 65000, 4),
(21, 'Sandwich Tuna', 60000, 4),
(22, 'Bagel Cream Cheese', 55000, 4),
(23, 'Club Sandwich', 75000, 4)
ON CONFLICT DO NOTHING;

-- ===== CUSTOMERS =====
INSERT INTO "Customers" ("Id", "Name", "Phone", "Email", "Address", "Gender", "Status", "Points", "Tier") VALUES
(1, 'Nguyễn Văn A', '0912345678', 'nguyenvana@email.com', '123 Nguyễn Huệ, HCM', 'Male', 'Active', 100, 1),
(2, 'Trần Thị B', '0987654321', 'tranthib@email.com', '456 Lê Lợi, HCM', 'Female', 'Active', 200, 1),
(3, 'Phạm Văn C', '0901234567', 'phamvanc@email.com', '789 Tran Hung Dao, HCM', 'Male', 'Active', 500, 2),
(4, 'Lê Thị D', '0945678901', 'lethid@email.com', '321 Hoàng Văn Thụ, HCM', 'Female', 'Active', 150, 1),
(5, 'Hoàng Văn E', '0923456789', 'hoangvane@email.com', '654 Cư Xá Phú Lâm, HCM', 'Male', 'Active', 800, 3),
(6, 'Vũ Thị F', '0934567890', 'vuthif@email.com', '987 Khánh Hội, HCM', 'Female', 'Inactive', 50, 0)
ON CONFLICT DO NOTHING;

-- ===== USERS (Staff) =====
INSERT INTO "Users" ("Id", "Username", "FullName", "PasswordHash", "HourlyRate", "Role", "IsActive") VALUES
(1, 'manager', 'Trương Quản Lý', 'sha256_hashed_password_1', 50000, '1', true),
(2, 'barista1', 'Đoàn Tuấn Barista 1', 'sha256_hashed_password_2', 35000, '2', true),
(3, 'barista2', 'Ngô Minh Barista 2', 'sha256_hashed_password_3', 35000, '2', true),
(4, 'barista3', 'Lương Nhật Barista 3', 'sha256_hashed_password_4', 35000, '2', true),
(5, 'cashier', 'Võ Thu Ngân', 'sha256_hashed_password_5', 30000, '2', true)
ON CONFLICT DO NOTHING;

-- ===== ORDERS =====
INSERT INTO "Orders" ("Id", "CustomerId", "UserId", "OrderDate", "SubTotal", "DiscountAmount", "TotalAmount", "Status") VALUES
(1, 1, 2, NOW() - INTERVAL '5 days', 95000, 0, 95000, 'Paid'),
(2, 2, 3, NOW() - INTERVAL '3 days', 180000, 15000, 165000, 'Paid'),
(3, 3, 2, NOW() - INTERVAL '1 day', 290000, 0, 290000, 'Paid'),
(4, 4, 5, NOW(), 125000, 0, 125000, 'Paid'),
(5, 5, 2, NOW() - INTERVAL '2 hours', 180000, 30000, 150000, 'Paid')
ON CONFLICT DO NOTHING;

-- ===== ORDER DETAILS =====
INSERT INTO "OrderDetails" ("Id", "OrderId", "ProductId", "Quantity", "UnitPrice") VALUES
-- Order 1
(1, 1, 2, 2, 45000),
(2, 1, 14, 1, 85000),

-- Order 2
(3, 2, 3, 2, 50000),
(4, 2, 17, 2, 45000),

-- Order 3
(5, 3, 5, 1, 55000),
(6, 3, 20, 1, 65000),
(7, 3, 15, 1, 95000),

-- Order 4
(8, 4, 1, 1, 30000),
(9, 4, 21, 1, 60000),
(10, 4, 18, 1, 40000),

-- Order 5
(11, 5, 2, 2, 45000),
(12, 5, 9, 1, 30000),
(13, 5, 16, 1, 75000)
ON CONFLICT DO NOTHING;

-- ===== PROMOTIONS =====
INSERT INTO "Promotions" ("Id", "PromoCode", "PromoType", "DiscountValue", "MinOrderAmount", "StartDate", "EndDate", "TargetAudience", "IsActive") VALUES
(1, 'MORNING10', 'Percent', 10, 50000, NOW(), NOW() + INTERVAL '90 days', 'All', true),
(2, 'BUY5GET1', 'FixedAmount', 35000, 0, NOW(), NOW() + INTERVAL '60 days', 'All', true),
(3, 'WELCOME25', 'FixedAmount', 25000, 0, NOW(), NOW() + INTERVAL '120 days', 'All', true),
(4, 'VIP20', 'Percent', 20, 100000, NOW(), NOW() + INTERVAL '90 days', 'VIP', true),
(5, 'LUNCH50', 'FixedAmount', 50000, 150000, NOW(), NOW() + INTERVAL '30 days', 'All', true)
ON CONFLICT DO NOTHING;

-- ===== Reset Identity Sequences =====
SELECT setval('"Categories_Id_seq"', (SELECT MAX("Id") FROM "Categories") + 1);
SELECT setval('"Products_Id_seq"', (SELECT MAX("Id") FROM "Products") + 1);
SELECT setval('"Customers_Id_seq"', (SELECT MAX("Id") FROM "Customers") + 1);
SELECT setval('"Users_Id_seq"', (SELECT MAX("Id") FROM "Users") + 1);
SELECT setval('"Orders_Id_seq"', (SELECT MAX("Id") FROM "Orders") + 1);
SELECT setval('"OrderDetails_Id_seq"', (SELECT MAX("Id") FROM "OrderDetails") + 1);
SELECT setval('"Promotions_Id_seq"', (SELECT MAX("Id") FROM "Promotions") + 1);

-- ===== Verification =====
SELECT 'Categories:' AS type, COUNT(*) as count FROM "Categories"
UNION ALL
SELECT 'Products', COUNT(*) FROM "Products"
UNION ALL
SELECT 'Customers', COUNT(*) FROM "Customers"
UNION ALL
SELECT 'Users', COUNT(*) FROM "Users"
UNION ALL
SELECT 'Orders', COUNT(*) FROM "Orders"
UNION ALL
SELECT 'OrderDetails', COUNT(*) FROM "OrderDetails"
UNION ALL
SELECT 'Promotions', COUNT(*) FROM "Promotions";
