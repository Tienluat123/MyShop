-- ============================================================================
-- MYSHOP COMPLETE SEED DATA
-- Kết hợp tất cả SeedData từ SeedData/ folder
-- Last updated: April 18, 2026
-- ============================================================================

-- ===== 01-CATEGORIES =====
INSERT INTO "Categories"
    ("Id","Name","HasCustomization","IconKey","Description")
VALUES
    (1,'Coffee',TRUE,'IconCoffee','Coffee drinks'),
    (2,'Tea',TRUE,'IconTea','Tea beverages'),
    (3,'Cake & Pastry',FALSE,'IconPastries','Cakes and pastries'),
    (4,'Snacks',FALSE,'IconSmoothies','Sandwiches and snacks')
ON CONFLICT ("Id") DO UPDATE
SET
    "Name" = EXCLUDED."Name",
    "HasCustomization" = EXCLUDED."HasCustomization",
    "IconKey" = EXCLUDED."IconKey",
    "Description" = EXCLUDED."Description";

-- ===== 02-PRODUCTS =====
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

SELECT setval('"Products_Id_seq"', (SELECT MAX("Id") FROM "Products") + 1);

-- ===== 03-CUSTOMERS =====
INSERT INTO "Customers"
    ("Id", "Name", "Phone", "Email", "Address", "Gender", "Status", "TotalAccumulatedPoints", "CurrentAvailablePoints", "Tier", "TotalVisits", "TotalSpent", "CreatedAt")
VALUES
    (1, 'Nguyễn Văn A', '0912345678', 'nguyenvana@email.com', '123 Nguyễn Huệ, HCM', 'Male', 'Active', 100, 100, 1, 8, 250000, NOW()),
    (2, 'Trần Thị B', '0987654321', 'tranthib@email.com', '456 Lê Lợi, HCM', 'Female', 'Active', 200, 200, 1, 15, 450000, NOW()),
    (3, 'Phạm Văn C', '0901234567', 'phamvanc@email.com', '789 Tran Hung Dao, HCM', 'Male', 'Active', 500, 500, 2, 25, 950000, NOW()),
    (4, 'Lê Thị D', '0945678901', 'lethid@email.com', '321 Hoàng Văn Thụ, HCM', 'Female', 'Active', 150, 150, 1, 10, 350000, NOW()),
    (5, 'Hoàng Văn E', '0923456789', 'hoangvane@email.com', '654 Cư Xá Phú Lâm, HCM', 'Male', 'Active', 800, 800, 3, 50, 1500000, NOW()),
    (6, 'Vũ Thị F', '0934567890', 'vuthif@email.com', '987 Khánh Hội, HCM', 'Female', 'Inactive', 50, 50, 0, 3, 95000, NOW())
ON CONFLICT DO NOTHING;

SELECT setval('"Customers_Id_seq"', (SELECT MAX("Id") FROM "Customers") + 1);

-- ===== 04-USERS =====
INSERT INTO "Users"
    ("Id", "Username", "FirstName", "LastName", "Phone", "PasswordHash", "HourlyRate", "Role", "IsActive")
