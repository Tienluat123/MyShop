-- ===== ORDERS & ORDER DETAILS =====
-- Sample Coffee Shop Transactions - Dual Status Schema
-- Last modified: 2026-04-04

-- ORDERS
INSERT INTO "Orders" ("Id", "CustomerId", "UserId", "OrderDate", "SubTotal", "DiscountAmount", "TotalAmount", "PaymentStatus", "FulfillmentStatus") VALUES
-- Legacy Migration
(1, 1, 2, NOW() - INTERVAL '5 days', 95000, 0, 95000, 'Paid', 'Completed'),
(2, 2, 3, NOW() - INTERVAL '3 days', 180000, 15000, 165000, 'Paid', 'Completed'),
(3, 3, 2, NOW() - INTERVAL '1 day', 290000, 0, 290000, 'Paid', 'Completed'),
(4, 4, 5, NOW(), 125000, 0, 125000, 'Paid', 'Completed'),
(5, 5, 2, NOW() - INTERVAL '2 hours', 180000, 30000, 150000, 'Paid', 'Completed'),

-- 20 ACTIVE ORDERS (None, Preparing, Ready)
(6, 1, 2, NOW() - INTERVAL '10 minutes', 45000, 0, 45000, 'PendingPayment', 'None'),
(7, 2, 2, NOW() - INTERVAL '15 minutes', 55000, 0, 55000, 'PendingPayment', 'None'),
(8, 3, 2, NOW() - INTERVAL '20 minutes', 30000, 0, 30000, 'PendingPayment', 'None'),
(9, 4, 2, NOW() - INTERVAL '25 minutes', 65000, 5000, 60000, 'Paid', 'None'),
(10, 5, 2, NOW() - INTERVAL '30 minutes', 40000, 0, 40000, 'PendingPayment', 'None'),
(11, 1, 3, NOW() - INTERVAL '35 minutes', 85000, 0, 85000, 'PendingPayment', 'Preparing'),
(12, 2, 3, NOW() - INTERVAL '40 minutes', 95000, 10000, 85000, 'Paid', 'Preparing'),
(13, 3, 3, NOW() - INTERVAL '45 minutes', 120000, 0, 120000, 'PendingPayment', 'Preparing'),
(14, 4, 3, NOW() - INTERVAL '50 minutes', 35000, 0, 35000, 'PendingPayment', 'Preparing'),
(15, 5, 3, NOW() - INTERVAL '55 minutes', 50000, 0, 50000, 'Paid', 'Preparing'),
(16, 1, 5, NOW() - INTERVAL '60 minutes', 60000, 0, 60000, 'Paid', 'Preparing'),
(17, 2, 5, NOW() - INTERVAL '65 minutes', 75000, 0, 75000, 'PendingPayment', 'Preparing'),
(18, 3, 5, NOW() - INTERVAL '70 minutes', 45000, 0, 45000, 'Paid', 'Ready'),
(19, 4, 5, NOW() - INTERVAL '75 minutes', 55000, 0, 55000, 'Paid', 'Ready'),
(20, 5, 5, NOW() - INTERVAL '80 minutes', 30000, 0, 30000, 'Paid', 'Ready'),
(21, 1, 2, NOW() - INTERVAL '85 minutes', 65000, 5000, 60000, 'Paid', 'Ready'),
(22, 2, 2, NOW() - INTERVAL '90 minutes', 40000, 0, 40000, 'Paid', 'Ready'),
(23, 3, 2, NOW() - INTERVAL '95 minutes', 85000, 0, 85000, 'Paid', 'Ready'),
(24, 4, 2, NOW() - INTERVAL '100 minutes', 95000, 10000, 85000, 'Paid', 'Ready'),
(25, 5, 2, NOW() - INTERVAL '105 minutes', 120000, 0, 120000, 'Paid', 'Ready'),

