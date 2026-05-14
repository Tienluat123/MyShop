-- ===== PROMOTIONS =====
-- Coffee Shop Promotions & Discounts
-- Edit by: PhamVanMinhBinhThuan
-- Updated: 2026-05-14
-- Dữ liệu mẫu bao quát 4 trạng thái: Active, Expired, Upcoming, Inactive

INSERT INTO "Promotions"
    ("Id", "PromoCode", "PromoType", "DiscountValue", "MinOrderAmount", "StartDate", "EndDate", "TargetAudience", "IsActive")
VALUES
    -- 1. ACTIVE PROMOTIONS (IsActive = true, Start <= NOW <= End)
    (1, 'MORNING10', 'Percent', 10.00, 50000, NOW() - INTERVAL '5 days', NOW() + INTERVAL '30 days', 'All', true),
    (2, 'VIP25', 'Percent', 25.00, 150000, NOW() - INTERVAL '10 days', NOW() + INTERVAL '10 days', 'VIP', true),
    (3, 'WELCOME30K', 'FixedAmount', 30000.00, 100000, NOW() - INTERVAL '2 days', NOW() + INTERVAL '60 days', 'NewCustomer', true),
    (4, 'LOYALTY15', 'Percent', 15.00, 80000, NOW() - INTERVAL '1 day', NOW() + INTERVAL '15 days', 'Loyalty', true),

    -- 2. EXPIRED PROMOTIONS (End < NOW)
    (7, 'NEWYEAR2026', 'Percent', 20.00, 200000, NOW() - INTERVAL '100 days', NOW() - INTERVAL '60 days', 'All', true),
    (8, 'TET60K', 'FixedAmount', 60000.00, 300000, NOW() - INTERVAL '70 days', NOW() - INTERVAL '30 days', 'VIP', true),
    (9, 'FLASH20OLD', 'Percent', 20.00, 120000, NOW() - INTERVAL '20 days', NOW() - INTERVAL '5 days', 'All', true),

    -- 3. UPCOMING PROMOTIONS (IsActive = true, Start > NOW)
    (10, 'SUMMER_SALE', 'Percent', 15.00, 90000, NOW() + INTERVAL '5 days', NOW() + INTERVAL '40 days', 'All', true),
    (11, 'SCHOOL40K', 'FixedAmount', 40000.00, 180000, NOW() + INTERVAL '15 days', NOW() + INTERVAL '45 days', 'NewCustomer', true),
    (12, 'MIDAUTUMN', 'Percent', 30.00, 300000, NOW() + INTERVAL '30 days', NOW() + INTERVAL '60 days', 'All', true),

    -- 4. INACTIVE PROMOTIONS (IsActive = false)
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

-- Cập nhật lại sequence của ID để tránh lỗi khi thêm mới từ giao diện
SELECT setval('"Promotions_Id_seq"', COALESCE((SELECT MAX("Id") FROM "Promotions"), 1), TRUE);

SELECT COUNT(*) AS "Total_Promotions" FROM "Promotions";