VALUES
    (1, 'admin_luat', 'Nguyễn Tiến', 'Luật', '0912345001', 'hash_pw_1', 100000, '1', true),
    (2, 'manager_minh', 'Phạm Văn', 'Minh', '0912345002', 'hash_pw_2', 80000, '1', true),
    (3, 'barista_tuan', 'Trần Anh', 'Tuấn', '0912345003', 'hash_pw_3', 35000, '2', true),
    (4, 'barista_hoa', 'Lê Thị', 'Hoa', '0912345004', 'hash_pw_4', 35000, '2', true),
    (5, 'cashier_ngan', 'Hoàng Thu', 'Ngân', '0912345005', 'hash_pw_5', 30000, '2', true),
    (6, 'staff_duy', 'Ngô Quang', 'Duy', '0912345006', 'hash_pw_6', 25000, '2', true),
    (7, 'staff_vy', 'Đặng Thúy', 'Vy', '0912345007', 'hash_pw_7', 25000, '2', true),
    (8, 'barista_lam', 'Võ Tùng', 'Lâm', '0912345008', 'hash_pw_8', 35000, '2', true),
    (9, 'cashier_linh', 'Mai Phương', 'Linh', '0912345009', 'hash_pw_9', 30000, '2', false),
    (10, 'staff_nam', 'Bùi Tiến', 'Nam', '0912345010', 'hash_pw_10', 25000, '2', true),
    (11, 'staff_huyen', 'Phan Thị', 'Huyền', '0912345011', 'hash_pw_11', 25000, '2', true),
    (12, 'barista_khoa', 'Đỗ Văn', 'Khoa', '0912345012', 'hash_pw_12', 35000, '2', true),
    (13, 'cashier_hang', 'Trương Thị', 'Hằng', '0912345013', 'hash_pw_13', 30000, '2', true),
    (14, 'staff_quang', 'Lý Quang', 'Quang', '0912345014', 'hash_pw_14', 25000, '2', true),
    (15, 'staff_thao', 'Phạm Thị', 'Thảo', '0912345015', 'hash_pw_15', 25000, '2', true),
    (16, 'barista_hieu', 'Nguyễn Hữu', 'Hiếu', '0912345016', 'hash_pw_16', 35000, '2', true),
    (17, 'cashier_hoa', 'Lê Thị', 'Hoa', '0912345017', 'hash_pw_17', 30000, '2', true),
    (18, 'staff_khoa', 'Đỗ Văn', 'Khoa', '0912345018', 'hash_pw_18', 25000, '2', true),
    (19, 'staff_hang', 'Trương Thị', 'Hằng', '0912345019', 'hash_pw_19', 25000, '2', true),
    (20, 'staff_quang2', 'Lý Quang', 'Quang2', '0912345020', 'hash_pw_20', 25000, '2', true),
    (21, 'staff_thao2', 'Phạm Thị', 'Thảo2', '0912345021', 'hash_pw_21', 25000, '2', true),
    (22, 'barista_hieu2', 'Nguyễn Hữu', 'Hiếu2', '0912345022', 'hash_pw_22', 35000, '2', true),
    (23, 'cashier_hoa2', 'Lê Thị', 'Hoa2', '0912345023', 'hash_pw_23', 30000, '2', true),
    (24, 'staff_khoa2', 'Đỗ Văn', 'Khoa2', '0912345024', 'hash_pw_24', 25000, '2', true),
    (25, 'staff_hang2', 'Trương Thị', 'Hằng2', '0912345025', 'hash_pw_25', 25000, '2', true)
ON CONFLICT ("Id") DO UPDATE SET
    "Username" = EXCLUDED."Username",
    "FirstName" = EXCLUDED."FirstName",
    "LastName" = EXCLUDED."LastName",
    "Phone" = EXCLUDED."Phone",
    "HourlyRate" = EXCLUDED."HourlyRate",
    "Role" = EXCLUDED."Role",
    "IsActive" = EXCLUDED."IsActive";

SELECT setval('"Users_Id_seq"', (SELECT MAX("Id") FROM "Users") + 1);

