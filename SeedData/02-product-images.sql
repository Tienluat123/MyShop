-- ===== PRODUCT IMAGES =====
-- Sample product gallery data (primary + secondary images)

INSERT INTO "ProductImages" ("ProductId", "ImageUrl", "SortOrder", "IsPrimary") VALUES
-- Coffee
(1, 'https://picsum.photos/seed/prod-1-main/1200/800', 1, TRUE),
(1, 'https://picsum.photos/seed/prod-1-side/1200/800', 2, FALSE),
(2, 'https://picsum.photos/seed/prod-2-main/1200/800', 1, TRUE),
(2, 'https://picsum.photos/seed/prod-2-side/1200/800', 2, FALSE),
(3, 'https://picsum.photos/seed/prod-3-main/1200/800', 1, TRUE),
(3, 'https://picsum.photos/seed/prod-3-side/1200/800', 2, FALSE),
(4, 'https://picsum.photos/seed/prod-4-main/1200/800', 1, TRUE),
(4, 'https://picsum.photos/seed/prod-4-side/1200/800', 2, FALSE),
(5, 'https://picsum.photos/seed/prod-5-main/1200/800', 1, TRUE),
(5, 'https://picsum.photos/seed/prod-5-side/1200/800', 2, FALSE),
(6, 'https://picsum.photos/seed/prod-6-main/1200/800', 1, TRUE),
(6, 'https://picsum.photos/seed/prod-6-side/1200/800', 2, FALSE),
(7, 'https://picsum.photos/seed/prod-7-main/1200/800', 1, TRUE),
(7, 'https://picsum.photos/seed/prod-7-side/1200/800', 2, FALSE),
(8, 'https://picsum.photos/seed/prod-8-main/1200/800', 1, TRUE),
(8, 'https://picsum.photos/seed/prod-8-side/1200/800', 2, FALSE),

-- Tea
(9, 'https://picsum.photos/seed/prod-9-main/1200/800', 1, TRUE),
(10, 'https://picsum.photos/seed/prod-10-main/1200/800', 1, TRUE),
(11, 'https://picsum.photos/seed/prod-11-main/1200/800', 1, TRUE),
(12, 'https://picsum.photos/seed/prod-12-main/1200/800', 1, TRUE),
(13, 'https://picsum.photos/seed/prod-13-main/1200/800', 1, TRUE),

-- Cake & Pastry
(14, 'https://picsum.photos/seed/prod-14-main/1200/800', 1, TRUE),
(15, 'https://picsum.photos/seed/prod-15-main/1200/800', 1, TRUE),
(16, 'https://picsum.photos/seed/prod-16-main/1200/800', 1, TRUE),
(17, 'https://picsum.photos/seed/prod-17-main/1200/800', 1, TRUE),
(18, 'https://picsum.photos/seed/prod-18-main/1200/800', 1, TRUE),
(19, 'https://picsum.photos/seed/prod-19-main/1200/800', 1, TRUE),

-- Snacks
(20, 'https://picsum.photos/seed/prod-20-main/1200/800', 1, TRUE),
(21, 'https://picsum.photos/seed/prod-21-main/1200/800', 1, TRUE),
(22, 'https://picsum.photos/seed/prod-22-main/1200/800', 1, TRUE),
(23, 'https://picsum.photos/seed/prod-23-main/1200/800', 1, TRUE)
ON CONFLICT DO NOTHING;

SELECT setval('"ProductImages_Id_seq"', COALESCE((SELECT MAX("Id") FROM "ProductImages"), 1), TRUE);
SELECT COUNT(*) as "ProductImages_Count" FROM "ProductImages";