-- 20 HISTORY ORDERS (Paid, Cancelled, Refunded + Completed)
(26, 1, 3, NOW() - INTERVAL '2 hours', 45000, 0, 45000, 'Paid', 'Completed'),
(27, 2, 3, NOW() - INTERVAL '3 hours', 55000, 0, 55000, 'Paid', 'Completed'),
(28, 3, 3, NOW() - INTERVAL '4 hours', 30000, 0, 30000, 'Paid', 'Completed'),
(29, 4, 3, NOW() - INTERVAL '5 hours', 65000, 5000, 60000, 'Paid', 'Completed'),
(30, 5, 3, NOW() - INTERVAL '6 hours', 40000, 0, 40000, 'Paid', 'Completed'),
(31, 1, 5, NOW() - INTERVAL '7 hours', 85000, 0, 85000, 'Paid', 'Completed'),
(32, 2, 5, NOW() - INTERVAL '8 hours', 95000, 10000, 85000, 'Paid', 'Completed'),
(33, 3, 5, NOW() - INTERVAL '1 day', 120000, 0, 120000, 'Paid', 'Completed'),
(34, 4, 5, NOW() - INTERVAL '1 day', 35000, 0, 35000, 'Paid', 'Completed'),
(35, 5, 5, NOW() - INTERVAL '1 day', 50000, 0, 50000, 'Paid', 'Completed'),
(36, 1, 2, NOW() - INTERVAL '2 days', 60000, 0, 60000, 'Cancelled', 'Completed'),
(37, 2, 2, NOW() - INTERVAL '2 days', 75000, 0, 75000, 'Cancelled', 'Completed'),
(38, 3, 2, NOW() - INTERVAL '3 days', 45000, 0, 45000, 'Cancelled', 'Completed'),
(39, 4, 2, NOW() - INTERVAL '3 days', 55000, 0, 55000, 'Refunded', 'Completed'),
(40, 5, 2, NOW() - INTERVAL '4 days', 30000, 0, 30000, 'Refunded', 'Completed'),
(41, 1, 3, NOW() - INTERVAL '4 days', 65000, 5000, 60000, 'Paid', 'Completed'),
(42, 2, 3, NOW() - INTERVAL '5 days', 40000, 0, 40000, 'Paid', 'Completed'),
(43, 3, 3, NOW() - INTERVAL '6 days', 85000, 0, 85000, 'Paid', 'Completed'),
(44, 4, 3, NOW() - INTERVAL '7 days', 95000, 10000, 85000, 'Paid', 'Completed'),
(45, 5, 3, NOW() - INTERVAL '8 days', 120000, 0, 120000, 'Paid', 'Completed')
ON CONFLICT DO NOTHING;

-- ORDER DETAILS (Approx 2 items per order for the new 40 orders)
-- Orders 6-45
INSERT INTO "OrderDetails" ("OrderId", "ProductId", "Quantity", "UnitPrice") VALUES
(6, 1, 1, 30000), (6, 2, 1, 15000),
(7, 3, 1, 25000), (7, 4, 1, 30000),
(8, 5, 1, 30000),
(9, 6, 2, 32500),
(10, 7, 1, 40000),
(11, 8, 2, 42500),
(12, 9, 1, 55000), (12, 1, 1, 40000),
(13, 10, 3, 40000),
(14, 11, 1, 35000),
(15, 12, 2, 25000),
(16, 13, 1, 60000),
(17, 14, 1, 75000),
(18, 15, 1, 45000),
(19, 16, 1, 55000),
(20, 17, 1, 30000),
(21, 18, 2, 32500),
(22, 19, 1, 40000),
(23, 20, 2, 42500),
(24, 21, 1, 55000), (24, 2, 1, 40000),
(25, 1, 3, 40000),
(26, 2, 1, 45000),
(27, 3, 1, 55000),
(28, 4, 1, 30000),
(29, 5, 2, 32500),
(30, 6, 1, 40000),
(31, 7, 2, 42500),
(32, 8, 1, 55000), (32, 9, 1, 40000),
(33, 10, 3, 40000),
(34, 11, 1, 35000),
(35, 12, 2, 25000),
(36, 13, 1, 60000),
(37, 14, 1, 75000),
(38, 15, 1, 45000),
(39, 16, 1, 55000),
(40, 17, 1, 30000),
(41, 18, 2, 32500),
(42, 19, 1, 40000),
(43, 20, 2, 42500),
(44, 21, 1, 55000), (44, 1, 1, 40000),
(45, 2, 3, 40000)
ON CONFLICT DO NOTHING;

-- Legacy Order Details (Matching IDs 1-5 already in file)
-- IDs 1-13 already exist. New ones start from 14.

SELECT setval('"Orders_Id_seq"', (SELECT MAX("Id") FROM "Orders") + 1);
SELECT setval('"OrderDetails_Id_seq"', (SELECT MAX("Id") FROM "OrderDetails") + 1);
SELECT COUNT(*) as "Orders_Count" FROM "Orders";
SELECT COUNT(*) as "OrderDetails_Count" FROM "OrderDetails";