-- ===== 05-ORDERS & ORDER DETAILS =====
INSERT INTO "Orders" ("Id", "CustomerId", "UserId", "OrderDate", "SubTotal", "DiscountAmount", "TotalAmount", "PaymentStatus", "FulfillmentStatus", "Type", "TableNumber", "Notes", "PromotionId", "TaxAmount", "ManualAdjustment", "ManualAdjustmentReason") VALUES
(1, 1, 2, NOW() - INTERVAL '5 days', 95000, 0, 104500, 'Paid', 'Completed', 'Dine-in', 'Table 4', '', NULL, 9500, 0, ''),
(2, 2, 3, NOW() - INTERVAL '3 days', 180000, 15000, 181500, 'Paid', 'Completed', 'Dine-in', 'Table 2', 'Extra sugar', NULL, 16500, 0, ''),
(3, 3, 2, NOW() - INTERVAL '1 day', 290000, 0, 319000, 'Paid', 'Completed', 'Takeaway', '', 'No plastic bag', NULL, 29000, 0, ''),
(4, 4, 5, NOW(), 125000, 0, 137500, 'Paid', 'Completed', 'Dine-in', 'Table 8', '', NULL, 12500, 0, ''),
(5, 5, 2, NOW() - INTERVAL '2 hours', 180000, 30000, 165000, 'Paid', 'Completed', 'Dine-in', 'Table 5', 'VIP Guest', NULL, 15000, 0, ''),
(6, 1, 2, NOW() - INTERVAL '10 minutes', 45000, 0, 49500, 'PendingPayment', 'None', 'Dine-in', 'Table 1', '', NULL, 4500, 0, ''),
(7, 2, 2, NOW() - INTERVAL '15 minutes', 55000, 0, 60500, 'PendingPayment', 'None', 'Dine-in', 'Table 3', '', NULL, 5500, 0, ''),
(8, 3, 2, NOW() - INTERVAL '20 minutes', 30000, 0, 33000, 'PendingPayment', 'None', 'Takeaway', '', '', NULL, 3000, 0, ''),
(9, 4, 2, NOW() - INTERVAL '25 minutes', 65000, 5000, 66000, 'Paid', 'None', 'Dine-in', 'Table 7', 'Meeting', NULL, 6000, 0, ''),
(10, 5, 2, NOW() - INTERVAL '30 minutes', 40000, 0, 44000, 'PendingPayment', 'None', 'Dine-in', 'Table 2', '', NULL, 4000, 0, ''),
(11, 1, 3, NOW() - INTERVAL '35 minutes', 85000, 0, 93500, 'PendingPayment', 'Preparing', 'Dine-in', 'Table 4', '', NULL, 8500, 0, ''),
(12, 2, 3, NOW() - INTERVAL '40 minutes', 95000, 10000, 93500, 'Paid', 'Preparing', 'Dine-in', 'Table 5', '', NULL, 8500, 0, ''),
(13, 3, 3, NOW() - INTERVAL '45 minutes', 120000, 0, 132000, 'PendingPayment', 'Preparing', 'Dine-in', 'Table 9', '', NULL, 12000, 0, ''),
(14, 4, 3, NOW() - INTERVAL '50 minutes', 35000, 0, 38500, 'PendingPayment', 'Preparing', 'Takeaway', '', '', NULL, 3500, 0, ''),
(15, 5, 3, NOW() - INTERVAL '55 minutes', 50000, 0, 55000, 'Paid', 'Preparing', 'Dine-in', 'Table 1', '', NULL, 5000, 0, ''),
(16, 1, 5, NOW() - INTERVAL '60 minutes', 60000, 0, 66000, 'Paid', 'Preparing', 'Dine-in', 'Table 6', '', NULL, 6000, 0, ''),
(17, 2, 5, NOW() - INTERVAL '65 minutes', 75000, 0, 82500, 'PendingPayment', 'Preparing', 'Dine-in', 'Table 8', '', NULL, 7500, 0, ''),
(18, 3, 5, NOW() - INTERVAL '70 minutes', 45000, 0, 49500, 'Paid', 'Ready', 'Dine-in', 'Table 2', '', NULL, 4500, 0, ''),
(19, 4, 5, NOW() - INTERVAL '75 minutes', 55000, 0, 60500, 'Paid', 'Ready', 'Dine-in', 'Table 4', '', NULL, 5500, 0, ''),
(20, 5, 5, NOW() - INTERVAL '80 minutes', 30000, 0, 33000, 'Paid', 'Ready', 'Takeaway', '', '', NULL, 3000, 0, ''),
(21, 1, 2, NOW() - INTERVAL '85 minutes', 65000, 5000, 66000, 'Paid', 'Ready', 'Dine-in', 'Table 10', '', NULL, 6000, 0, ''),
(22, 2, 2, NOW() - INTERVAL '90 minutes', 40000, 0, 44000, 'Paid', 'Ready', 'Dine-in', 'Table 5', '', NULL, 4000, 0, ''),
(23, 3, 2, NOW() - INTERVAL '95 minutes', 85000, 0, 93500, 'Paid', 'Ready', 'Dine-in', 'Table 7', '', NULL, 8500, 0, ''),
(24, 4, 2, NOW() - INTERVAL '100 minutes', 95000, 10000, 93500, 'Paid', 'Ready', 'Takeaway', '', '', NULL, 8500, 0, ''),
(25, 5, 2, NOW() - INTERVAL '105 minutes', 120000, 0, 132000, 'Paid', 'Ready', 'Dine-in', 'Table 3', '', NULL, 12000, 0, ''),
(26, 1, 3, NOW() - INTERVAL '2 hours', 45000, 0, 49500, 'Paid', 'Completed', 'Dine-in', 'Table 1', '', NULL, 4500, 0, ''),
(27, 2, 3, NOW() - INTERVAL '3 hours', 55000, 0, 60500, 'Paid', 'Completed', 'Dine-in', 'Table 2', '', NULL, 5500, 0, ''),
(28, 3, 3, NOW() - INTERVAL '4 hours', 30000, 0, 33000, 'Paid', 'Completed', 'Takeaway', '', '', NULL, 3000, 0, ''),
(29, 4, 3, NOW() - INTERVAL '5 hours', 65000, 5000, 66000, 'Paid', 'Completed', 'Dine-in', 'Table 6', '', NULL, 6000, 0, ''),
(30, 5, 3, NOW() - INTERVAL '6 hours', 40000, 0, 44000, 'Paid', 'Completed', 'Dine-in', 'Table 4', '', NULL, 4000, 0, ''),
(31, 1, 5, NOW() - INTERVAL '7 hours', 85000, 0, 93500, 'Paid', 'Completed', 'Dine-in', 'Table 5', '', NULL, 8500, 0, ''),
(32, 2, 5, NOW() - INTERVAL '8 hours', 95000, 10000, 93500, 'Paid', 'Completed', 'Dine-in', 'Table 8', '', NULL, 8500, 0, ''),
(33, 3, 5, NOW() - INTERVAL '1 day', 120000, 0, 132000, 'Paid', 'Completed', 'Dine-in', 'Table 1', '', NULL, 12000, 0, ''),
(34, 4, 5, NOW() - INTERVAL '1 day', 35000, 0, 38500, 'Paid', 'Completed', 'Takeaway', '', '', NULL, 3500, 0, ''),
(35, 5, 5, NOW() - INTERVAL '1 day', 50000, 0, 55000, 'Paid', 'Completed', 'Dine-in', 'Table 2', '', NULL, 5000, 0, ''),
(36, 1, 2, NOW() - INTERVAL '2 days', 60000, 0, 66000, 'Cancelled', 'Completed', 'Dine-in', 'Table 3', 'Customer left', NULL, 6000, 0, ''),
(37, 2, 2, NOW() - INTERVAL '2 days', 75000, 0, 82500, 'Cancelled', 'Completed', 'Dine-in', 'Table 5', 'Spilled coffee', NULL, 7500, 0, ''),
(38, 3, 2, NOW() - INTERVAL '3 days', 45000, 0, 49500, 'Cancelled', 'Completed', 'Takeaway', '', 'Changed mind', NULL, 4500, 0, ''),
(39, 4, 2, NOW() - INTERVAL '3 days', 55000, 0, 60500, 'Refunded', 'Completed', 'Dine-in', 'Table 1', 'Wrong item', NULL, 5500, 0, ''),
(40, 5, 2, NOW() - INTERVAL '4 days', 30000, 0, 33000, 'Refunded', 'Completed', 'Dine-in', 'Table 4', 'Tastes bad', NULL, 3000, 0, ''),
(41, 1, 3, NOW() - INTERVAL '4 days', 65000, 5000, 66000, 'Paid', 'Completed', 'Dine-in', 'Table 6', '', NULL, 6000, 0, ''),
(42, 2, 3, NOW() - INTERVAL '5 days', 40000, 0, 44000, 'Paid', 'Completed', 'Dine-in', 'Table 7', '', NULL, 4000, 0, ''),
(43, 3, 3, NOW() - INTERVAL '6 days', 85000, 0, 93500, 'Paid', 'Completed', 'Dine-in', 'Table 2', '', NULL, 8500, 0, ''),
(44, 4, 3, NOW() - INTERVAL '7 days', 95000, 10000, 93500, 'Paid', 'Completed', 'Takeaway', '', '', NULL, 8500, 0, ''),
(45, 5, 3, NOW() - INTERVAL '8 days', 120000, 0, 132000, 'Paid', 'Completed', 'Dine-in', 'Table 5', '', NULL, 12000, 0, '')
ON CONFLICT ("Id") DO UPDATE SET
    "CustomerId" = EXCLUDED."CustomerId",
    "UserId" = EXCLUDED."UserId",
    "OrderDate" = EXCLUDED."OrderDate",
    "SubTotal" = EXCLUDED."SubTotal",
    "DiscountAmount" = EXCLUDED."DiscountAmount",
    "TotalAmount" = EXCLUDED."TotalAmount",
    "PaymentStatus" = EXCLUDED."PaymentStatus",
    "FulfillmentStatus" = EXCLUDED."FulfillmentStatus",
    "Type" = EXCLUDED."Type",
    "TableNumber" = EXCLUDED."TableNumber",
    "Notes" = EXCLUDED."Notes",
    "PromotionId" = EXCLUDED."PromotionId",
    "TaxAmount" = EXCLUDED."TaxAmount",
    "ManualAdjustment" = EXCLUDED."ManualAdjustment",
    "ManualAdjustmentReason" = EXCLUDED."ManualAdjustmentReason";

