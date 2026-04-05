-- ===== ORDERS & ORDER DETAILS =====
-- Sample Coffee Shop Transactions - Dual Status Schema
-- Last modified: 2026-04-04

-- ORDERS
INSERT INTO "Orders" ("Id", "CustomerId", "UserId", "OrderDate", "SubTotal", "DiscountAmount", "TotalAmount", "PaymentStatus", "FulfillmentStatus", "Type", "TableNumber", "Notes", "PromotionId", "TaxAmount", "ManualAdjustment", "ManualAdjustmentReason") VALUES
-- Legacy Migration
(1, 1, 2, NOW() - INTERVAL '5 days', 95000, 0, 104500, 'Paid', 'Completed', 'Dine-in', 'Table 4', '', NULL, 9500, 0, ''),
(2, 2, 3, NOW() - INTERVAL '3 days', 180000, 15000, 181500, 'Paid', 'Completed', 'Dine-in', 'Table 2', 'Extra sugar', NULL, 16500, 0, ''),
(3, 3, 2, NOW() - INTERVAL '1 day', 290000, 0, 319000, 'Paid', 'Completed', 'Takeaway', '', 'No plastic bag', NULL, 29000, 0, ''),
(4, 4, 5, NOW(), 125000, 0, 137500, 'Paid', 'Completed', 'Dine-in', 'Table 8', '', NULL, 12500, 0, ''),
(5, 5, 2, NOW() - INTERVAL '2 hours', 180000, 30000, 165000, 'Paid', 'Completed', 'Dine-in', 'Table 5', 'VIP Guest', NULL, 15000, 0, ''),

-- 20 ACTIVE ORDERS (None, Preparing, Ready)
(6, 1, 2, NOW() - INTERVAL '10 minutes', 45000, 0, 49500, 'PendingPayment', 'None', 'Dine-in', 'Table 1', '', NULL, 4500, 0, ''),
(7, 2, 2, NOW() - INTERVAL '15 minutes', 55000, 0, 60500, 'PendingPayment', 'None', 'Dine-in', 'Table 3', '', NULL, 5500, 0, ''),
(8, 3, 2, NOW() - INTERVAL '20 minutes', 30000, 0, 33000, 'PendingPayment', 'None', 'Takeaway', '', '', NULL, 3000, 0, ''),
(9, 4, 2, NOW() - INTERVAL '25 minutes', 65000, 5000, 66000, 'Paid', 'None', 'Dine-in', 'Table 7', 'Meeting', NULL, 6000, 0, ''),
(10, 5, 2, NOW() - INTERVAL '30 minutes', 40000, 0, 44000, 'PendingPayment', 'None', 'Dine-in', 'Table 2', '', NULL, 4000, 0, ''),
(11, 1, 3, NOW() - INTERVAL '35 minutes', 85000, 0, 93500, 'PendingPayment', 'Preparing', 'Dine-in', 'Table 4', '', NULL, 8500, 0, ''),
(12, 2, 3, NOW() - INTERVAL '40 minutes', 95000, 10000, 93500, 'Paid', 'Preparing', 'Dine-in', 'Table 5', '', NULL, 8500, 0, ''),
(13, 3, 3, NOW() - INTERVAL '45 minutes', 120000, 0, 132000, 'PendingPayment', 'Preparing', 'Dine-in', 'Table 9', '', NULL, 12000, 0, ''),
(14, 4, 3, NOW() - INTERVAL '50 minutes', 35000, 0, 38500, 'PendingPayment', 'Preparing', 'Takeaway', '', '', NULL, 3500, 0, ''),
(15, 5, 3, NOW() - INTERVAL '55 minutes', 50000, 0, 55000, 'Paid', 'Preparing', 'Dine-in', 'Table 1', '', NULL, 5000, 0, ''),
(16, 1, 5, NOW() - INTERVAL '60 minutes', 60000, 0, 66000, 'Paid', 'Preparing', 'Dine-in', 'Table 6', '', NULL, 6000, 0, ''),
(17, 2, 5, NOW() - INTERVAL '65 minutes', 75000, 0, 82500, 'PendingPayment', 'Preparing', 'Dine-in', 'Table 8', '', NULL, 7500, 0, ''),
(18, 3, 5, NOW() - INTERVAL '70 minutes', 45000, 0, 49500, 'Paid', 'Ready', 'Dine-in', 'Table 2', '', NULL, 4500, 0, ''),
(19, 4, 5, NOW() - INTERVAL '75 minutes', 55000, 0, 60500, 'Paid', 'Ready', 'Dine-in', 'Table 4', '', NULL, 5500, 0, ''),
(20, 5, 5, NOW() - INTERVAL '80 minutes', 30000, 0, 33000, 'Paid', 'Ready', 'Takeaway', '', '', NULL, 3000, 0, ''),
(21, 1, 2, NOW() - INTERVAL '85 minutes', 65000, 5000, 66000, 'Paid', 'Ready', 'Dine-in', 'Table 10', '', NULL, 6000, 0, ''),
(22, 2, 2, NOW() - INTERVAL '90 minutes', 40000, 0, 44000, 'Paid', 'Ready', 'Dine-in', 'Table 5', '', NULL, 4000, 0, ''),
(23, 3, 2, NOW() - INTERVAL '95 minutes', 85000, 0, 93500, 'Paid', 'Ready', 'Dine-in', 'Table 7', '', NULL, 8500, 0, ''),
(24, 4, 2, NOW() - INTERVAL '100 minutes', 95000, 10000, 93500, 'Paid', 'Ready', 'Takeaway', '', '', NULL, 8500, 0, ''),
(25, 5, 2, NOW() - INTERVAL '105 minutes', 120000, 0, 132000, 'Paid', 'Ready', 'Dine-in', 'Table 3', '', NULL, 12000, 0, ''),

