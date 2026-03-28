-- ===== CATEGORIES =====
-- Coffee Shop Categories
-- Edit by: [Your Name]
-- Last modified: [Date]

INSERT INTO "Categories" ("Id", "Name", "IconGlyph", "Description") VALUES
(1, 'Coffee', '☕', 'Coffee drinks - Espresso, Latte, Cappuccino, etc.'),
(2, 'Tea', '🍵', 'Tea beverages - Green Tea, Black Tea, Jasmine, etc.'),
(3, 'Cake & Pastry', '🍰', 'Cakes, pastries, and desserts'),
(4, 'Snacks', '🥐', 'Sandwiches, bagels, and light snacks')
ON CONFLICT DO NOTHING;

SELECT setval('"Categories_Id_seq"', (SELECT MAX("Id") FROM "Categories") + 1);
SELECT COUNT(*) as "Categories_Count" FROM "Categories";
