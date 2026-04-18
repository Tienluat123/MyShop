-- ===== CUSTOMERS =====
-- Coffee Shop Customers
-- Edit by: PhamVanMinhBinhThuan
-- Last modified: 2026-04-09

INSERT INTO "Customers"
    ("Id", "Name", "Phone", "Email", "Address", "Gender", "Status", "TotalAccumulatedPoints", "CurrentAvailablePoints", "Tier", "TotalVisits", "TotalSpent", "CreatedAt")
VALUES
    (1, 'Nguyễn Văn A', '0912345678', 'nguyenvana@email.com', '123 Nguyễn Huệ, HCM', 'Male', 'Active', 100, 100, 1, 8, 250000, NOW()),
    (2, 'Trần Thị B', '0987654321', 'tranthib@email.com', '456 Lê Lợi, HCM', 'Female', 'Active', 200, 200, 1, 15, 450000, NOW()),
    (3, 'Phạm Văn C', '0901234567', 'phamvanc@email.com', '789 Tran Hung Dao, HCM', 'Male', 'Active', 500, 500, 2, 25, 950000, NOW()),
    (4, 'Lê Thị D', '0945678901', 'lethid@email.com', '321 Hoàng Văn Thụ, HCM', 'Female', 'Active', 150, 150, 1, 10, 350000, NOW()),
    (5, 'Hoàng Văn E', '0923456789', 'hoangvane@email.com', '654 Cư Xá Phú Lâm, HCM', 'Male', 'Active', 800, 800, 3, 50, 1500000, NOW()),
    (6, 'Vũ Thị F', '0934567890', 'vuthif@email.com', '987 Khánh Hội, HCM', 'Female', 'Inactive', 50, 50, 0, 3, 95000, NOW())
ON CONFLICT DO NOTHING;

SELECT setval('"Customers_Id_seq"', (SELECT MAX("Id")
    FROM "Customers") + 1);
SELECT COUNT(*) as "Customers_Count"
FROM "Customers";
