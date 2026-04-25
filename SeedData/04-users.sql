-- ===== USERS (Staff) =====
-- Coffee Shop Staff Members
-- Edit by: [Your Name]
-- Last modified: [Date]

INSERT INTO "Users"
    ("Id", "Username", "FullName", "PasswordHash", "HourlyRate", "Role", "IsActive")
VALUES
    (1, 'manager', 'Trương Quản Lý', 'sha256_hashed_password_1', 50000, '1', true),
    (2, 'barista1', 'Đoàn Tuấn Barista 1', 'sha256_hashed_password_2', 35000, '2', true),
    (3, 'barista2', 'Ngô Minh Barista 2', 'sha256_hashed_password_3', 35000, '2', true),
    (4, 'barista3', 'Lương Nhật Barista 3', 'sha256_hashed_password_4', 35000, '2', true),
    (5, 'cashier', 'Võ Thu Ngân', 'sha256_hashed_password_5', 30000, '2', true)
ON CONFLICT DO NOTHING;

SELECT setval('"Users_Id_seq"', (SELECT MAX("Id")
    FROM "Users") + 1);
SELECT COUNT(*) as "Users_Count"
FROM "Users";
