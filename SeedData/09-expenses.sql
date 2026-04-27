-- ===== OPERATING EXPENSES =====
-- Sample overhead costs synchronized with orders range
-- Last modified: 2026-04-27

INSERT INTO "Expenses" ("Id", "Title", "Amount", "Date", "Category", "IsEditing") VALUES
-- Monthly Recurring (Jan - April)
(1, 'Shop Rent - January', 15000000, NOW() - INTERVAL '100 days', 'Rent', false),
(2, 'Electricity - January', 1200000, NOW() - INTERVAL '95 days', 'Utilities', false),
(3, 'Salaries - January', 25000000, NOW() - INTERVAL '90 days', 'Salaries', false),

(4, 'Shop Rent - February', 15000000, NOW() - INTERVAL '70 days', 'Rent', false),
(5, 'Electricity - February', 1350000, NOW() - INTERVAL '65 days', 'Utilities', false),
(6, 'Salaries - February', 25000000, NOW() - INTERVAL '60 days', 'Salaries', false),

(7, 'Shop Rent - March', 15000000, NOW() - INTERVAL '40 days', 'Rent', false),
(8, 'Electricity - March', 1100000, NOW() - INTERVAL '35 days', 'Utilities', false),
(9, 'Salaries - March', 25000000, NOW() - INTERVAL '30 days', 'Salaries', false),

(10, 'Shop Rent - April', 15000000, NOW() - INTERVAL '5 days', 'Rent', false),
(11, 'Salaries - April', 25000000, NOW() - INTERVAL '1 hour', 'Salaries', false),

-- Inventory Batches (Ad-hoc)
(100, 'Coffee Beans Batch #1', 5000000, NOW() - INTERVAL '85 days', 'Batch', false),
(101, 'Milk & Syrups Weekly', 1200000, NOW() - INTERVAL '80 days', 'Batch', false),
(102, 'Coffee Beans Batch #2', 5000000, NOW() - INTERVAL '55 days', 'Batch', false),
(103, 'Pastry Supplies', 2500000, NOW() - INTERVAL '32 days', 'Batch', false),
(104, 'Coffee Beans Batch #3 (Premium)', 7500000, NOW() - INTERVAL '15 days', 'Batch', false),

-- Marketing
(200, 'Facebook Ads - March', 3000000, NOW() - INTERVAL '38 days', 'Marketing', false),
(201, 'New Signage', 4500000, NOW() - INTERVAL '12 days', 'Marketing', false),

-- Other
(300, 'Machine Maintenance', 1500000, NOW() - INTERVAL '45 days', 'Other', false),
(301, 'Cleaning Supplies', 350000, NOW() - INTERVAL '2 days', 'Other', false)

ON CONFLICT ("Id") DO UPDATE SET
    "Title" = EXCLUDED."Title",
    "Amount" = EXCLUDED."Amount",
    "Date" = EXCLUDED."Date",
    "Category" = EXCLUDED."Category";
