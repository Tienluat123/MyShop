-- ===== PRODUCTS =====
-- Coffee Shop Products
-- Edit by: Antigravity
-- Last modified: 2026-04-26

INSERT INTO "Products" ("Id", "Name", "Price", "BaseCostPrice", "CategoryId") VALUES
-- Coffee (CategoryId = 1)
(1, 'Espresso', 30000, 10000, 1),
(2, 'Latte', 45000, 18000, 1),
(3, 'Cappuccino', 50000, 20000, 1),
(4, 'Americano', 40000, 12000, 1),
(5, 'Macchiato', 55000, 22000, 1),
(6, 'Cold Brew', 48000, 15000, 1),
(7, 'Mocha', 52000, 22000, 1),
(8, 'Flat White', 48000, 18000, 1),

-- Tea (CategoryId = 2)
(9, 'Green Tea', 30000, 8000, 2),
(10, 'Black Tea', 32000, 8000, 2),
(11, 'Jasmine Tea', 35000, 10000, 2),
(12, 'Oolong Tea', 38000, 12000, 2),
(13, 'Iced Tea', 28000, 6000, 2),

-- Cake & Pastry (CategoryId = 3)
(14, 'Chocolate Cake', 85000, 35000, 3),
(15, 'Cheesecake', 95000, 42000, 3),
(16, 'Tiramisu', 75000, 30000, 3),
(17, 'Croissant', 45000, 18000, 3),
(18, 'Muffin Chocolate', 40000, 15000, 3),
(19, 'Donut', 35000, 12000, 3),

-- Snacks (CategoryId = 4)
(20, 'Sandwich Chicken', 65000, 28000, 4),
(21, 'Sandwich Tuna', 60000, 25000, 4),
(22, 'Bagel Cream Cheese', 55000, 20000, 4),
(23, 'Club Sandwich', 75000, 32000, 4)
ON CONFLICT ("Id") DO UPDATE SET
    "Name" = EXCLUDED."Name",
    "Price" = EXCLUDED."Price",
    "BaseCostPrice" = EXCLUDED."BaseCostPrice",
    "CategoryId" = EXCLUDED."CategoryId";

SELECT setval('"Products_Id_seq"', (SELECT MAX("Id") FROM "Products") + 1);
SELECT COUNT(*) as "Products_Count" FROM "Products";
