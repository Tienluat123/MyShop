#!/bin/bash

# ============================================
# Coffee Shop Seed Data - Run All Script
# ============================================
# Usage: chmod +x run-all.sh && ./run-all.sh
# Or: bash run-all.sh

DB_USER="nguyentienluat"
DB_NAME="myshop"
DB_HOST="localhost"
DB_PORT="5432"

echo "🚀 Starting Coffee Shop Seed Data initialization..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

# Array of SQL files to run in order
FILES=(
    "01-categories.sql"
    "02-products.sql"
    "02-product-images.sql"
    "03-customers.sql"
    "04-users.sql"
    "06-promotions.sql"
    "05-orders.sql"
    "07-loyalty-rules.sql"
    "07-workschedules.sql"
    "08-appconfig.sql"
    "09-expenses.sql"
)

# Run each SQL file
for file in "${FILES[@]}"; do
    if [ -f "$file" ]; then
        echo "📝 Running: $file"
        psql -U "$DB_USER" -d "$DB_NAME" -h "$DB_HOST" -p "$DB_PORT" -f "$file"
        if [ $? -eq 0 ]; then
            echo "✅ $file completed successfully"
        else
            echo "❌ $file failed!"
            exit 1
        fi
        echo ""
    else
        echo "❌ File not found: $file"
        exit 1
    fi
done

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "🎉 All seed data loaded successfully!"
echo ""
echo "📊 Final Summary:"
psql -U "$DB_USER" -d "$DB_NAME" -h "$DB_HOST" -p "$DB_PORT" -c "
SELECT 'Categories' as type, COUNT(*) as count FROM \"Categories\"
UNION ALL
SELECT 'Products', COUNT(*) FROM \"Products\"
UNION ALL
SELECT 'ProductImages', COUNT(*) FROM \"ProductImages\"
UNION ALL
SELECT 'Customers', COUNT(*) FROM \"Customers\"
UNION ALL
SELECT 'Users', COUNT(*) FROM \"Users\"
UNION ALL
SELECT 'Orders', COUNT(*) FROM \"Orders\"
UNION ALL
SELECT 'OrderDetails', COUNT(*) FROM \"OrderDetails\"
UNION ALL
SELECT 'Promotions', COUNT(*) FROM \"Promotions\"
UNION ALL
SELECT 'LoyaltyTierConfigs', COUNT(*) FROM \"LoyaltyTierConfigs\"
UNION ALL
SELECT 'WorkSchedules', COUNT(*) FROM \"WorkSchedules\"
UNION ALL
SELECT 'AppConfigs', COUNT(*) FROM \"AppConfigs\"
UNION ALL
SELECT 'Expenses', COUNT(*) FROM \"Expenses\"
ORDER BY type;
"
