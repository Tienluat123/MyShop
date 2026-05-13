-- ============================================================================
-- INSERT PRODUCT IMAGES FOR NEW PRODUCTS (3 images per product)
-- Products with IDs 1-19, 26-28 already have images, skip them
-- ============================================================================

-- Insert ProductImages for all products that need them (3 images per product)
-- Using picsum.photos as placeholder image source
INSERT INTO "ProductImages" ("ProductId", "ImageUrl", "SortOrder", "IsPrimary", "CreatedAt") VALUES

-- Category 1 (Coffee) - Products needing images: 30, 33, 34, 37, 40-53
-- Product 30: Tra sua hun khoi
(30, 'https://picsum.photos/400/400?u=product30_1', 1, true, NOW()),
(30, 'https://picsum.photos/400/400?u=product30_2', 2, false, NOW()),
(30, 'https://picsum.photos/400/400?u=product30_3', 3, false, NOW()),

-- Product 33: Specialty Coffee
(33, 'https://picsum.photos/400/400?u=product33_1', 1, true, NOW()),
(33, 'https://picsum.photos/400/400?u=product33_2', 2, false, NOW()),
(33, 'https://picsum.photos/400/400?u=product33_3', 3, false, NOW()),

-- Product 34: Rich Latte
(34, 'https://picsum.photos/400/400?u=product34_1', 1, true, NOW()),
(34, 'https://picsum.photos/400/400?u=product34_2', 2, false, NOW()),
(34, 'https://picsum.photos/400/400?u=product34_3', 3, false, NOW()),

-- Product 37: Coffee II
(37, 'https://picsum.photos/400/400?u=product37_1', 1, true, NOW()),
(37, 'https://picsum.photos/400/400?u=product37_2', 2, false, NOW()),
(37, 'https://picsum.photos/400/400?u=product37_3', 3, false, NOW()),

-- Product 40: Affogato
(40, 'https://picsum.photos/400/400?u=product40_1', 1, true, NOW()),
(40, 'https://picsum.photos/400/400?u=product40_2', 2, false, NOW()),
(40, 'https://picsum.photos/400/400?u=product40_3', 3, false, NOW()),

-- Product 41: Lungo
(41, 'https://picsum.photos/400/400?u=product41_1', 1, true, NOW()),
(41, 'https://picsum.photos/400/400?u=product41_2', 2, false, NOW()),
(41, 'https://picsum.photos/400/400?u=product41_3', 3, false, NOW()),

-- Product 42: Ristretto
(42, 'https://picsum.photos/400/400?u=product42_1', 1, true, NOW()),
(42, 'https://picsum.photos/400/400?u=product42_2', 2, false, NOW()),
(42, 'https://picsum.photos/400/400?u=product42_3', 3, false, NOW()),

-- Product 43: Cortado
(43, 'https://picsum.photos/400/400?u=product43_1', 1, true, NOW()),
(43, 'https://picsum.photos/400/400?u=product43_2', 2, false, NOW()),
(43, 'https://picsum.photos/400/400?u=product43_3', 3, false, NOW()),

-- Product 44: Macchiato Lungo
(44, 'https://picsum.photos/400/400?u=product44_1', 1, true, NOW()),
(44, 'https://picsum.photos/400/400?u=product44_2', 2, false, NOW()),
(44, 'https://picsum.photos/400/400?u=product44_3', 3, false, NOW()),

-- Product 45: Espresso Con Panna
(45, 'https://picsum.photos/400/400?u=product45_1', 1, true, NOW()),
(45, 'https://picsum.photos/400/400?u=product45_2', 2, false, NOW()),
(45, 'https://picsum.photos/400/400?u=product45_3', 3, false, NOW()),

-- Product 46: Doppio
(46, 'https://picsum.photos/400/400?u=product46_1', 1, true, NOW()),
(46, 'https://picsum.photos/400/400?u=product46_2', 2, false, NOW()),
(46, 'https://picsum.photos/400/400?u=product46_3', 3, false, NOW()),

-- Product 47: Lungo Freddo
(47, 'https://picsum.photos/400/400?u=product47_1', 1, true, NOW()),
(47, 'https://picsum.photos/400/400?u=product47_2', 2, false, NOW()),
(47, 'https://picsum.photos/400/400?u=product47_3', 3, false, NOW()),

-- Product 48: Irish Coffee
(48, 'https://picsum.photos/400/400?u=product48_1', 1, true, NOW()),
(48, 'https://picsum.photos/400/400?u=product48_2', 2, false, NOW()),
(48, 'https://picsum.photos/400/400?u=product48_3', 3, false, NOW()),