INSERT INTO "OrderDetails" ("Id", "OrderId", "ProductId", "Quantity", "UnitPrice", "Notes") VALUES
(1, 1, 2, 2, 45000, NULL), (2, 1, 14, 1, 85000, NULL),
(3, 2, 3, 2, 50000, NULL), (4, 2, 17, 2, 45000, NULL),
(5, 3, 5, 1, 55000, NULL), (6, 3, 20, 1, 65000, NULL), (7, 3, 15, 1, 95000, NULL),
(8, 4, 1, 1, 30000, NULL), (9, 4, 21, 1, 60000, NULL), (10, 4, 18, 1, 40000, NULL),
(11, 5, 2, 2, 45000, NULL), (12, 5, 9, 1, 30000, NULL), (13, 5, 16, 1, 75000, NULL),
(14, 6, 1, 1, 30000, NULL), (15, 6, 2, 1, 15000, NULL),
(16, 7, 3, 1, 25000, NULL), (17, 7, 4, 1, 30000, NULL),
(18, 8, 5, 1, 30000, NULL),
(19, 9, 6, 2, 32500, NULL),
(20, 10, 7, 1, 40000, NULL),
(21, 11, 8, 2, 42500, NULL),
(22, 12, 9, 1, 55000, NULL), (23, 12, 1, 1, 40000, NULL),
(24, 13, 10, 3, 40000, NULL),
(25, 14, 11, 1, 35000, NULL),
(26, 15, 12, 2, 25000, NULL),
(27, 16, 13, 1, 60000, NULL),
(28, 17, 14, 1, 75000, NULL),
(29, 18, 15, 1, 45000, NULL),
(30, 19, 16, 1, 55000, NULL),
(31, 20, 17, 1, 30000, NULL),
(32, 21, 18, 2, 32500, NULL),
(33, 22, 19, 1, 40000, NULL),
(34, 23, 20, 2, 42500, NULL),
(35, 24, 21, 1, 55000, NULL), (36, 24, 2, 1, 40000, NULL),
(37, 25, 1, 3, 40000, NULL),
(38, 26, 2, 1, 45000, NULL),
(39, 27, 3, 1, 55000, NULL),
(40, 28, 4, 1, 30000, NULL),
(41, 29, 5, 2, 32500, NULL),
(42, 30, 6, 1, 40000, NULL),
(43, 31, 7, 2, 42500, NULL),
(44, 32, 8, 1, 55000, NULL), (45, 32, 9, 1, 40000, NULL),
(46, 33, 10, 3, 40000, NULL),
(47, 34, 11, 1, 35000, NULL),
(48, 35, 12, 2, 25000, NULL),
(49, 36, 13, 1, 60000, NULL),
(50, 37, 14, 1, 75000, NULL),
(51, 38, 15, 1, 45000, NULL),
(52, 39, 16, 1, 55000, NULL),
(53, 40, 17, 1, 30000, NULL),
(54, 41, 18, 2, 32500, NULL),
(55, 42, 19, 1, 40000, NULL),
(56, 43, 20, 2, 42500, NULL),
(57, 44, 21, 1, 55000, NULL), (58, 44, 1, 1, 40000, NULL),
(59, 45, 2, 3, 40000, NULL)
ON CONFLICT ("Id") DO UPDATE SET
    "OrderId" = EXCLUDED."OrderId",
    "ProductId" = EXCLUDED."ProductId",
    "Quantity" = EXCLUDED."Quantity",
    "UnitPrice" = EXCLUDED."UnitPrice",
    "Notes" = EXCLUDED."Notes";

