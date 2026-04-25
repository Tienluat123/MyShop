# Excel Import Phase 1 (Categories + Products)

## Workbook structure

Use one .xlsx file with these required sheets:

1. Categories
2. Products

Header names are case-insensitive, but must match the expected words.

## Sheet: Categories

Required headers:

- CategoryCode
- Name
- HasCustomization

Optional headers:

- IconKey
- Description

Example rows:

| CategoryCode | Name          | HasCustomization | IconKey      | Description        |
| ------------ | ------------- | ---------------- | ------------ | ------------------ |
| CAT_COFFEE   | Coffee        | TRUE             | IconCoffee   | Coffee drinks      |
| CAT_TEA      | Tea           | TRUE             | IconTea      | Tea beverages      |
| CAT_PASTRY   | Cake & Pastry | FALSE            | IconPastries | Cakes and pastries |

## Sheet: Products

Required headers:

- ProductCode
- Name
- Price
- CategoryCode

Optional headers:

- ImagePath

Example rows:

| ProductCode  | Name      | Price | CategoryCode | ImagePath |
| ------------ | --------- | ----- | ------------ | --------- |
| PRD_ESPRESSO | Espresso  | 30000 | CAT_COFFEE   |           |
| PRD_LATTE    | Latte     | 45000 | CAT_COFFEE   |           |
| PRD_GREENTEA | Green Tea | 30000 | CAT_TEA      |           |

## Validation rules

- Only .xlsx is supported.
- CategoryCode and ProductCode must be unique within each sheet.
- HasCustomization supports TRUE/FALSE, 1/0, yes/no.
- Price must be a valid number >= 0.
- Products.CategoryCode must exist in Categories sheet.

## Upsert behavior

- Categories: upsert by Name (case-insensitive).
- Products: upsert by Name + Category.
- If parsing or validation has any error, the import is cancelled before database writes.

## UI entry point

From Product Management screen:

- Click Import Excel.
- Paste full .xlsx path.
- Confirm Import.

Status and errors appear in the Product Management message area.