-- Product 49: Vanilla Latte
(49, 'https://picsum.photos/400/400?u=product49_1', 1, true, NOW()),
(49, 'https://picsum.photos/400/400?u=product49_2', 2, false, NOW()),
(49, 'https://picsum.photos/400/400?u=product49_3', 3, false, NOW()),

-- Product 50: Caramel Macchiato
(50, 'https://picsum.photos/400/400?u=product50_1', 1, true, NOW()),
(50, 'https://picsum.photos/400/400?u=product50_2', 2, false, NOW()),
(50, 'https://picsum.photos/400/400?u=product50_3', 3, false, NOW()),

-- Product 51: Hazelnut Cappuccino
(51, 'https://picsum.photos/400/400?u=product51_1', 1, true, NOW()),
(51, 'https://picsum.photos/400/400?u=product51_2', 2, false, NOW()),
(51, 'https://picsum.photos/400/400?u=product51_3', 3, false, NOW()),

-- Product 52: Signature Blend
(52, 'https://picsum.photos/400/400?u=product52_1', 1, true, NOW()),
(52, 'https://picsum.photos/400/400?u=product52_2', 2, false, NOW()),
(52, 'https://picsum.photos/400/400?u=product52_3', 3, false, NOW()),

-- Product 53: Premium Espresso
(53, 'https://picsum.photos/400/400?u=product53_1', 1, true, NOW()),
(53, 'https://picsum.photos/400/400?u=product53_2', 2, false, NOW()),
(53, 'https://picsum.photos/400/400?u=product53_3', 3, false, NOW()),

-- Category 2 (Tea) - Products needing images: 30, 35, 38, 54-70
-- Product 30: Tra sua hun khoi (duplicate check - already in category 1)
-- Product 35: Pure Green Tea
(35, 'https://picsum.photos/400/400?u=product35_1', 1, true, NOW()),
(35, 'https://picsum.photos/400/400?u=product35_2', 2, false, NOW()),
(35, 'https://picsum.photos/400/400?u=product35_3', 3, false, NOW()),

-- Product 38: Grape fruit cold brew
(38, 'https://picsum.photos/400/400?u=product38_1', 1, true, NOW()),
(38, 'https://picsum.photos/400/400?u=product38_2', 2, false, NOW()),
(38, 'https://picsum.photos/400/400?u=product38_3', 3, false, NOW()),

-- Product 54: Nuoc cam (duplicate from original)
(54, 'https://picsum.photos/400/400?u=product54_1', 1, true, NOW()),
(54, 'https://picsum.photos/400/400?u=product54_2', 2, false, NOW()),
(54, 'https://picsum.photos/400/400?u=product54_3', 3, false, NOW()),

-- Product 55: Cafe (duplicate from original)
(55, 'https://picsum.photos/400/400?u=product55_1', 1, true, NOW()),
(55, 'https://picsum.photos/400/400?u=product55_2', 2, false, NOW()),
(55, 'https://picsum.photos/400/400?u=product55_3', 3, false, NOW()),

-- Product 57: Matcha Latte
(57, 'https://picsum.photos/400/400?u=product57_1', 1, true, NOW()),
(57, 'https://picsum.photos/400/400?u=product57_2', 2, false, NOW()),
(57, 'https://picsum.photos/400/400?u=product57_3', 3, false, NOW()),

-- Product 58: Pure Green Tea
(58, 'https://picsum.photos/400/400?u=product58_1', 1, true, NOW()),
(58, 'https://picsum.photos/400/400?u=product58_2', 2, false, NOW()),
(58, 'https://picsum.photos/400/400?u=product58_3', 3, false, NOW()),

-- Product 59: Grape fruit cold brew
(59, 'https://picsum.photos/400/400?u=product59_1', 1, true, NOW()),
(59, 'https://picsum.photos/400/400?u=product59_2', 2, false, NOW()),
(59, 'https://picsum.photos/400/400?u=product59_3', 3, false, NOW()),

-- Product 60: White Tea
(60, 'https://picsum.photos/400/400?u=product60_1', 1, true, NOW()),
(60, 'https://picsum.photos/400/400?u=product60_2', 2, false, NOW()),
(60, 'https://picsum.photos/400/400?u=product60_3', 3, false, NOW()),

-- Product 61: Pu-erh Tea
(61, 'https://picsum.photos/400/400?u=product61_1', 1, true, NOW()),
(61, 'https://picsum.photos/400/400?u=product61_2', 2, false, NOW()),
(61, 'https://picsum.photos/400/400?u=product61_3', 3, false, NOW()),

