-- ===== ORDERS & ORDER DETAILS =====
-- Sample Coffee Shop Transactions
-- Edit by: [Your Name]
-- Last modified: [Date]

-- ORDERS
INSERT INTO "Orders" ("Id", "CustomerId", "UserId", "OrderDate", "SubTotal", "DiscountAmount", "TotalAmount", "Status") VALUES
(1, 1, 2, NOW() - INTERVAL '5 days', 95000, 0, 95000, 'Paid'),
(2, 2, 3, NOW() - INTERVAL '3 days', 180000, 15000, 165000, 'Paid'),
(3, 3, 2, NOW() - INTERVAL '1 day', 290000, 0, 290000, 'Paid'),
(4, 4, 5, NOW(), 125000, 0, 125000, 'Paid'),
(5, 5, 2, NOW() - INTERVAL '2 hours', 180000, 30000, 150000, 'Paid')
ON CONFLICT DO NOTHING;

-- ORDER DETAILS
INSERT INTO "OrderDetails" ("Id", "OrderId", "ProductId", "Quantity", "UnitPrice") VALUES
-- Order 1 (Customer A)
(1, 1, 2, 2, 45000),
(2, 1, 14, 1, 85000),

-- Order 2 (Customer B)
(3, 2, 3, 2, 50000),
(4, 2, 17, 2, 45000),

-- Order 3 (Customer C - VIP)
(5, 3, 5, 1, 55000),
(6, 3, 20, 1, 65000),
(7, 3, 15, 1, 95000),

-- Order 4 (Customer D)
(8, 4, 1, 1, 30000),
(9, 4, 21, 1, 60000),
(10, 4, 18, 1, 40000),

-- Order 5 (Customer E - VIP)
(11, 5, 2, 2, 45000),
(12, 5, 9, 1, 30000),
(13, 5, 16, 1, 75000)
ON CONFLICT DO NOTHING;

SELECT setval('"Orders_Id_seq"', (SELECT MAX("Id") FROM "Orders") + 1);
SELECT setval('"OrderDetails_Id_seq"', (SELECT MAX("Id") FROM "OrderDetails") + 1);
SELECT COUNT(*) as "Orders_Count" FROM "Orders";
SELECT COUNT(*) as "OrderDetails_Count" FROM "OrderDetails";