-- 20 HISTORY ORDERS (Paid, Cancelled, Refunded + Completed)
(26, 1, 3, NOW() - INTERVAL '2 hours', 45000, 0, 49500, 'Paid', 'Completed', 'Dine-in', 'Table 1', '', NULL, 4500, 0, ''),
(27, 2, 3, NOW() - INTERVAL '3 hours', 55000, 0, 60500, 'Paid', 'Completed', 'Dine-in', 'Table 2', '', NULL, 5500, 0, ''),
(28, 3, 3, NOW() - INTERVAL '4 hours', 30000, 0, 33000, 'Paid', 'Completed', 'Takeaway', '', '', NULL, 3000, 0, ''),
(29, 4, 3, NOW() - INTERVAL '5 hours', 65000, 5000, 66000, 'Paid', 'Completed', 'Dine-in', 'Table 6', '', NULL, 6000, 0, ''),
(30, 5, 3, NOW() - INTERVAL '6 hours', 40000, 0, 44000, 'Paid', 'Completed', 'Dine-in', 'Table 4', '', NULL, 4000, 0, ''),
(31, 1, 5, NOW() - INTERVAL '7 hours', 85000, 0, 93500, 'Paid', 'Completed', 'Dine-in', 'Table 5', '', NULL, 8500, 0, ''),
(32, 2, 5, NOW() - INTERVAL '8 hours', 95000, 10000, 93500, 'Paid', 'Completed', 'Dine-in', 'Table 8', '', NULL, 8500, 0, ''),
(33, 3, 5, NOW() - INTERVAL '1 day', 120000, 0, 132000, 'Paid', 'Completed', 'Dine-in', 'Table 1', '', NULL, 12000, 0, ''),
(34, 4, 5, NOW() - INTERVAL '1 day', 35000, 0, 38500, 'Paid', 'Completed', 'Takeaway', '', '', NULL, 3500, 0, ''),
(35, 5, 5, NOW() - INTERVAL '1 day', 50000, 0, 55000, 'Paid', 'Completed', 'Dine-in', 'Table 2', '', NULL, 5000, 0, ''),
(36, 1, 2, NOW() - INTERVAL '2 days', 60000, 0, 66000, 'Cancelled', 'Completed', 'Dine-in', 'Table 3', 'Customer left', NULL, 6000, 0, ''),
(37, 2, 2, NOW() - INTERVAL '2 days', 75000, 0, 82500, 'Cancelled', 'Completed', 'Dine-in', 'Table 5', 'Spilled coffee', NULL, 7500, 0, ''),
(38, 3, 2, NOW() - INTERVAL '3 days', 45000, 0, 49500, 'Cancelled', 'Completed', 'Takeaway', '', 'Changed mind', NULL, 4500, 0, ''),
(39, 4, 2, NOW() - INTERVAL '3 days', 55000, 0, 60500, 'Refunded', 'Completed', 'Dine-in', 'Table 1', 'Wrong item', NULL, 5500, 0, ''),
(40, 5, 2, NOW() - INTERVAL '4 days', 30000, 0, 33000, 'Refunded', 'Completed', 'Dine-in', 'Table 4', 'Tastes bad', NULL, 3000, 0, ''),
(41, 1, 3, NOW() - INTERVAL '4 days', 65000, 5000, 66000, 'Paid', 'Completed', 'Dine-in', 'Table 6', '', NULL, 6000, 0, ''),
(42, 2, 3, NOW() - INTERVAL '5 days', 40000, 0, 44000, 'Paid', 'Completed', 'Dine-in', 'Table 7', '', NULL, 4000, 0, ''),
(43, 3, 3, NOW() - INTERVAL '6 days', 85000, 0, 93500, 'Paid', 'Completed', 'Dine-in', 'Table 2', '', NULL, 8500, 0, ''),
(44, 4, 3, NOW() - INTERVAL '7 days', 95000, 10000, 93500, 'Paid', 'Completed', 'Takeaway', '', '', NULL, 8500, 0, ''),
(45, 5, 3, NOW() - INTERVAL '8 days', 120000, 0, 132000, 'Paid', 'Completed', 'Dine-in', 'Table 5', '', NULL, 12000, 0, '')
ON CONFLICT ("Id") DO UPDATE SET
    "CustomerId" = EXCLUDED."CustomerId",
    "UserId" = EXCLUDED."UserId",
    "OrderDate" = EXCLUDED."OrderDate",
    "SubTotal" = EXCLUDED."SubTotal",
    "DiscountAmount" = EXCLUDED."DiscountAmount",
    "TotalAmount" = EXCLUDED."TotalAmount",
    "PaymentStatus" = EXCLUDED."PaymentStatus",
    "FulfillmentStatus" = EXCLUDED."FulfillmentStatus",
    "Type" = EXCLUDED."Type",
    "TableNumber" = EXCLUDED."TableNumber",
    "Notes" = EXCLUDED."Notes",
    "PromotionId" = EXCLUDED."PromotionId",
    "TaxAmount" = EXCLUDED."TaxAmount",
    "ManualAdjustment" = EXCLUDED."ManualAdjustment",
    "ManualAdjustmentReason" = EXCLUDED."ManualAdjustmentReason";

