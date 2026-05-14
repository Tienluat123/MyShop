-- ===== LOYALTY RULES SEED DATA =====
-- LoyaltyTierConfigs: PointsPerVnd, RedemptionRate, Tier thresholds
-- Config schema: Tier_{Name} → JSON { "TierName": "Bronze", "MinPoints": 0, "Multiplier": 1.0 }
-- Run AFTER migration: AddLoyaltyTierConfig
-- Edit by: PhamVanMinhBinhThuan
-- Last modified: 2026-04-09

-- =============================================================================
-- SECTION 1: Base config — PointsPerVnd & RedemptionRate
-- =============================================================================

-- PointsPerVnd: 1000 VND = 1 point (used for earning)
INSERT INTO "LoyaltyTierConfigs" ("Id", "ConfigKey", "ConfigValue", "Description", "DisplayOrder")
VALUES (1, 'PointsPerVnd', '1000', 'Earn: 1000 VND = 1 point', 1)
ON CONFLICT ("ConfigKey") DO UPDATE SET
    "ConfigValue" = EXCLUDED."ConfigValue",
    "Description" = EXCLUDED."Description",
    "DisplayOrder" = EXCLUDED."DisplayOrder";

-- RedemptionRate: 1 point = 100 VND (used for redeeming)
INSERT INTO "LoyaltyTierConfigs" ("Id", "ConfigKey", "ConfigValue", "Description", "DisplayOrder")
VALUES (2, 'RedemptionRate', '100', 'Redeem: 1 point = 100 VND', 2)
ON CONFLICT ("ConfigKey") DO UPDATE SET
    "ConfigValue" = EXCLUDED."ConfigValue",
    "Description" = EXCLUDED."Description",
    "DisplayOrder" = EXCLUDED."DisplayOrder";

-- =============================================================================
-- SECTION 2: Tier Thresholds — per Requirement.md spec
-- Bronze (1x, 0pts), Silver (1.5x, 5000pts), Gold (2x, 15000pts), Platinum (2.5x, 30000pts)
-- ConfigValue stores JSON: { "TierName": "...", "MinPoints": N, "Multiplier": N.N }
-- =============================================================================

-- Tier: Bronze — default tier, multiplier 1.0x, min 0 points
INSERT INTO "LoyaltyTierConfigs" ("Id", "ConfigKey", "ConfigValue", "Description", "DisplayOrder")
VALUES (10, 'Tier_Bronze', '{"TierName":"Bronze","MinPoints":0,"Multiplier":1.0}', 'Bronze: 1.0x, from 0 pts', 10)
ON CONFLICT ("ConfigKey") DO UPDATE SET
    "ConfigValue" = EXCLUDED."ConfigValue",
    "Description" = EXCLUDED."Description",
    "DisplayOrder" = EXCLUDED."DisplayOrder";

-- Tier: Silver — multiplier 1.5x, min 5000 points
INSERT INTO "LoyaltyTierConfigs" ("Id", "ConfigKey", "ConfigValue", "Description", "DisplayOrder")
VALUES (11, 'Tier_Silver', '{"TierName":"Silver","MinPoints":5000,"Multiplier":1.5}', 'Silver: 1.5x, from 5000 pts', 11)
ON CONFLICT ("ConfigKey") DO UPDATE SET
    "ConfigValue" = EXCLUDED."ConfigValue",
    "Description" = EXCLUDED."Description",
    "DisplayOrder" = EXCLUDED."DisplayOrder";

-- Tier: Gold — multiplier 2.0x, min 15000 points
INSERT INTO "LoyaltyTierConfigs" ("Id", "ConfigKey", "ConfigValue", "Description", "DisplayOrder")
VALUES (12, 'Tier_Gold', '{"TierName":"Gold","MinPoints":15000,"Multiplier":2.0}', 'Gold: 2.0x, from 15000 pts', 12)
ON CONFLICT ("ConfigKey") DO UPDATE SET
    "ConfigValue" = EXCLUDED."ConfigValue",
    "Description" = EXCLUDED."Description",
    "DisplayOrder" = EXCLUDED."DisplayOrder";

-- Tier: Platinum — multiplier 2.5x, min 30000 points
INSERT INTO "LoyaltyTierConfigs" ("Id", "ConfigKey", "ConfigValue", "Description", "DisplayOrder")
VALUES (13, 'Tier_Platinum', '{"TierName":"Platinum","MinPoints":30000,"Multiplier":2.5}', 'Platinum: 2.5x, from 30000 pts', 13)
ON CONFLICT ("ConfigKey") DO UPDATE SET
    "ConfigValue" = EXCLUDED."ConfigValue",
    "Description" = EXCLUDED."Description",
    "DisplayOrder" = EXCLUDED."DisplayOrder";

-- =============================================================================
-- SECTION 3: Update existing Customers to new split-points schema
-- Map: Points → TotalAccumulatedPoints = Points, CurrentAvailablePoints = Points
-- =============================================================================

UPDATE "Customers" SET "TotalAccumulatedPoints" = COALESCE("TotalAccumulatedPoints", 0), "CurrentAvailablePoints" = COALESCE("CurrentAvailablePoints", 0)
WHERE "TotalAccumulatedPoints" = 0 AND "CurrentAvailablePoints" = 0;

-- Assign tiers and realistic point totals for demo
UPDATE "Customers" SET
    "TotalAccumulatedPoints" = 1200,
    "CurrentAvailablePoints" = 800,
    "Tier" = 1  -- Silver
WHERE "Id" = 1;

UPDATE "Customers" SET
    "TotalAccumulatedPoints" = 5800,
    "CurrentAvailablePoints" = 3500,
    "Tier" = 1  -- Silver (near Gold threshold)
WHERE "Id" = 2;

UPDATE "Customers" SET
    "TotalAccumulatedPoints" = 16000,
    "CurrentAvailablePoints" = 12000,
    "Tier" = 2  -- Gold
WHERE "Id" = 3;

UPDATE "Customers" SET
    "TotalAccumulatedPoints" = 4500,
    "CurrentAvailablePoints" = 2000,
    "Tier" = 1  -- Silver
WHERE "Id" = 4;

UPDATE "Customers" SET
    "TotalAccumulatedPoints" = 35000,
    "CurrentAvailablePoints" = 28000,
    "Tier" = 3  -- Platinum
WHERE "Id" = 5;

UPDATE "Customers" SET
    "TotalAccumulatedPoints" = 200,
    "CurrentAvailablePoints" = 50,
    "Tier" = 0  -- Bronze
WHERE "Id" = 6;

SELECT setval('"LoyaltyTierConfigs_Id_seq"', COALESCE((SELECT MAX("Id") FROM "LoyaltyTierConfigs"), 1), TRUE);

-- =============================================================================
-- VERIFY
-- =============================================================================

SELECT "ConfigKey", "ConfigValue", "Description" FROM "LoyaltyTierConfigs" ORDER BY "DisplayOrder";

SELECT "Id", "Name", "Phone",
       "TotalAccumulatedPoints" AS "TotalPts",
       "CurrentAvailablePoints" AS "AvailablePts",
       "Tier"
FROM "Customers" ORDER BY "Id";