-- Product 62: Butterfly Pea Flower Tea
(62, 'https://picsum.photos/400/400?u=product62_1', 1, true, NOW()),
(62, 'https://picsum.photos/400/400?u=product62_2', 2, false, NOW()),
(62, 'https://picsum.photos/400/400?u=product62_3', 3, false, NOW()),

-- Product 63: Chrysanthemum Tea
(63, 'https://picsum.photos/400/400?u=product63_1', 1, true, NOW()),
(63, 'https://picsum.photos/400/400?u=product63_2', 2, false, NOW()),
(63, 'https://picsum.photos/400/400?u=product63_3', 3, false, NOW()),

-- Product 64: Lemongrass Tea
(64, 'https://picsum.photos/400/400?u=product64_1', 1, true, NOW()),
(64, 'https://picsum.photos/400/400?u=product64_2', 2, false, NOW()),
(64, 'https://picsum.photos/400/400?u=product64_3', 3, false, NOW()),

-- Product 65: Honey Dew Tea
(65, 'https://picsum.photos/400/400?u=product65_1', 1, true, NOW()),
(65, 'https://picsum.photos/400/400?u=product65_2', 2, false, NOW()),
(65, 'https://picsum.photos/400/400?u=product65_3', 3, false, NOW()),

-- Product 66: Peach Tea
(66, 'https://picsum.photos/400/400?u=product66_1', 1, true, NOW()),
(66, 'https://picsum.photos/400/400?u=product66_2', 2, false, NOW()),
(66, 'https://picsum.photos/400/400?u=product66_3', 3, false, NOW()),

-- Product 67: Mango Tea
(67, 'https://picsum.photos/400/400?u=product67_1', 1, true, NOW()),
(67, 'https://picsum.photos/400/400?u=product67_2', 2, false, NOW()),
(67, 'https://picsum.photos/400/400?u=product67_3', 3, false, NOW()),

-- Product 68: Lychee Tea
(68, 'https://picsum.photos/400/400?u=product68_1', 1, true, NOW()),
(68, 'https://picsum.photos/400/400?u=product68_2', 2, false, NOW()),
(68, 'https://picsum.photos/400/400?u=product68_3', 3, false, NOW()),

-- Product 69: Passion Fruit Tea
(69, 'https://picsum.photos/400/400?u=product69_1', 1, true, NOW()),
(69, 'https://picsum.photos/400/400?u=product69_2', 2, false, NOW()),
(69, 'https://picsum.photos/400/400?u=product69_3', 3, false, NOW()),

-- Product 70: Mixed Berries Tea
(70, 'https://picsum.photos/400/400?u=product70_1', 1, true, NOW()),
(70, 'https://picsum.photos/400/400?u=product70_2', 2, false, NOW()),
(70, 'https://picsum.photos/400/400?u=product70_3', 3, false, NOW()),

-- Category 3 (Cake & Pastry) - Products needing images: 32, 36, 71-86
-- Product 32: Patechaud
(32, 'https://picsum.photos/400/400?u=product32_1', 1, true, NOW()),
(32, 'https://picsum.photos/400/400?u=product32_2', 2, false, NOW()),
(32, 'https://picsum.photos/400/400?u=product32_3', 3, false, NOW()),

-- Product 36: Pate-chaud
(36, 'https://picsum.photos/400/400?u=product36_1', 1, true, NOW()),
(36, 'https://picsum.photos/400/400?u=product36_2', 2, false, NOW()),
(36, 'https://picsum.photos/400/400?u=product36_3', 3, false, NOW()),

-- Product 71: Patechaud
(71, 'https://picsum.photos/400/400?u=product71_1', 1, true, NOW()),
(71, 'https://picsum.photos/400/400?u=product71_2', 2, false, NOW()),
(71, 'https://picsum.photos/400/400?u=product71_3', 3, false, NOW()),

-- Product 72: Carrot Cake
(72, 'https://picsum.photos/400/400?u=product72_1', 1, true, NOW()),
(72, 'https://picsum.photos/400/400?u=product72_2', 2, false, NOW()),
(72, 'https://picsum.photos/400/400?u=product72_3', 3, false, NOW()),

-- Product 73: Vanilla Cake
(73, 'https://picsum.photos/400/400?u=product73_1', 1, true, NOW()),
(73, 'https://picsum.photos/400/400?u=product73_2', 2, false, NOW()),
(73, 'https://picsum.photos/400/400?u=product73_3', 3, false, NOW()),

