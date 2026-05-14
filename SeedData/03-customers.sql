-- ===== CUSTOMERS =====
-- Coffee Shop Customers
-- Edit by: PhamVanMinhBinhThuan
-- Last modified: 2026-05-14

INSERT INTO "Customers"
    ("Id", "Name", "Phone", "Email", "Address", "Gender", "Birthday", "Notes", "Status", "TotalAccumulatedPoints", "CurrentAvailablePoints", "Tier", "TotalVisits", "TotalSpent", "LastVisitDate", "CreatedAt")
VALUES
    (1, 'Nguyễn Văn A', '0912345678', 'nguyenvana@email.com', '123 Nguyễn Huệ, HCM', 'Male', '1995-02-12', 'Prefers less sugar', 'Active', 1200, 800, 1, 8, 250000, NOW() - INTERVAL '2 days', NOW() - INTERVAL '90 days'),
    (2, 'Trần Thị B', '0987654321', 'tranthib@email.com', '456 Lê Lợi, HCM', 'Female', '1998-07-04', 'Likes latte', 'Active', 5800, 3500, 1, 15, 450000, NOW() - INTERVAL '1 day', NOW() - INTERVAL '75 days'),
    (3, 'Phạm Văn C', '0901234567', 'phamvanc@email.com', '789 Tran Hung Dao, HCM', 'Male', '1992-11-20', 'Gold member', 'Active', 16000, 12000, 2, 25, 950000, NOW() - INTERVAL '3 hours', NOW() - INTERVAL '120 days'),
    (4, 'Lê Thị D', '0945678901', 'lethid@email.com', '321 Hoàng Văn Thụ, HCM', 'Female', '2000-03-18', 'Takeaway often', 'Active', 4500, 2000, 1, 10, 350000, NOW() - INTERVAL '5 days', NOW() - INTERVAL '60 days'),
    (5, 'Hoàng Văn E', '0923456789', 'hoangvane@email.com', '654 Cư Xá Phú Lâm, HCM', 'Male', '1989-09-30', 'Platinum customer', 'Active', 35000, 28000, 3, 50, 1500000, NOW() - INTERVAL '1 hour', NOW() - INTERVAL '180 days'),
    (6, 'Vũ Thị F', '0934567890', 'vuthif@email.com', '987 Khánh Hội, HCM', 'Female', '1997-05-25', 'Inactive demo customer', 'Inactive', 200, 50, 0, 3, 95000, NOW() - INTERVAL '45 days', NOW() - INTERVAL '100 days')
ON CONFLICT ("Id") DO UPDATE SET
    "Name" = EXCLUDED."Name",
    "Phone" = EXCLUDED."Phone",
    "Email" = EXCLUDED."Email",
    "Address" = EXCLUDED."Address",
    "Gender" = EXCLUDED."Gender",
    "Birthday" = EXCLUDED."Birthday",
    "Notes" = EXCLUDED."Notes",
    "Status" = EXCLUDED."Status",
    "TotalAccumulatedPoints" = EXCLUDED."TotalAccumulatedPoints",
    "CurrentAvailablePoints" = EXCLUDED."CurrentAvailablePoints",
    "Tier" = EXCLUDED."Tier",
    "TotalVisits" = EXCLUDED."TotalVisits",
    "TotalSpent" = EXCLUDED."TotalSpent",
    "LastVisitDate" = EXCLUDED."LastVisitDate";

SELECT setval('"Customers_Id_seq"', COALESCE((SELECT MAX("Id")
    FROM "Customers"), 1), TRUE);
SELECT COUNT(*) as "Customers_Count"
FROM "Customers";
