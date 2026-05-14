INSERT INTO "Categories"
    ("Id","Name","HasCustomization","IconKey","Description")
VALUES
    (1,'Coffee',TRUE,'IconCoffee','Coffee drinks'),
    (2,'Tea',TRUE,'IconTea','Tea beverages'),
    (3,'Cake & Pastry',FALSE,'IconPastries','Cakes and pastries'),
    (4,'Snacks',FALSE,'IconSmoothies','Sandwiches and snacks')
ON CONFLICT ("Id") DO UPDATE
SET
    "Name" = EXCLUDED."Name",
    "HasCustomization" = EXCLUDED."HasCustomization",
    "IconKey" = EXCLUDED."IconKey",
    "Description" = EXCLUDED."Description";

SELECT setval('"Categories_Id_seq"', COALESCE((SELECT MAX("Id") FROM "Categories"), 1), TRUE);