-- Product 74: Red Velvet Cake
(74, 'https://picsum.photos/400/400?u=product74_1', 1, true, NOW()),
(74, 'https://picsum.photos/400/400?u=product74_2', 2, false, NOW()),
(74, 'https://picsum.photos/400/400?u=product74_3', 3, false, NOW()),

-- Product 75: Strawberry Cheesecake
(75, 'https://picsum.photos/400/400?u=product75_1', 1, true, NOW()),
(75, 'https://picsum.photos/400/400?u=product75_2', 2, false, NOW()),
(75, 'https://picsum.photos/400/400?u=product75_3', 3, false, NOW()),

-- Product 76: Lemon Tart
(76, 'https://picsum.photos/400/400?u=product76_1', 1, true, NOW()),
(76, 'https://picsum.photos/400/400?u=product76_2', 2, false, NOW()),
(76, 'https://picsum.photos/400/400?u=product76_3', 3, false, NOW()),

-- Product 77: Apple Pie
(77, 'https://picsum.photos/400/400?u=product77_1', 1, true, NOW()),
(77, 'https://picsum.photos/400/400?u=product77_2', 2, false, NOW()),
(77, 'https://picsum.photos/400/400?u=product77_3', 3, false, NOW()),

-- Product 78: Blueberry Muffin
(78, 'https://picsum.photos/400/400?u=product78_1', 1, true, NOW()),
(78, 'https://picsum.photos/400/400?u=product78_2', 2, false, NOW()),
(78, 'https://picsum.photos/400/400?u=product78_3', 3, false, NOW()),

-- Product 79: Banana Bread
(79, 'https://picsum.photos/400/400?u=product79_1', 1, true, NOW()),
(79, 'https://picsum.photos/400/400?u=product79_2', 2, false, NOW()),
(79, 'https://picsum.photos/400/400?u=product79_3', 3, false, NOW()),

-- Product 80: Scone with Jam
(80, 'https://picsum.photos/400/400?u=product80_1', 1, true, NOW()),
(80, 'https://picsum.photos/400/400?u=product80_2', 2, false, NOW()),
(80, 'https://picsum.photos/400/400?u=product80_3', 3, false, NOW()),

-- Product 81: Chocolate Chip Cookie
(81, 'https://picsum.photos/400/400?u=product81_1', 1, true, NOW()),
(81, 'https://picsum.photos/400/400?u=product81_2', 2, false, NOW()),
(81, 'https://picsum.photos/400/400?u=product81_3', 3, false, NOW()),

-- Product 82: Brownie
(82, 'https://picsum.photos/400/400?u=product82_1', 1, true, NOW()),
(82, 'https://picsum.photos/400/400?u=product82_2', 2, false, NOW()),
(82, 'https://picsum.photos/400/400?u=product82_3', 3, false, NOW()),

-- Product 83: Eclair
(83, 'https://picsum.photos/400/400?u=product83_1', 1, true, NOW()),
(83, 'https://picsum.photos/400/400?u=product83_2', 2, false, NOW()),
(83, 'https://picsum.photos/400/400?u=product83_3', 3, false, NOW()),

-- Product 84: Profiterole
(84, 'https://picsum.photos/400/400?u=product84_1', 1, true, NOW()),
(84, 'https://picsum.photos/400/400?u=product84_2', 2, false, NOW()),
(84, 'https://picsum.photos/400/400?u=product84_3', 3, false, NOW()),

-- Product 85: Macarons
(85, 'https://picsum.photos/400/400?u=product85_1', 1, true, NOW()),
(85, 'https://picsum.photos/400/400?u=product85_2', 2, false, NOW()),
(85, 'https://picsum.photos/400/400?u=product85_3', 3, false, NOW()),

-- Product 86: Tart Tatin
(86, 'https://picsum.photos/400/400?u=product86_1', 1, true, NOW()),
(86, 'https://picsum.photos/400/400?u=product86_2', 2, false, NOW()),
(86, 'https://picsum.photos/400/400?u=product86_3', 3, false, NOW())

ON CONFLICT DO NOTHING;

-- Reset sequence
SELECT setval('"ProductImages_Id_seq"', (SELECT MAX("Id") FROM "ProductImages") + 1);

-- Verification: Count images by product
SELECT 
    "ProductId",
    (SELECT "Name" FROM "Products" p WHERE p."Id" = "ProductId") as "ProductName",
    COUNT(*) as "ImageCount"
FROM "ProductImages"
GROUP BY "ProductId"
ORDER BY "ProductId";

-- Final verification
SELECT COUNT(*) as "Total_Images" FROM "ProductImages";