SELECT setval('"Orders_Id_seq"', (SELECT MAX("Id") FROM "Orders") + 1);
SELECT setval('"OrderDetails_Id_seq"', (SELECT MAX("Id") FROM "OrderDetails") + 1);

-- ===== 06-PROMOTIONS =====
INSERT INTO "Promotions"
    ("Id", "PromoCode", "PromoType", "DiscountValue", "MinOrderAmount", "StartDate", "EndDate", "TargetAudience", "IsActive")
VALUES
    (1, 'MORNING10', 'Percent', 10.00, 50000, NOW() - INTERVAL '5 days', NOW() + INTERVAL '30 days', 'All', true),
    (2, 'VIP25', 'Percent', 25.00, 150000, NOW() - INTERVAL '10 days', NOW() + INTERVAL '10 days', 'VIP', true),
    (3, 'WELCOME30K', 'FixedAmount', 30000.00, 100000, NOW() - INTERVAL '2 days', NOW() + INTERVAL '60 days', 'NewCustomer', true),
    (4, 'LOYALTY15', 'Percent', 15.00, 80000, NOW() - INTERVAL '1 day', NOW() + INTERVAL '15 days', 'Loyalty', true),
    (7, 'NEWYEAR2026', 'Percent', 20.00, 200000, NOW() - INTERVAL '100 days', NOW() - INTERVAL '60 days', 'All', true),
    (8, 'TET60K', 'FixedAmount', 60000.00, 300000, NOW() - INTERVAL '70 days', NOW() - INTERVAL '30 days', 'VIP', true),
    (9, 'FLASH20OLD', 'Percent', 20.00, 120000, NOW() - INTERVAL '20 days', NOW() - INTERVAL '5 days', 'All', true),
    (10, 'SUMMER_SALE', 'Percent', 15.00, 90000, NOW() + INTERVAL '5 days', NOW() + INTERVAL '40 days', 'All', true),
    (11, 'SCHOOL40K', 'FixedAmount', 40000.00, 180000, NOW() + INTERVAL '15 days', NOW() + INTERVAL '45 days', 'NewCustomer', true),
    (12, 'MIDAUTUMN', 'Percent', 30.00, 300000, NOW() + INTERVAL '30 days', NOW() + INTERVAL '60 days', 'All', true),
    (13, 'TEST_OFF', 'Percent', 50.00, 1000000, NOW() - INTERVAL '1 day', NOW() + INTERVAL '10 days', 'All', false),
    (14, 'OFF_VIP', 'FixedAmount', 100000.00, 500000, NOW() - INTERVAL '5 days', NOW() + INTERVAL '5 days', 'VIP', false)
