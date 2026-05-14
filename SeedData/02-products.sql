-- ===== PRODUCTS =====
-- Coffee Shop Products
-- Edit by: Antigravity
-- Last modified: 2026-05-14

INSERT INTO "Products" ("Id", "Name", "Price", "BaseCostPrice", "CategoryId", "ImagePath", "StockQuantity") VALUES
-- Coffee (CategoryId = 1)
(1, 'Espresso', 30000, 10000, 1, 'https://source.unsplash.com/1200x800/?espresso,coffee&sig=101', 24),
(2, 'Latte', 45000, 18000, 1, 'https://source.unsplash.com/1200x800/?latte,coffee&sig=201', 18),
(3, 'Cappuccino', 50000, 20000, 1, 'https://source.unsplash.com/1200x800/?cappuccino,coffee&sig=301', 15),
(4, 'Americano', 40000, 12000, 1, 'https://source.unsplash.com/1200x800/?americano,coffee&sig=401', 30),
(5, 'Macchiato', 55000, 22000, 1, 'https://source.unsplash.com/1200x800/?macchiato,coffee&sig=501', 12),
(6, 'Cold Brew', 48000, 15000, 1, 'https://source.unsplash.com/1200x800/?coldbrew,coffee&sig=601', 9),
(7, 'Mocha', 52000, 22000, 1, 'https://source.unsplash.com/1200x800/?mocha,coffee&sig=701', 6),
(8, 'Flat White', 48000, 18000, 1, 'https://source.unsplash.com/1200x800/?flatwhite,coffee&sig=801', 4),

-- Tea (CategoryId = 2)
(9, 'Green Tea', 30000, 8000, 2, 'https://source.unsplash.com/1200x800/?green,tea&sig=901', 26),
(10, 'Black Tea', 32000, 8000, 2, 'https://source.unsplash.com/1200x800/?black,tea&sig=1001', 22),
(11, 'Jasmine Tea', 35000, 10000, 2, 'https://source.unsplash.com/1200x800/?jasmine,tea&sig=1101', 16),
(12, 'Oolong Tea', 38000, 12000, 2, 'https://source.unsplash.com/1200x800/?oolong,tea&sig=1201', 10),
(13, 'Iced Tea', 28000, 6000, 2, 'https://source.unsplash.com/1200x800/?iced,tea&sig=1301', 3),

-- Cake & Pastry (CategoryId = 3)
(14, 'Chocolate Cake', 85000, 35000, 3, 'https://source.unsplash.com/1200x800/?chocolate,cake&sig=1401', 8),
(15, 'Cheesecake', 95000, 42000, 3, 'https://source.unsplash.com/1200x800/?cheesecake,dessert&sig=1501', 5),
(16, 'Tiramisu', 75000, 30000, 3, 'https://source.unsplash.com/1200x800/?tiramisu,dessert&sig=1601', 4),
(17, 'Croissant', 45000, 18000, 3, 'https://source.unsplash.com/1200x800/?croissant,bakery&sig=1701', 28),
(18, 'Muffin Chocolate', 40000, 15000, 3, 'https://source.unsplash.com/1200x800/?chocolate,muffin&sig=1801', 17),
(19, 'Donut', 35000, 12000, 3, 'https://source.unsplash.com/1200x800/?donut,dessert&sig=1901', 2),

-- Snacks (CategoryId = 4)
(20, 'Sandwich Chicken', 65000, 28000, 4, 'https://source.unsplash.com/1200x800/?chicken,sandwich&sig=2001', 11),
(21, 'Sandwich Tuna', 60000, 25000, 4, 'https://source.unsplash.com/1200x800/?tuna,sandwich&sig=2101', 7),
(22, 'Bagel Cream Cheese', 55000, 20000, 4, 'https://source.unsplash.com/1200x800/?bagel,creamcheese&sig=2201', 14),
(23, 'Club Sandwich', 75000, 32000, 4, 'https://source.unsplash.com/1200x800/?club,sandwich&sig=2301', 3)
ON CONFLICT ("Id") DO UPDATE SET
    "Name" = EXCLUDED."Name",
    "Price" = EXCLUDED."Price",
    "BaseCostPrice" = EXCLUDED."BaseCostPrice",
    "CategoryId" = EXCLUDED."CategoryId",
    "ImagePath" = EXCLUDED."ImagePath",
    "StockQuantity" = EXCLUDED."StockQuantity";

SELECT setval('"Products_Id_seq"', COALESCE((SELECT MAX("Id") FROM "Products"), 1), TRUE);
SELECT COUNT(*) as "Products_Count" FROM "Products";
