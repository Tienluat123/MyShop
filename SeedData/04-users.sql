-- ===== USERS (Staff) =====
-- Coffee Shop Staff Members
-- Edit by: Nguyễn Tiến Luật
-- Last modified: 6/4/2026
-- Database: PostgreSQL


INSERT INTO "Users"
    ("Id", "Username", "FirstName", "LastName", "Phone", "PasswordHash", "HourlyRate", "Role", "IsActive")
VALUES
    (1, 'admin_luat', 'Nguyễn Tiến', 'Luật', '0912345001', 'hash_pw_1', 100000, '1', true),
    (2, 'manager_minh', 'Phạm Văn', 'Minh', '0912345002', 'hash_pw_2', 80000, '1', true),
    (3, 'barista_tuan', 'Trần Anh', 'Tuấn', '0912345003', 'hash_pw_3', 35000, '2', true),
    (4, 'barista_hoa', 'Lê Thị', 'Hoa', '0912345004', 'hash_pw_4', 35000, '2', true),
    (5, 'cashier_ngan', 'Hoàng Thu', 'Ngân', '0912345005', 'hash_pw_5', 30000, '2', true),
    (6, 'staff_duy', 'Ngô Quang', 'Duy', '0912345006', 'hash_pw_6', 25000, '2', true),
    (7, 'staff_vy', 'Đặng Thúy', 'Vy', '0912345007', 'hash_pw_7', 25000, '2', true),
    (8, 'barista_lam', 'Võ Tùng', 'Lâm', '0912345008', 'hash_pw_8', 35000, '2', true),
    (9, 'cashier_linh', 'Mai Phương', 'Linh', '0912345009', 'hash_pw_9', 30000, '2', false),
    (10, 'staff_nam', 'Bùi Tiến', 'Nam', '0912345010', 'hash_pw_10', 25000, '2', true),
    (11, 'staff_huyen', 'Phan Thị', 'Huyền', '0912345011', 'hash_pw_11', 25000, '2', true),
    (12, 'barista_khoa', 'Đỗ Văn', 'Khoa', '0912345012', 'hash_pw_12', 35000, '2', true),
    (13, 'cashier_hang', 'Trương Thị', 'Hằng', '0912345013', 'hash_pw_13', 30000, '2', true),
    (14, 'staff_quang', 'Lý Quang', 'Quang', '0912345014', 'hash_pw_14', 25000, '2', true),
    (15, 'staff_thao', 'Phạm Thị', 'Thảo', '0912345015', 'hash_pw_15', 25000, '2', true),
    (16, 'barista_hieu', 'Nguyễn Hữu', 'Hiếu', '0912345016', 'hash_pw_16', 35000, '2', true),
    (17, 'cashier_hoa', 'Lê Thị', 'Hoa', '0912345017', 'hash_pw_17', 30000, '2', true),
    (18, 'staff_khoa', 'Đỗ Văn', 'Khoa', '0912345018', 'hash_pw_18', 25000, '2', true),
    (19, 'staff_hang', 'Trương Thị', 'Hằng', '0912345019', 'hash_pw_19', 25000, '2', true),
    (20, 'staff_quang2', 'Lý Quang', 'Quang2', '0912345020', 'hash_pw_20', 25000, '2', true),
    (21, 'staff_thao2', 'Phạm Thị', 'Thảo2', '0912345021', 'hash_pw_21', 25000, '2', true),
    (22, 'barista_hieu2', 'Nguyễn Hữu', 'Hiếu2', '0912345022', 'hash_pw_22', 35000, '2', true),
    (23, 'cashier_hoa2', 'Lê Thị', 'Hoa2', '0912345023', 'hash_pw_23', 30000, '2', true),
    (24, 'staff_khoa2', 'Đỗ Văn', 'Khoa2', '0912345024', 'hash_pw_24', 25000, '2', true),
    (25, 'staff_hang2', 'Trương Thị', 'Hằng2', '0912345025', 'hash_pw_25', 25000, '2', true)

ON CONFLICT
("Id") DO
UPDATE SET
    "Username" = EXCLUDED."Username",
    "FirstName" = EXCLUDED."FirstName",
    "LastName" = EXCLUDED."LastName",
    "Phone" = EXCLUDED."Phone",
    "HourlyRate" = EXCLUDED."HourlyRate",
    "Role" = EXCLUDED."Role",
    "IsActive" = EXCLUDED."IsActive";

-- Cập nhật lại số thứ tự ID để khi thêm nhân viên mới trên App không bị lỗi trùng ID
SELECT setval(pg_get_serial_sequence('"Users"', 'Id'), (SELECT MAX("Id")
    FROM "Users"));

-- Kiểm tra kết quả
SELECT "Id", "Username", "FirstName", "LastName", "Role", "IsActive"
FROM "Users"
ORDER BY "Id" ASC;


IN