ON CONFLICT ("Id") DO UPDATE SET
    "PromoCode" = EXCLUDED."PromoCode",
    "PromoType" = EXCLUDED."PromoType",
    "DiscountValue" = EXCLUDED."DiscountValue",
    "MinOrderAmount" = EXCLUDED."MinOrderAmount",
    "StartDate" = EXCLUDED."StartDate",
    "EndDate" = EXCLUDED."EndDate",
    "TargetAudience" = EXCLUDED."TargetAudience",
    "IsActive" = EXCLUDED."IsActive";

SELECT setval('"Promotions_Id_seq"', COALESCE((SELECT MAX("Id") FROM "Promotions"), 0) + 1, false);

-- ===== 07-WORKSCHEDULES =====
INSERT INTO "WorkSchedules"
    ("UserId", "WorkDate", "ShiftName", "Note", "Status")
VALUES
    (1, '2026-04-08', 'Morning', 'Admin duty', 'Scheduled'),
    (3, '2026-04-08', 'Afternoon', 'Barista duty', 'Scheduled'),
    (4, '2026-04-08', 'Evening', 'Barista duty', 'Scheduled'),
    (5, '2026-04-08', 'Morning', 'Cashier duty', 'Scheduled'),
    (6, '2026-04-08', 'Afternoon', 'Staff duty', 'Scheduled'),
    (7, '2026-04-08', 'Evening', 'Staff duty', 'Scheduled'),
    (2, '2026-04-09', 'Morning', 'Manager duty', 'Scheduled'),
    (8, '2026-04-09', 'Afternoon', 'Barista duty', 'Scheduled'),
    (3, '2026-04-09', 'Evening', 'Barista duty', 'Scheduled'),
    (10, '2026-04-09', 'Morning', 'Staff duty', 'Scheduled'),
    (11, '2026-04-09', 'Afternoon', 'Staff duty', 'Scheduled'),
    (5, '2026-04-09', 'Evening', 'Cashier duty', 'Scheduled'),
    (1, '2026-04-10', 'Morning', 'Admin duty', 'Scheduled'),
    (4, '2026-04-10', 'Afternoon', 'Barista duty', 'Scheduled'),
    (12, '2026-04-10', 'Evening', 'Barista duty', 'Scheduled'),
    (13, '2026-04-10', 'Morning', 'Cashier duty', 'Scheduled'),
    (14, '2026-04-10', 'Afternoon', 'Staff duty', 'Scheduled'),
    (15, '2026-04-10', 'Evening', 'Staff duty', 'Scheduled')
