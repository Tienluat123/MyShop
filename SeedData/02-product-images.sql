-- ===== PRODUCT IMAGES =====
-- Upsert 3 images / product for provided base product list.
-- Rule: SortOrder 1 = primary, SortOrder 2-3 = secondary.

WITH image_data ("ProductId", "ImageUrl", "SortOrder", "IsPrimary") AS (
	VALUES
	-- 1 Espresso
	(1,  'https://source.unsplash.com/1200x800/?espresso,coffee&sig=101', 1, TRUE),
	(1,  'https://source.unsplash.com/1200x800/?espresso,beans&sig=102', 2, FALSE),
	(1,  'https://source.unsplash.com/1200x800/?espresso,barista&sig=103', 3, FALSE),

	-- 2 Latte
	(2,  'https://source.unsplash.com/1200x800/?latte,coffee&sig=201', 1, TRUE),
	(2,  'https://source.unsplash.com/1200x800/?latte,art&sig=202', 2, FALSE),
	(2,  'https://source.unsplash.com/1200x800/?milk,coffee&sig=203', 3, FALSE),

	-- 3 Cappuccino
	(3,  'https://source.unsplash.com/1200x800/?cappuccino,coffee&sig=301', 1, TRUE),
	(3,  'https://source.unsplash.com/1200x800/?cappuccino,foam&sig=302', 2, FALSE),
	(3,  'https://source.unsplash.com/1200x800/?coffee,cup&sig=303', 3, FALSE),

	-- 4 Americano
	(4,  'https://source.unsplash.com/1200x800/?americano,coffee&sig=401', 1, TRUE),
	(4,  'https://source.unsplash.com/1200x800/?black,coffee&sig=402', 2, FALSE),
	(4,  'https://source.unsplash.com/1200x800/?hot,coffee&sig=403', 3, FALSE),

	-- 5 Macchiato
	(5,  'https://source.unsplash.com/1200x800/?macchiato,coffee&sig=501', 1, TRUE),
	(5,  'https://source.unsplash.com/1200x800/?espresso,milk&sig=502', 2, FALSE),
	(5,  'https://source.unsplash.com/1200x800/?coffee,smallcup&sig=503', 3, FALSE),

	-- 6 Cold Brew
	(6,  'https://source.unsplash.com/1200x800/?coldbrew,coffee&sig=601', 1, TRUE),
	(6,  'https://source.unsplash.com/1200x800/?iced,coffee&sig=602', 2, FALSE),
	(6,  'https://source.unsplash.com/1200x800/?coffee,glass&sig=603', 3, FALSE),

	-- 7 Mocha
	(7,  'https://source.unsplash.com/1200x800/?mocha,coffee&sig=701', 1, TRUE),
	(7,  'https://source.unsplash.com/1200x800/?chocolate,coffee&sig=702', 2, FALSE),
	(7,  'https://source.unsplash.com/1200x800/?coffee,cocoa&sig=703', 3, FALSE),

	-- 8 Flat White
	(8,  'https://source.unsplash.com/1200x800/?flatwhite,coffee&sig=801', 1, TRUE),
	(8,  'https://source.unsplash.com/1200x800/?coffee,latteart&sig=802', 2, FALSE),
	(8,  'https://source.unsplash.com/1200x800/?milk,foam&sig=803', 3, FALSE),

	-- 9 Green Tea
	(9,  'https://source.unsplash.com/1200x800/?green,tea&sig=901', 1, TRUE),
	(9,  'https://source.unsplash.com/1200x800/?tea,leaf&sig=902', 2, FALSE),
	(9,  'https://source.unsplash.com/1200x800/?teacup,green&sig=903', 3, FALSE),

	-- 10 Black Tea
	(10, 'https://source.unsplash.com/1200x800/?black,tea&sig=1001', 1, TRUE),
	(10, 'https://source.unsplash.com/1200x800/?tea,pot&sig=1002', 2, FALSE),
	(10, 'https://source.unsplash.com/1200x800/?teacup,hot&sig=1003', 3, FALSE),

	-- 11 Jasmine Tea
	(11, 'https://source.unsplash.com/1200x800/?jasmine,tea&sig=1101', 1, TRUE),
	(11, 'https://source.unsplash.com/1200x800/?tea,flower&sig=1102', 2, FALSE),
	(11, 'https://source.unsplash.com/1200x800/?herbal,tea&sig=1103', 3, FALSE),

	-- 12 Oolong Tea
	(12, 'https://source.unsplash.com/1200x800/?oolong,tea&sig=1201', 1, TRUE),
	(12, 'https://source.unsplash.com/1200x800/?tea,leaves&sig=1202', 2, FALSE),
	(12, 'https://source.unsplash.com/1200x800/?tea,ceramiccup&sig=1203', 3, FALSE),

	-- 13 Iced Tea
	(13, 'https://source.unsplash.com/1200x800/?iced,tea&sig=1301', 1, TRUE),
	(13, 'https://source.unsplash.com/1200x800/?tea,ice&sig=1302', 2, FALSE),
	(13, 'https://source.unsplash.com/1200x800/?lemon,tea&sig=1303', 3, FALSE),

	-- 14 Chocolate Cake
	(14, 'https://source.unsplash.com/1200x800/?chocolate,cake&sig=1401', 1, TRUE),
	(14, 'https://source.unsplash.com/1200x800/?cake,slice&sig=1402', 2, FALSE),
	(14, 'https://source.unsplash.com/1200x800/?dessert,chocolate&sig=1403', 3, FALSE),

	-- 15 Cheesecake
	(15, 'https://source.unsplash.com/1200x800/?cheesecake,dessert&sig=1501', 1, TRUE),
	(15, 'https://source.unsplash.com/1200x800/?cake,cheese&sig=1502', 2, FALSE),
	(15, 'https://source.unsplash.com/1200x800/?berry,cheesecake&sig=1503', 3, FALSE),

	-- 16 Tiramisu
	(16, 'https://source.unsplash.com/1200x800/?tiramisu,dessert&sig=1601', 1, TRUE),
	(16, 'https://source.unsplash.com/1200x800/?coffee,dessert&sig=1602', 2, FALSE),
	(16, 'https://source.unsplash.com/1200x800/?mascarpone,cake&sig=1603', 3, FALSE),

	-- 17 Croissant
	(17, 'https://source.unsplash.com/1200x800/?croissant,bakery&sig=1701', 1, TRUE),
	(17, 'https://source.unsplash.com/1200x800/?pastry,butter&sig=1702', 2, FALSE),
	(17, 'https://source.unsplash.com/1200x800/?bread,croissant&sig=1703', 3, FALSE),

	-- 18 Muffin Chocolate
	(18, 'https://source.unsplash.com/1200x800/?chocolate,muffin&sig=1801', 1, TRUE),
	(18, 'https://source.unsplash.com/1200x800/?muffin,dessert&sig=1802', 2, FALSE),
	(18, 'https://source.unsplash.com/1200x800/?cupcake,chocolate&sig=1803', 3, FALSE),

	-- 19 Donut
	(19, 'https://source.unsplash.com/1200x800/?donut,dessert&sig=1901', 1, TRUE),
	(19, 'https://source.unsplash.com/1200x800/?doughnut,glaze&sig=1902', 2, FALSE),
	(19, 'https://source.unsplash.com/1200x800/?sweet,donut&sig=1903', 3, FALSE),

	-- 20 Sandwich Chicken
	(20, 'https://source.unsplash.com/1200x800/?chicken,sandwich&sig=2001', 1, TRUE),
	(20, 'https://source.unsplash.com/1200x800/?sandwich,lunch&sig=2002', 2, FALSE),
	(20, 'https://source.unsplash.com/1200x800/?grilled,chicken&sig=2003', 3, FALSE),

	-- 21 Sandwich Tuna
	(21, 'https://source.unsplash.com/1200x800/?tuna,sandwich&sig=2101', 1, TRUE),
	(21, 'https://source.unsplash.com/1200x800/?sandwich,fish&sig=2102', 2, FALSE),
	(21, 'https://source.unsplash.com/1200x800/?healthy,sandwich&sig=2103', 3, FALSE),

	-- 22 Bagel Cream Cheese
	(22, 'https://source.unsplash.com/1200x800/?bagel,creamcheese&sig=2201', 1, TRUE),
	(22, 'https://source.unsplash.com/1200x800/?bagel,breakfast&sig=2202', 2, FALSE),
	(22, 'https://source.unsplash.com/1200x800/?bread,creamcheese&sig=2203', 3, FALSE),

	-- 23 Club Sandwich
	(23, 'https://source.unsplash.com/1200x800/?club,sandwich&sig=2301', 1, TRUE),
	(23, 'https://source.unsplash.com/1200x800/?sandwich,toast&sig=2302', 2, FALSE),
	(23, 'https://source.unsplash.com/1200x800/?ham,sandwich&sig=2303', 3, FALSE),

	-- 26 Nuoc cam
	(26, 'https://source.unsplash.com/1200x800/?orange,juice&sig=2601', 1, TRUE),
	(26, 'https://source.unsplash.com/1200x800/?fresh,orangejuice&sig=2602', 2, FALSE),
	(26, 'https://source.unsplash.com/1200x800/?citrus,drink&sig=2603', 3, FALSE),

	-- 27 Nuoc mia
	(27, 'https://source.unsplash.com/1200x800/?sugarcane,juice&sig=2701', 1, TRUE),
	(27, 'https://source.unsplash.com/1200x800/?sugarcane,drink&sig=2702', 2, FALSE),
	(27, 'https://source.unsplash.com/1200x800/?cold,juice&sig=2703', 3, FALSE),

	-- 28 Cafe
	(28, 'https://source.unsplash.com/1200x800/?coffee,cafe&sig=2801', 1, TRUE),
	(28, 'https://source.unsplash.com/1200x800/?coffee,shop&sig=2802', 2, FALSE),
	(28, 'https://source.unsplash.com/1200x800/?barista,coffee&sig=2803', 3, FALSE)
)
INSERT INTO "ProductImages" ("ProductId", "ImageUrl", "SortOrder", "IsPrimary")
SELECT "ProductId", "ImageUrl", "SortOrder", "IsPrimary"
FROM image_data i
WHERE EXISTS (
		SELECT 1
		FROM "Products" p
		WHERE p."Id" = i."ProductId"
)
ON CONFLICT ("ProductId", "SortOrder")
DO UPDATE SET
	"ImageUrl" = EXCLUDED."ImageUrl",
	"IsPrimary" = EXCLUDED."IsPrimary";

DELETE FROM "ProductImages"
WHERE "ProductId" IN (1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,26,27,28)
	AND "SortOrder" NOT IN (1,2,3);

SELECT setval('"ProductImages_Id_seq"', COALESCE((SELECT MAX("Id") FROM "ProductImages"), 1), TRUE);
SELECT "ProductId", COUNT(*) AS "ImageCount"
FROM "ProductImages"
WHERE "ProductId" IN (1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,26,27,28)
GROUP BY "ProductId"
ORDER BY "ProductId";