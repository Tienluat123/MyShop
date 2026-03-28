# 🌱 SeedData - Coffee Shop Database Initialization

Team-friendly seed data structure for MyShop Coffee Shop POS System.

## 📁 File Structure

```
SeedData/
├── 01-categories.sql    ← Menu categories (Coffee, Tea, Cake, Snacks)
├── 02-products.sql      ← Menu items with prices
├── 03-customers.sql     ← Customer profiles & loyalty tiers
├── 04-users.sql         ← Staff members (Manager, Baristas, Cashier)
├── 05-orders.sql        ← Transaction history + order details
├── 06-promotions.sql    ← Discount codes & promotions
├── run-all.sh           ← Script to run all files
└── README.md            ← This file
```

## 🚀 How to Use

### **Option 1: Run All at Once (Recommended)**

```bash
cd SeedData

# Make script executable (first time only)
chmod +x run-all.sh

# Run all seed files
./run-all.sh
```

Or directly with bash:

```bash
bash run-all.sh
```

### **Option 2: Run Individual Files**

```bash
# From project root
psql -U nguyentienluat -d myshop < SeedData/01-categories.sql
psql -U nguyentienluat -d myshop < SeedData/02-products.sql
psql -U nguyentienluat -d myshop < SeedData/03-customers.sql
psql -U nguyentienluat -d myshop < SeedData/04-users.sql
psql -U nguyentienluat -d myshop < SeedData/05-orders.sql
psql -U nguyentienluat -d myshop < SeedData/06-promotions.sql
```

### **Option 3: Run All in One Command**

```bash
# From SeedData directory
for f in *.sql; do echo "Processing $f..."; psql -U nguyentienluat -d myshop -f "$f"; done
```

## 📝 How to Edit

Each file has header comments showing who last edited it:

```sql
-- ===== CATEGORIES =====
-- Coffee Shop Categories
-- Edit by: [Your Name]
-- Last modified: [Date]
```

### **Team Collaboration Rules:**

1. **One person per file** - Avoid conflicts
   - Categories: `[Name]`
   - Products: `[Name]`
   - Customers: `[Name]`
   - Users: `[Name]`
   - Orders: `[Name]`
   - Promotions: `[Name]`

2. **Before editing:**
   - Pull latest changes
   - Update the "Edit by" header
   - Make your changes
   - Test on your local database

3. **After editing:**
   - Commit: `git add SeedData/` && `git commit -m "update: [file name] - [what changed]"`
   - Push: `git push`
   - Notify team: Tell them to pull and re-run seed data

## 🔄 Workflow Example

**Scenario:** Developer B wants to add new products (Coffee)

```bash
# 1. Get latest from team
git pull origin refactor/setup-mvvm

# 2. Check current data
cd SeedData
bash run-all.sh

# 3. Edit the file with DBeaver or text editor
# Update 02-products.sql → Add new items

# 4. Test on local database
psql -U nguyentienluat -d myshop < 02-products.sql

# 5. Verify in DBeaver or psql
psql -U nguyentienluat -d myshop -c "SELECT * FROM \"Products\" WHERE \"CategoryId\" = 1;"

# 6. Commit changes
git add SeedData/02-products.sql
git commit -m "update: products - add 3 new cold coffee items"
git push

# 7. Tell team to pull & re-run:
# git pull && cd SeedData && bash run-all.sh
```

## ⚠️ Important Notes

### **Avoid Conflicts:**

- ✅ Each person edits ONE file only
- ✅ Always pull before editing
- ✅ Always push after editing
- ❌ DON'T edit multiple files in one commit
- ❌ DON'T merge manually if conflicts occur

### **ID Management:**

Each file resets its own sequences:

```sql
SELECT setval('"Categories_Id_seq"', (SELECT MAX("Id") FROM "Categories") + 1);
```

This prevents ID conflicts even if someone deletes data.

### **On Conflict Handling:**

All INSERT statements use `ON CONFLICT DO NOTHING`, so re-running is safe:

```sql
INSERT INTO ... ON CONFLICT DO NOTHING;
```

✅ Safe to run multiple times without duplicates!

## 🗑️ Cleanup

**Reset all data and start fresh:**

```bash
# From SeedData directory
psql -U nguyentienluat -d myshop << EOF
-- Delete in reverse order (respecting FK constraints)
DELETE FROM "OrderDetails";
DELETE FROM "Orders";
DELETE FROM "Promotions";
DELETE FROM "Users";
DELETE FROM "Customers";
DELETE FROM "Products";
DELETE FROM "Categories";

-- Reset all sequences
SELECT setval('"Categories_Id_seq"', 1);
SELECT setval('"Products_Id_seq"', 1);
SELECT setval('"Customers_Id_seq"', 1);
SELECT setval('"Users_Id_seq"', 1);
SELECT setval('"Orders_Id_seq"', 1);
SELECT setval('"OrderDetails_Id_seq"', 1);
SELECT setval('"Promotions_Id_seq"', 1);

-- Verify
SELECT COUNT(*) FROM "Categories";
EOF

# Then re-run seed data
bash run-all.sh
```