ON CONFLICT DO NOTHING;

-- ===== 08-APPCONFIG =====
INSERT INTO "AppConfigs" ("SettingKey", "SettingValue", "Description") VALUES
('ShopName', 'MyShop', 'Tên cửa hàng'),
('Phone', '0123456789', 'Số điện thoại liên hệ'),
('Address', '123 Đường ABC,Quận XYZ, TP.HCM', 'Địa chỉ cửa hàng'),
('ShopCode', 'MS001', 'Mã cửa hàng'),
('ContactEmail', 'contact@myshop.com', 'Email liên hệ'),
('ItemsPerPage', '20', 'Số lượng mục hiển thị trên mỗi trang')
ON CONFLICT DO NOTHING;

-- ===== 07-LOYALTY-RULES =====
INSERT INTO "LoyaltyTierConfigs" ("Id", "ConfigKey", "ConfigValue", "Description", "DisplayOrder")
VALUES (1, 'PointsPerVnd', '1000', 'Earn: 1000 VND = 1 point', 1)
ON CONFLICT ("ConfigKey") DO UPDATE SET
    "ConfigValue" = EXCLUDED."ConfigValue",
    "Description" = EXCLUDED."Description",
    "DisplayOrder" = EXCLUDED."DisplayOrder";

INSERT INTO "LoyaltyTierConfigs" ("Id", "ConfigKey", "ConfigValue", "Description", "DisplayOrder")
VALUES (2, 'RedemptionRate', '100', 'Redeem: 1 point = 100 VND', 2)
ON CONFLICT ("ConfigKey") DO UPDATE SET
    "ConfigValue" = EXCLUDED."ConfigValue",
    "Description" = EXCLUDED."Description",
    "DisplayOrder" = EXCLUDED."DisplayOrder";

