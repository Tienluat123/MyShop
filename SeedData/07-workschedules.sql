-- ===== WORK SCHEDULES =====
-- Staff work schedules for current and previous month.
-- Shift: Morning / Afternoon / Evening, each shift counts as 6 hours in the app.
-- Last modified: 2026-05-14

-- Keep the seed idempotent. Only seed-owned rows are removed.
DELETE FROM "WorkSchedules"
WHERE "Note" LIKE 'Seed:%';

WITH current_month_days AS (
    SELECT generate_series(
        date_trunc('month', CURRENT_DATE)::date,
        (date_trunc('month', CURRENT_DATE)::date + INTERVAL '23 days')::date,
        INTERVAL '1 day'
    )::date AS work_date
),
previous_month_days AS (
    SELECT generate_series(
        (date_trunc('month', CURRENT_DATE)::date - INTERVAL '1 month')::date,
        (date_trunc('month', CURRENT_DATE)::date - INTERVAL '1 month' + INTERVAL '17 days')::date,
        INTERVAL '1 day'
    )::date AS work_date
),
current_assignments AS (
    SELECT *
    FROM (VALUES
        (1, 'Morning', 'Admin duty'),
        (2, 'Afternoon', 'Manager duty'),
        (3, 'Evening', 'Barista duty'),
        (4, 'Morning', 'Barista duty'),
        (5, 'Afternoon', 'Cashier duty'),
        (6, 'Evening', 'Staff duty'),
        (7, 'Morning', 'Staff duty'),
        (8, 'Afternoon', 'Barista duty')
    ) AS a(user_id, shift_name, note)
),
previous_assignments AS (
    SELECT *
    FROM (VALUES
        (1, 'Afternoon', 'Admin duty'),
        (2, 'Morning', 'Manager duty'),
        (3, 'Morning', 'Barista duty'),
        (4, 'Evening', 'Barista duty'),
        (5, 'Morning', 'Cashier duty'),
        (8, 'Evening', 'Barista duty')
    ) AS a(user_id, shift_name, note)
)
INSERT INTO "WorkSchedules" ("UserId", "WorkDate", "ShiftName", "Note", "Status")
SELECT
    a.user_id,
    d.work_date,
    a.shift_name,
    'Seed: ' || a.note,
    CASE
        WHEN d.work_date <= CURRENT_DATE THEN 'Completed'
        ELSE 'Scheduled'
    END
FROM current_month_days d
CROSS JOIN current_assignments a
WHERE EXISTS (SELECT 1 FROM "Users" u WHERE u."Id" = a.user_id)
UNION ALL
SELECT
    a.user_id,
    d.work_date,
    a.shift_name,
    'Seed: previous month ' || a.note,
    'Completed'
FROM previous_month_days d
CROSS JOIN previous_assignments a
WHERE EXISTS (SELECT 1 FROM "Users" u WHERE u."Id" = a.user_id);

SELECT setval('"WorkSchedules_Id_seq"', COALESCE((SELECT MAX("Id") FROM "WorkSchedules"), 1), TRUE);

SELECT ws."UserId", u."FirstName", u."LastName", ws."ShiftName", COUNT(*) AS "ShiftCount"
FROM "WorkSchedules" ws
LEFT JOIN "Users" u ON ws."UserId" = u."Id"
WHERE ws."Note" LIKE 'Seed:%'
GROUP BY ws."UserId", u."FirstName", u."LastName", ws."ShiftName"
ORDER BY ws."UserId", ws."ShiftName";
