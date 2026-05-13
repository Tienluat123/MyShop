-- ============================================================================
-- ADD PRODUCTS FOR CATEGORIES 1, 2, 3 (TO REACH 22 EACH) & DELETE CATEGORY 4
-- Script to ensure balanced product count across categories
-- ============================================================================

-- Step 1: Delete OrderDetails that reference products in Category 4
DELETE FROM "OrderDetails"
WHERE "ProductId" IN (
    SELECT "Id" FROM "Products" WHERE "CategoryId" = 4
);

-- Step 2: Delete Products from Category 4
DELETE FROM "Products" WHERE "CategoryId" = 4;

-- Step 3: Delete Category 4
DELETE FROM "Categories" WHERE "Id" = 4;

-- Step 4: Reset sequences if needed
SELECT setval('"Products_Id_seq"', (SELECT MAX("Id") FROM "Products") + 1);
SELECT setval('"Categories_Id_seq"', (SELECT MAX("Id") FROM "Categories") + 1);

-- Step 5: Add products to Category 1 (Coffee) - currently 8, add 14 more to reach 22
INSERT INTO "Products" ("Name", "Price", "BaseCostPrice", "CategoryId") VALUES
-- Coffee additions
('Affogato', 35000, 15000, 1),
('Lungo', 32000, 11000, 1),
('Ristretto', 28000, 9000, 1),
('Cortado', 38000, 14000, 1),
('Macchiato Lungo', 50000, 20000, 1),
('Espresso Con Panna', 40000, 16000, 1),
('Doppio', 42000, 15000, 1),
('Lungo Freddo', 45000, 17000, 1),
('Irish Coffee', 55000, 25000, 1),
('Vanilla Latte', 48000, 19000, 1),
('Caramel Macchiato', 52000, 21000, 1),
('Hazelnut Cappuccino', 50000, 20000, 1),
('Signature Blend', 58000, 24000, 1),
('Premium Espresso', 65000, 28000, 1)
ON CONFLICT DO NOTHING;

-- Step 6: Add products to Category 2 (Tea) - currently 5, add 17 more to reach 22
INSERT INTO "Products" ("Name", "Price", "BaseCostPrice", "CategoryId") VALUES
-- Tea additions
('Nuoc cam', 28000, 10000, 2),
('Cafe', 30000, 12000, 2),
('Tra sua hun khoi', 32000, 12000, 2),
('Matcha Latte', 42000, 16000, 2),
('Pure Green Tea', 32000, 10000, 2),
('Grape fruit cold brew', 38000, 14000, 2),
('White Tea', 35000, 11000, 2),
('Pu-erh Tea', 38000, 13000, 2),
('Butterfly Pea Flower Tea', 40000, 15000, 2),
('Chrysanthemum Tea', 32000, 10000, 2),
('Lemongrass Tea', 35000, 12000, 2),
('Honey Dew Tea', 42000, 16000, 2),
('Peach Tea', 38000, 14000, 2),
('Mango Tea', 40000, 15000, 2),
('Lychee Tea', 42000, 16000, 2),
('Passion Fruit Tea', 38000, 14000, 2),
('Mixed Berries Tea', 40000, 15000, 2)
ON CONFLICT DO NOTHING;

-- Step 7: Add products to Category 3 (Cake & Pastry) - currently 6, add 16 more to reach 22
INSERT INTO "Products" ("Name", "Price", "BaseCostPrice", "CategoryId") VALUES
-- Cake & Pastry additions
('Patechaud', 85000, 35000, 3),
('Carrot Cake', 75000, 32000, 3),
('Vanilla Cake', 70000, 30000, 3),
('Red Velvet Cake', 85000, 36000, 3),
('Strawberry Cheesecake', 95000, 42000, 3),
('Lemon Tart', 65000, 26000, 3),
('Apple Pie', 68000, 28000, 3),
('Blueberry Muffin', 42000, 16000, 3),
('Banana Bread', 48000, 18000, 3),
('Scone with Jam', 45000, 17000, 3),
('Chocolate Chip Cookie', 32000, 12000, 3),
('Brownie', 45000, 18000, 3),
('Eclair', 52000, 20000, 3),
('Profiterole', 50000, 19000, 3),
('Macarons', 55000, 22000, 3),
('Tart Tatin', 70000, 29000, 3)
ON CONFLICT DO NOTHING;

-- Step 8: Reset sequences after bulk insert
SELECT setval('"Products_Id_seq"', (SELECT MAX("Id") FROM "Products") + 1);

-- Verification: Show product count by category
SELECT 
    "CategoryId",
    (SELECT "Name" FROM "Categories" c WHERE c."Id" = "CategoryId") as "CategoryName",
    COUNT(*) as "ProductCount"
FROM "Products"
GROUP BY "CategoryId"
ORDER BY "CategoryId";

-- Final verification: Show total products
SELECT COUNT(*) as "Total_Products" FROM "Products";