## � Modifying Database Schema

> **⚠️ IMPORTANT:** Schema changes are **NOT** done in SeedData files!  
> Use **EF Core Migrations** instead. SeedData is for DATA only.

### **What is Schema?**

- Database tables (`Categories`, `Products`, `Orders`, etc.)
- Columns in tables (`Name`, `Price`, `Email`, etc.)
- Relationships (Foreign Keys, etc.)

### **Schema Changes Workflow**

**Example:** Add new column `QRCode` to Product table

#### **Step 1: Edit the C# Model**

File: `MyShop/Models/Product.cs`

```csharp
public partial class Product : ObservableObject
{
    // Existing properties...

    [ObservableProperty]
    private string? qrCode;  // ← NEW column
}
```

#### **Step 2: Create Migration**

From project root:

```bash
cd MyShop
dotnet ef migrations add AddQRCodeToProduct --project MyShop.csproj
```

✅ This auto-generates migration files in `MyShop/Migrations/`

#### **Step 3: Apply Migration to Your Database**

```bash
dotnet ef database update --project MyShop.csproj
```

✅ Database schema updated locally!

#### **Step 4: Commit & Share with Team**

```bash
git add MyShop/Migrations/
git commit -m "migration: add QRCode column to Product table"
git push origin refactor/setup-mvvm
```

#### **Step 5: Team Members Update**

Each team member runs:

```bash
git pull origin refactor/setup-mvvm
cd MyShop
dotnet ef database update
```

✅ Everyone's database schema is synced!

#### **Step 6: Update Seed Data (if needed)**

If your new column needs default values, edit the seed files:

```sql
-- SeedData/02-products.sql

INSERT INTO "Products" ("Id", "Name", "Price", "CategoryId", "QRCode") VALUES
(1, 'Espresso', 30000, 1, 'PROD-001'),
(2, 'Latte', 45000, 1, 'PROD-002'),
...
```

Then team re-runs:

```bash
cd SeedData
bash run-all.sh
```

---

### **Common Schema Changes**

| Change                 | Command                                       | File              |
| ---------------------- | --------------------------------------------- | ----------------- |
| **Add column**         | `dotnet ef migrations add AddXxxToYyy`        | Model + Migration |
| **Remove column**      | `dotnet ef migrations add RemoveXxxFromYyy`   | Model + Migration |
| **Change column type** | `dotnet ef migrations add ChangeXxxTypeInYyy` | Model + Migration |
| **Add relationship**   | `dotnet ef migrations add AddXxxRelationship` | Model + Migration |
| **Rename column**      | `dotnet ef migrations add RenameXxxInYyy`     | Model + Migration |

---

### **❌ DON'T:**

- ❌ Edit migrations manually (auto-generated!)
- ❌ Change schema via SQL files (use migrations!)
- ❌ Forget to `dotnet ef database update` after pulling

### **✅ DO:**

- ✅ Edit C# Models first
- ✅ Let EF Core generate migrations
- ✅ Share migrations via Git
- ✅ All team members run `database update`

---

### **Verify Schema Applied**

Check if migration worked:

```bash
# From terminal
psql -U nguyentienluat -d myshop -c "\d \"Products\""

# Shows all columns including new "QRCode"
```

Or use DBeaver:

- Connect to database
- Right-click table → **View table**
- Look for new columns ✅

---

## �📞 Troubleshooting

### **Error: "database does not exist"**

```bash
createdb -U nguyentienluat myshop
```

### **Error: "relation 'Categories' does not exist"**

- Migrations haven't run yet
- Run app first: `dotnet run -f net10.0-desktop --project ../MyShop/MyShop.csproj`

### **Error: "permission denied"**

```bash
chmod +x run-all.sh
```

### **Duplicate key errors**

- Someone already ran the script
- Use `ON CONFLICT DO NOTHING` to skip duplicates (already in place)

## 📊 Data Overview

| Entity       | Count | Purpose                    |
| ------------ | ----- | -------------------------- |
| Categories   | 4     | Coffee, Tea, Cake, Snacks  |
| Products     | 23    | Menu items with prices     |
| Customers    | 6     | Sample cafe customers      |
| Users        | 5     | Manager + 4 Staff          |
| Orders       | 5     | Sample transaction history |
| OrderDetails | 13    | Order line items           |
| Promotions   | 5     | Discount codes             |

## ✅ Quick Checklist

- [ ] Pull latest code
- [ ] cd SeedData
- [ ] bash run-all.sh
- [ ] Verify in DBeaver or psql
- [ ] Ready to develop!

---

**Last Updated:** March 28, 2026  
**Maintained By:** Development Team

