-- ===== CUSTOMERS =====
-- Coffee Shop Customers
-- Edit by: [Your Name]
-- Last modified: [Date]

INSERT INTO "Customers" ("Id", "Name", "Phone", "Email", "Address", "Gender", "Status", "Points", "Tier") VALUES
(1, 'Nguyễn Văn A', '0912345678', 'nguyenvana@email.com', '123 Nguyễn Huệ, HCM', 'Male', 'Active', 100, 1),
(2, 'Trần Thị B', '0987654321', 'tranthib@email.com', '456 Lê Lợi, HCM', 'Female', 'Active', 200, 1),
(3, 'Phạm Văn C', '0901234567', 'phamvanc@email.com', '789 Tran Hung Dao, HCM', 'Male', 'Active', 500, 2),
(4, 'Lê Thị D', '0945678901', 'lethid@email.com', '321 Hoàng Văn Thụ, HCM', 'Female', 'Active', 150, 1),
(5, 'Hoàng Văn E', '0923456789', 'hoangvane@email.com', '654 Cư Xá Phú Lâm, HCM', 'Male', 'Active', 800, 3),
(6, 'Vũ Thị F', '0934567890', 'vuthif@email.com', '987 Khánh Hội, HCM', 'Female', 'Inactive', 50, 0)
ON CONFLICT DO NOTHING;

SELECT setval('"Customers_Id_seq"', (SELECT MAX("Id") FROM "Customers") + 1);
SELECT COUNT(*) as "Customers_Count" FROM "Customers";
