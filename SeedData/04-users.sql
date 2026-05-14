-- ===== USERS (Staff) =====
-- Coffee Shop Staff Members
-- Edit by: Nguyễn Tiến Luật
-- Last modified: 2026-05-14
-- Database: PostgreSQL
-- Default password for all seeded users: password


INSERT INTO "Users"
    ("Id", "Username", "FirstName", "LastName", "Phone", "PasswordHash", "HourlyRate", "Role", "IsActive")
VALUES
    (1, 'admin_luat', 'Nguyễn Tiến', 'Luật', '0912345001', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 100000, '1', true),
    (2, 'manager_minh', 'Phạm Văn', 'Minh', '0912345002', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 80000, '1', true),
    (3, 'barista_tuan', 'Trần Anh', 'Tuấn', '0912345003', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 35000, '2', true),
    (4, 'barista_hoa', 'Lê Thị', 'Hoa', '0912345004', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 35000, '2', true),
    (5, 'cashier_ngan', 'Hoàng Thu', 'Ngân', '0912345005', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 30000, '2', true),
    (6, 'staff_duy', 'Ngô Quang', 'Duy', '0912345006', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 25000, '2', true),
    (7, 'staff_vy', 'Đặng Thúy', 'Vy', '0912345007', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 25000, '2', true),
    (8, 'barista_lam', 'Võ Tùng', 'Lâm', '0912345008', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 35000, '2', true),
    (9, 'cashier_linh', 'Mai Phương', 'Linh', '0912345009', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 30000, '2', false),
    (10, 'staff_nam', 'Bùi Tiến', 'Nam', '0912345010', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 25000, '2', true),
    (11, 'staff_huyen', 'Phan Thị', 'Huyền', '0912345011', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 25000, '2', true),
    (12, 'barista_khoa', 'Đỗ Văn', 'Khoa', '0912345012', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 35000, '2', true),
    (13, 'cashier_hang', 'Trương Thị', 'Hằng', '0912345013', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 30000, '2', true),
    (14, 'staff_quang', 'Lý Quang', 'Quang', '0912345014', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 25000, '2', true),
    (15, 'staff_thao', 'Phạm Thị', 'Thảo', '0912345015', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 25000, '2', true),
    (16, 'barista_hieu', 'Nguyễn Hữu', 'Hiếu', '0912345016', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 35000, '2', true),
    (17, 'cashier_hoa', 'Lê Thị', 'Hoa', '0912345017', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 30000, '2', true),
    (18, 'staff_khoa', 'Đỗ Văn', 'Khoa', '0912345018', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 25000, '2', true),
    (19, 'staff_hang', 'Trương Thị', 'Hằng', '0912345019', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 25000, '2', true),
    (20, 'staff_quang2', 'Lý Quang', 'Quang2', '0912345020', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 25000, '2', true),
    (21, 'staff_thao2', 'Phạm Thị', 'Thảo2', '0912345021', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 25000, '2', true),
    (22, 'barista_hieu2', 'Nguyễn Hữu', 'Hiếu2', '0912345022', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 35000, '2', true),
    (23, 'cashier_hoa2', 'Lê Thị', 'Hoa2', '0912345023', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 30000, '2', true),
    (24, 'staff_khoa2', 'Đỗ Văn', 'Khoa2', '0912345024', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 25000, '2', true),
    (25, 'staff_hang2', 'Trương Thị', 'Hằng2', '0912345025', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 25000, '2', true)

ON CONFLICT
("Id") DO
UPDATE SET
    "Username" = EXCLUDED."Username",
    "FirstName" = EXCLUDED."FirstName",
    "LastName" = EXCLUDED."LastName",
    "Phone" = EXCLUDED."Phone",
    "PasswordHash" = EXCLUDED."PasswordHash",
    "HourlyRate" = EXCLUDED."HourlyRate",
    "Role" = EXCLUDED."Role",
    "IsActive" = EXCLUDED."IsActive";

-- Cập nhật lại số thứ tự ID để khi thêm nhân viên mới trên App không bị lỗi trùng ID
SELECT setval(pg_get_serial_sequence('"Users"', 'Id'), COALESCE((SELECT MAX("Id")
    FROM "Users"), 1), TRUE);

-- Kiểm tra kết quả
SELECT "Id", "Username", "FirstName", "LastName", "Role", "IsActive"
FROM "Users"
ORDER BY "Id" ASC;
