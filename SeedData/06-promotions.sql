-- ===== PROMOTIONS =====
-- Coffee Shop Promotions & Discounts
-- Edit by: [Your Name]
-- Last modified: [Date]

INSERT INTO "Promotions"
    ("Id", "PromoCode", "PromoType", "DiscountValue", "MinOrderAmount", "StartDate", "EndDate", "TargetAudience", "IsActive")
VALUES
    (1, 'MORNING10', 'Percent', 10, 50000, NOW(), NOW() + INTERVAL
'90 days', 'All', true),
(2, 'BUY5GET1', 'FixedAmount', 35000, 0, NOW
(), NOW
() + INTERVAL '60 days', 'All', true),
(3, 'WELCOME25', 'FixedAmount', 25000, 0, NOW
(), NOW
() + INTERVAL '120 days', 'All', true),
(4, 'VIP20', 'Percent', 20, 100000, NOW
(), NOW
() + INTERVAL '90 days', 'VIP', true),
(5, 'LUNCH50', 'FixedAmount', 50000, 150000, NOW
(), NOW
() + INTERVAL '30 days', 'All', true)
ON CONFLICT DO NOTHING;

SELECT setval('"Promotions_Id_seq"', (SELECT MAX("Id")
    FROM "Promotions") + 1);
SELECT COUNT(*) as "Promotions_Count"
FROM "Promotions";
