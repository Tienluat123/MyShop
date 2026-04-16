-- ===== WORK SCHEDULES =====
-- Staff work schedules for 2 weeks
-- Shift: Morning (8h-14h), Afternoon (14h-20h), Evening (20h-02h) - each 6 hours
-- Edit by: Nguyễn Tiến Luật
-- Last modified: 7/4/2026
-- Database: PostgreSQL

-- Week 1: 8/4/2026 - 14/4/2026
-- Week 2: 15/4/2026 - 21/4/2026

INSERT INTO "WorkSchedules"
    ("UserId", "WorkDate", "ShiftName", "Note", "Status")
VALUES
    -- Week 1: April 8-14, 2026
    -- Monday 8/4
    (1, '2026-04-08', 'Morning', 'Admin duty', 'Scheduled'),
    (3, '2026-04-08', 'Afternoon', 'Barista duty', 'Scheduled'),
    (4, '2026-04-08', 'Evening', 'Barista duty', 'Scheduled'),
    (5, '2026-04-08', 'Morning', 'Cashier duty', 'Scheduled'),
    (6, '2026-04-08', 'Afternoon', 'Staff duty', 'Scheduled'),
    (7, '2026-04-08', 'Evening', 'Staff duty', 'Scheduled'),

    -- Tuesday 9/4
    (2, '2026-04-09', 'Morning', 'Manager duty', 'Scheduled'),
    (8, '2026-04-09', 'Afternoon', 'Barista duty', 'Scheduled'),
    (3, '2026-04-09', 'Evening', 'Barista duty', 'Scheduled'),
    (10, '2026-04-09', 'Morning', 'Staff duty', 'Scheduled'),
    (11, '2026-04-09', 'Afternoon', 'Staff duty', 'Scheduled'),
    (5, '2026-04-09', 'Evening', 'Cashier duty', 'Scheduled'),

    -- Wednesday 10/4
    (1, '2026-04-10', 'Morning', 'Admin duty', 'Scheduled'),
    (4, '2026-04-10', 'Afternoon', 'Barista duty', 'Scheduled'),
    (12, '2026-04-10', 'Evening', 'Barista duty', 'Scheduled'),
    (13, '2026-04-10', 'Morning', 'Cashier duty', 'Scheduled'),
    (14, '2026-04-10', 'Afternoon', 'Staff duty', 'Scheduled'),
    (15, '2026-04-10', 'Evening', 'Staff duty', 'Scheduled'),

    -- Thursday 11/4
    (2, '2026-04-11', 'Morning', 'Manager duty', 'Scheduled'),
    (16, '2026-04-11', 'Afternoon', 'Barista duty', 'Scheduled'),
    (8, '2026-04-11', 'Evening', 'Barista duty', 'Scheduled'),
    (17, '2026-04-11', 'Morning', 'Cashier duty', 'Scheduled'),
    (18, '2026-04-11', 'Afternoon', 'Staff duty', 'Scheduled'),
    (19, '2026-04-11', 'Evening', 'Staff duty', 'Scheduled'),

    -- Friday 12/4
    (1, '2026-04-12', 'Morning', 'Admin duty', 'Scheduled'),
    (3, '2026-04-12', 'Afternoon', 'Barista duty', 'Scheduled'),
    (4, '2026-04-12', 'Evening', 'Barista duty', 'Scheduled'),
    (20, '2026-04-12', 'Morning', 'Staff duty', 'Scheduled'),
    (21, '2026-04-12', 'Afternoon', 'Staff duty', 'Scheduled'),
    (22, '2026-04-12', 'Evening', 'Barista duty', 'Scheduled'),

    -- Saturday 13/4
    (2, '2026-04-13', 'Morning', 'Manager duty', 'Scheduled'),
    (12, '2026-04-13', 'Afternoon', 'Barista duty', 'Scheduled'),
    (16, '2026-04-13', 'Evening', 'Barista duty', 'Scheduled'),
    (13, '2026-04-13', 'Morning', 'Cashier duty', 'Scheduled'),
    (23, '2026-04-13', 'Afternoon', 'Cashier duty', 'Scheduled'),
    (24, '2026-04-13', 'Evening', 'Staff duty', 'Scheduled'),

    -- Sunday 14/4
    (1, '2026-04-14', 'Morning', 'Admin duty', 'Scheduled'),
    (8, '2026-04-14', 'Afternoon', 'Barista duty', 'Scheduled'),
    (3, '2026-04-14', 'Evening', 'Barista duty', 'Scheduled'),
    (5, '2026-04-14', 'Morning', 'Cashier duty', 'Scheduled'),
    (25, '2026-04-14', 'Afternoon', 'Staff duty', 'Scheduled'),
    (6, '2026-04-14', 'Evening', 'Staff duty', 'Scheduled'),

    -- Week 2: April 15-21, 2026
    -- Monday 15/4
    (1, '2026-04-15', 'Morning', 'Admin duty', 'Scheduled'),
    (4, '2026-04-15', 'Afternoon', 'Barista duty', 'Scheduled'),
    (12, '2026-04-15', 'Evening', 'Barista duty', 'Scheduled'),
    (5, '2026-04-15', 'Morning', 'Cashier duty', 'Scheduled'),
    (7, '2026-04-15', 'Afternoon', 'Staff duty', 'Scheduled'),
    (10, '2026-04-15', 'Evening', 'Staff duty', 'Scheduled'),

    -- Tuesday 16/4
    (2, '2026-04-16', 'Morning', 'Manager duty', 'Scheduled'),
    (16, '2026-04-16', 'Afternoon', 'Barista duty', 'Scheduled'),
    (8, '2026-04-16', 'Evening', 'Barista duty', 'Scheduled'),
    (13, '2026-04-16', 'Morning', 'Cashier duty', 'Scheduled'),
    (11, '2026-04-16', 'Afternoon', 'Staff duty', 'Scheduled'),
    (14, '2026-04-16', 'Evening', 'Staff duty', 'Scheduled'),

    -- Wednesday 17/4
    (1, '2026-04-17', 'Morning', 'Admin duty', 'Scheduled'),
    (3, '2026-04-17', 'Afternoon', 'Barista duty', 'Scheduled'),
    (17, '2026-04-17', 'Evening', 'Cashier duty', 'Scheduled'),
    (19, '2026-04-17', 'Morning', 'Staff duty', 'Scheduled'),
    (15, '2026-04-17', 'Afternoon', 'Staff duty', 'Scheduled'),
    (18, '2026-04-17', 'Evening', 'Staff duty', 'Scheduled'),

    -- Thursday 18/4
    (2, '2026-04-18', 'Morning', 'Manager duty', 'Scheduled'),
    (4, '2026-04-18', 'Afternoon', 'Barista duty', 'Scheduled'),
    (12, '2026-04-18', 'Evening', 'Barista duty', 'Scheduled'),
    (5, '2026-04-18', 'Morning', 'Cashier duty', 'Scheduled'),
    (20, '2026-04-18', 'Afternoon', 'Staff duty', 'Scheduled'),
    (21, '2026-04-18', 'Evening', 'Staff duty', 'Scheduled'),

    -- Friday 19/4
    (1, '2026-04-19', 'Morning', 'Admin duty', 'Scheduled'),
    (8, '2026-04-19', 'Afternoon', 'Barista duty', 'Scheduled'),
    (16, '2026-04-19', 'Evening', 'Barista duty', 'Scheduled'),
    (23, '2026-04-19', 'Morning', 'Cashier duty', 'Scheduled'),
    (22, '2026-04-19', 'Afternoon', 'Barista duty', 'Scheduled'),
    (24, '2026-04-19', 'Evening', 'Staff duty', 'Scheduled'),

    -- Saturday 20/4
    (2, '2026-04-20', 'Morning', 'Manager duty', 'Scheduled'),
    (3, '2026-04-20', 'Afternoon', 'Barista duty', 'Scheduled'),
    (4, '2026-04-20', 'Evening', 'Barista duty', 'Scheduled'),
    (13, '2026-04-20', 'Morning', 'Cashier duty', 'Scheduled'),
    (25, '2026-04-20', 'Afternoon', 'Staff duty', 'Scheduled'),
    (6, '2026-04-20', 'Evening', 'Staff duty', 'Scheduled'),

    -- Sunday 21/4
    (1, '2026-04-21', 'Morning', 'Admin duty', 'Scheduled'),
    (12, '2026-04-21', 'Afternoon', 'Barista duty', 'Scheduled'),
    (8, '2026-04-21', 'Evening', 'Barista duty', 'Scheduled'),
    (5, '2026-04-21', 'Morning', 'Cashier duty', 'Scheduled'),
    (11, '2026-04-21', 'Afternoon', 'Staff duty', 'Scheduled'),
    (14, '2026-04-21', 'Evening', 'Staff duty', 'Scheduled')

ON CONFLICT DO NOTHING;

-- Kiểm tra kết quả
SELECT ws."Id", ws."UserId", u."FirstName", u."LastName", ws."WorkDate", ws."ShiftName", ws."Status"
FROM "WorkSchedules" ws
    LEFT JOIN "Users" u ON ws."UserId" = u."Id"
ORDER BY ws."WorkDate" ASC, ws."ShiftName" ASC
LIMIT 50;
