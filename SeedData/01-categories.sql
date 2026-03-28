-- ===== CATEGORIES =====
-- Coffee Shop Categories
-- Edit by: [Your Name]
-- Last modified: [Date]

INSERT INTO "Categories"
    ("Id", "Name", "IconGlyph", "Description")
VALUES
    (1, 'Coffee', '☕', 'Coffee drinks - Espresso, Latte, Cappuccino, Americano, and more'),
    (2, 'Tea', '🍵', 'Tea beverages - Green Tea, Black Tea, Jasmine, Oolong, and herbal blends'),
    (3, 'Cake & Pastry', '🍰', 'Delicious cakes, croissants, pastries, and desserts'),
    (4, 'Snacks', '🥐', 'Sandwiches, bagels, cookies, and light snacks')
ON CONFLICT DO NOTHING;

SELECT setval('"Categories_Id_seq"', (SELECT MAX("Id")
    FROM "Categories") + 1);
SELECT COUNT(*) as "Categories_Count"
FROM "Categories";