-- ORDER DETAILS (Approx 2 items per order for the new 40 orders)
-- Orders 6-45
INSERT INTO "OrderDetails" ("Id", "OrderId", "ProductId", "Quantity", "UnitPrice", "Notes") VALUES
-- Legacy Migration Details (IDs 1-5)
(1, 1, 2, 2, 45000, NULL), (2, 1, 14, 1, 85000, NULL),
(3, 2, 3, 2, 50000, NULL), (4, 2, 17, 2, 45000, NULL),
(5, 3, 5, 1, 55000, NULL), (6, 3, 20, 1, 65000, NULL), (7, 3, 15, 1, 95000, NULL),
(8, 4, 1, 1, 30000, NULL), (9, 4, 21, 1, 60000, NULL), (10, 4, 18, 1, 40000, NULL),
(11, 5, 2, 2, 45000, NULL), (12, 5, 9, 1, 30000, NULL), (13, 5, 16, 1, 75000, NULL),

-- 20 ACTIVE ORDERS (None, Preparing, Ready) - Starting ID 14
(14, 6, 1, 1, 30000, NULL), (15, 6, 2, 1, 15000, NULL),
(16, 7, 3, 1, 25000, NULL), (17, 7, 4, 1, 30000, NULL),
(18, 8, 5, 1, 30000, NULL),
(19, 9, 6, 2, 32500, NULL),
(20, 10, 7, 1, 40000, NULL),
(21, 11, 8, 2, 42500, NULL),
(22, 12, 9, 1, 55000, NULL), (23, 12, 1, 1, 40000, NULL),
(24, 13, 10, 3, 40000, NULL),
(25, 14, 11, 1, 35000, NULL),
(26, 15, 12, 2, 25000, NULL),
(27, 16, 13, 1, 60000, NULL),
(28, 17, 14, 1, 75000, NULL),
(29, 18, 15, 1, 45000, NULL),
(30, 19, 16, 1, 55000, NULL),
(31, 20, 17, 1, 30000, NULL),
(32, 21, 18, 2, 32500, NULL),
(33, 22, 19, 1, 40000, NULL),
(34, 23, 20, 2, 42500, NULL),
(35, 24, 21, 1, 55000, NULL), (36, 24, 2, 1, 40000, NULL),
(37, 25, 1, 3, 40000, NULL),

-- 20 HISTORY ORDERS (Paid, Cancelled, Refunded + Completed) - Starting ID 38
(38, 26, 2, 1, 45000, NULL),
(39, 27, 3, 1, 55000, NULL),
(40, 28, 4, 1, 30000, NULL),
(41, 29, 5, 2, 32500, NULL),
(42, 30, 6, 1, 40000, NULL),
(43, 31, 7, 2, 42500, NULL),
(44, 32, 8, 1, 55000, NULL), (45, 32, 9, 1, 40000, NULL),
(46, 33, 10, 3, 40000, NULL),
(47, 34, 11, 1, 35000, NULL),
(48, 35, 12, 2, 25000, NULL),
(49, 36, 13, 1, 60000, NULL),
(50, 37, 14, 1, 75000, NULL),
(51, 38, 15, 1, 45000, NULL),
(52, 39, 16, 1, 55000, NULL),
(53, 40, 17, 1, 30000, NULL),
(54, 41, 18, 2, 32500, NULL),
(55, 42, 19, 1, 40000, NULL),
(56, 43, 20, 2, 42500, NULL),
(57, 44, 21, 1, 55000, NULL), (58, 44, 1, 1, 40000, NULL),
(59, 45, 2, 3, 40000, NULL)
ON CONFLICT ("Id") DO UPDATE SET
    "OrderId" = EXCLUDED."OrderId",
    "ProductId" = EXCLUDED."ProductId",
    "Quantity" = EXCLUDED."Quantity",
    "UnitPrice" = EXCLUDED."UnitPrice",
    "Notes" = EXCLUDED."Notes";


SELECT setval('"Orders_Id_seq"', (SELECT MAX("Id") FROM "Orders") + 1);
SELECT setval('"OrderDetails_Id_seq"', (SELECT MAX("Id") FROM "OrderDetails") + 1);
SELECT COUNT(*) as "Orders_Count" FROM "Orders";
SELECT COUNT(*) as "OrderDetails_Count" FROM "OrderDetails";