INSERT INTO "LoyaltyTierConfigs" ("Id", "ConfigKey", "ConfigValue", "Description", "DisplayOrder")
VALUES (10, 'Tier_Bronze', '{"TierName":"Bronze","MinPoints":0,"Multiplier":1.0}', 'Bronze: 1.0x, from 0 pts', 10)
ON CONFLICT ("ConfigKey") DO UPDATE SET
    "ConfigValue" = EXCLUDED."ConfigValue",
    "Description" = EXCLUDED."Description";

INSERT INTO "LoyaltyTierConfigs" ("Id", "ConfigKey", "ConfigValue", "Description", "DisplayOrder")
VALUES (11, 'Tier_Silver', '{"TierName":"Silver","MinPoints":5000,"Multiplier":1.5}', 'Silver: 1.5x, from 5000 pts', 11)
ON CONFLICT ("ConfigKey") DO UPDATE SET
    "ConfigValue" = EXCLUDED."ConfigValue",
    "Description" = EXCLUDED."Description";

INSERT INTO "LoyaltyTierConfigs" ("Id", "ConfigKey", "ConfigValue", "Description", "DisplayOrder")
VALUES (12, 'Tier_Gold', '{"TierName":"Gold","MinPoints":15000,"Multiplier":2.0}', 'Gold: 2.0x, from 15000 pts', 12)
ON CONFLICT ("ConfigKey") DO UPDATE SET
    "ConfigValue" = EXCLUDED."ConfigValue",
    "Description" = EXCLUDED."Description";

INSERT INTO "LoyaltyTierConfigs" ("Id", "ConfigKey", "ConfigValue", "Description", "DisplayOrder")
VALUES (13, 'Tier_Platinum', '{"TierName":"Platinum","MinPoints":30000,"Multiplier":2.5}', 'Platinum: 2.5x, from 30000 pts', 13)
ON CONFLICT ("ConfigKey") DO UPDATE SET
    "ConfigValue" = EXCLUDED."ConfigValue",
    "Description" = EXCLUDED."Description";

UPDATE "Customers" SET "TotalAccumulatedPoints" = COALESCE("TotalAccumulatedPoints", 0), "CurrentAvailablePoints" = COALESCE("CurrentAvailablePoints", 0)
WHERE "TotalAccumulatedPoints" = 0 AND "CurrentAvailablePoints" = 0;

UPDATE "Customers" SET "TotalAccumulatedPoints" = 1200, "CurrentAvailablePoints" = 800, "Tier" = 1 WHERE "Id" = 1;
UPDATE "Customers" SET "TotalAccumulatedPoints" = 5800, "CurrentAvailablePoints" = 3500, "Tier" = 1 WHERE "Id" = 2;
UPDATE "Customers" SET "TotalAccumulatedPoints" = 16000, "CurrentAvailablePoints" = 12000, "Tier" = 2 WHERE "Id" = 3;
UPDATE "Customers" SET "TotalAccumulatedPoints" = 4500, "CurrentAvailablePoints" = 2000, "Tier" = 1 WHERE "Id" = 4;
UPDATE "Customers" SET "TotalAccumulatedPoints" = 35000, "CurrentAvailablePoints" = 28000, "Tier" = 3 WHERE "Id" = 5;
UPDATE "Customers" SET "TotalAccumulatedPoints" = 200, "CurrentAvailablePoints" = 50, "Tier" = 0 WHERE "Id" = 6;

-- ===== VERIFICATION =====
SELECT 'SEED DATA COMPLETE' as Status;
SELECT COUNT(*) as "Categories_Count" FROM "Categories";
SELECT COUNT(*) as "Products_Count" FROM "Products";
SELECT COUNT(*) as "Customers_Count" FROM "Customers";
SELECT COUNT(*) as "Users_Count" FROM "Users";
SELECT COUNT(*) as "Orders_Count" FROM "Orders";
SELECT COUNT(*) as "OrderDetails_Count" FROM "OrderDetails";
SELECT COUNT(*) as "Promotions_Count" FROM "Promotions";
SELECT COUNT(*) as "WorkSchedules_Count" FROM "WorkSchedules";
SELECT COUNT(*) as "AppConfigs_Count" FROM "AppConfigs";
SELECT COUNT(*) as "LoyaltyTierConfigs_Count" FROM "LoyaltyTierConfigs";
