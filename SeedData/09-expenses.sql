-- ===== OPERATING EXPENSES (RECALIBRATED) =====
-- Scaled overhead to match ~4M monthly revenue
-- Ensures positive net profit for testing
-- Last modified: 2026-04-27

INSERT INTO "Expenses" ("Id", "Title", "Amount", "Date", "Category", "IsEditing") VALUES
-- Monthly Recurring (Jan - March - Historical reference)
(1, 'Shop Rent - Jan', 500000, NOW() - INTERVAL '100 days', 'Rent', false),
(2, 'Electricity - Jan', 100000, NOW() - INTERVAL '95 days', 'Utilities', false),
(3, 'Salaries - Jan', 800000, NOW() - INTERVAL '90 days', 'Salaries', false),

(4, 'Shop Rent - Feb', 500000, NOW() - INTERVAL '70 days', 'Rent', false),
(5, 'Electricity - Feb', 100000, NOW() - INTERVAL '65 days', 'Utilities', false),
(6, 'Salaries - Feb', 800000, NOW() - INTERVAL '60 days', 'Salaries', false),

(7, 'Shop Rent - March', 500000, NOW() - INTERVAL '40 days', 'Rent', false),
(8, 'Electricity - March', 110000, NOW() - INTERVAL '35 days', 'Utilities', false),
(9, 'Salaries - March', 800000, NOW() - INTERVAL '30 days', 'Salaries', false),

-- APRIL SPLIT (Starts from 1/4)
(10, 'Rent (First half April)', 250000, NOW() - INTERVAL '26 days', 'Rent', false),
(11, 'Rent (Second half April)', 250000, NOW() - INTERVAL '5 days', 'Rent', false),
(12, 'Mid-month Salaries', 400000, NOW() - INTERVAL '12 days', 'Salaries', false),
(13, 'End-month Salaries', 400000, NOW() - INTERVAL '1 hour', 'Salaries', false),
(14, 'Electricity - April', 150000, NOW() - INTERVAL '2 days', 'Utilities', false),

-- Inventory Batches (Scaled down)
(100, 'Coffee Beans Batch #1', 250000, NOW() - INTERVAL '85 days', 'Batch', false),
(101, 'Milk & Syrups Weekly', 100000, NOW() - INTERVAL '80 days', 'Batch', false),
(102, 'Coffee Beans Batch #2', 250000, NOW() - INTERVAL '55 days', 'Batch', false),
(103, 'Pastry Supplies', 150000, NOW() - INTERVAL '32 days', 'Batch', false),
(104, 'Coffee Beans Batch #3 (Premium)', 300000, NOW() - INTERVAL '15 days', 'Batch', false),

-- Marketing
(200, 'Facebook Ads - March', 200000, NOW() - INTERVAL '38 days', 'Marketing', false),
(201, 'Local Flyers', 150000, NOW() - INTERVAL '12 days', 'Marketing', false),

-- Other
(300, 'Machine Maintenance', 100000, NOW() - INTERVAL '45 days', 'Other', false),
(301, 'Cleaning Supplies', 35000, NOW() - INTERVAL '2 days', 'Other', false)

ON CONFLICT ("Id") DO UPDATE SET
    "Title" = EXCLUDED."Title",
    "Amount" = EXCLUDED."Amount",
    "Date" = EXCLUDED."Date",
    "Category" = EXCLUDED."Category",
    "IsEditing" = EXCLUDED."IsEditing";
