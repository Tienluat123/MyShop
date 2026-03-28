using MyShop.Models;

namespace MyShop.Data;

/// <summary>
/// Seed dữ liệu mẫu cho Coffee Shop vào database
/// </summary>
public static class SeedData
{
    public static void Initialize(AppDbContext context)
    {
        // Nếu đã có data thì skip
        if (context.Categories.Any())
            return;

        // ===== Categories - Menu Coffee Shop =====
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Coffee", IconGlyph = "☕", Description = "Various coffee drinks" },
            new Category { Id = 2, Name = "Tea", IconGlyph = "🍵", Description = "Tea and herbal drinks" },
            new Category { Id = 3, Name = "Cake & Pastry", IconGlyph = "🍰", Description = "Cakes, pastries, and desserts" },
            new Category { Id = 4, Name = "Snacks", IconGlyph = "🥐", Description = "Sandwiches and light snacks" }
        };
        context.Categories.AddRange(categories);
        context.SaveChanges();

        // ===== Products =====
        var products = new List<Product>
        {
            // Coffee (CategoryId = 1)
            new Product { Id = 1, Name = "Espresso", Price = 30000, CategoryId = 1 },
            new Product { Id = 2, Name = "Latte", Price = 45000, CategoryId = 1 },
            new Product { Id = 3, Name = "Cappuccino", Price = 50000, CategoryId = 1 },
            new Product { Id = 4, Name = "Americano", Price = 40000, CategoryId = 1 },
            new Product { Id = 5, Name = "Macchiato", Price = 55000, CategoryId = 1 },
            new Product { Id = 6, Name = "Cold Brew", Price = 48000, CategoryId = 1 },

            // Tea (CategoryId = 2)
            new Product { Id = 7, Name = "Green Tea", Price = 30000, CategoryId = 2 },
            new Product { Id = 8, Name = "Black Tea", Price = 32000, CategoryId = 2 },
            new Product { Id = 9, Name = "Jasmine Tea", Price = 35000, CategoryId = 2 },
            new Product { Id = 10, Name = "Oolong Tea", Price = 38000, CategoryId = 2 },

            // Cake & Pastry (CategoryId = 3)
            new Product { Id = 11, Name = "Chocolate Cake", Price = 85000, CategoryId = 3 },
            new Product { Id = 12, Name = "Cheesecake", Price = 95000, CategoryId = 3 },
            new Product { Id = 13, Name = "Tiramisu", Price = 75000, CategoryId = 3 },
            new Product { Id = 14, Name = "Croissant", Price = 45000, CategoryId = 3 },

            // Snacks (CategoryId = 4)
            new Product { Id = 15, Name = "Sandwich Chicken", Price = 65000, CategoryId = 4 },
            new Product { Id = 16, Name = "Sandwich Tuna", Price = 60000, CategoryId = 4 },
            new Product { Id = 17, Name = "Bagel Cream Cheese", Price = 55000, CategoryId = 4 }
        };
        context.Products.AddRange(products);
        context.SaveChanges();

        // ===== Customers =====
        var customers = new List<Customer>
        {
            new Customer { Id = 1, Name = "Nguyễn Văn A", Phone = "0912345678", Email = "a@email.com", Status = "Active" },
            new Customer { Id = 2, Name = "Trần Thị B", Phone = "0987654321", Email = "b@email.com", Status = "Active" },
            new Customer { Id = 3, Name = "Phạm Văn C", Phone = "0901234567", Email = "c@email.com", Status = "VIP" },
            new Customer { Id = 4, Name = "Lê Thị D", Phone = "0945678901", Email = "d@email.com", Status = "Active" },
            new Customer { Id = 5, Name = "Hoàng Văn E", Phone = "0923456789", Email = "e@email.com", Status = "VIP" },
            new Customer { Id = 6, Name = "Vũ Thị F", Phone = "0934567890", Email = "f@email.com", Status = "Active" }
        };
        context.Customers.AddRange(customers);
        context.SaveChanges();

        // ===== Users (Staff) =====
        var users = new List<User>
        {
            new User { Id = 1, Username = "manager", FullName = "Quản lý Cafe", PasswordHash = "hashed_pw", HourlyRate = 50000, Role = "1", IsActive = true },
            new User { Id = 2, Username = "barista1", FullName = "Barista 1", PasswordHash = "hashed_pw", HourlyRate = 35000, Role = "2", IsActive = true },
            new User { Id = 3, Username = "barista2", FullName = "Barista 2", PasswordHash = "hashed_pw", HourlyRate = 35000, Role = "2", IsActive = true },
            new User { Id = 4, Username = "cashier", FullName = "Thu ngân", PasswordHash = "hashed_pw", HourlyRate = 30000, Role = "2", IsActive = true }
        };
        context.Users.AddRange(users);
        context.SaveChanges();

        // ===== Orders =====
        var orders = new List<Order>
        {
            new Order 
            { 
                Id = 1, 
                CustomerId = 1, 
                UserId = 2, 
                OrderDate = DateTime.Now.AddDays(-5),
                SubTotal = 125000,
                DiscountAmount = 0,
                TotalAmount = 125000,
                Status = "Paid"
            },
            new Order 
            { 
                Id = 2, 
                CustomerId = 2, 
                UserId = 3, 
                OrderDate = DateTime.Now.AddDays(-3),
                SubTotal = 180000,
                DiscountAmount = 0,
                TotalAmount = 180000,
                Status = "Paid"
            },
            new Order 
            { 
                Id = 3, 
                CustomerId = 3, 
                UserId = 2, 
                OrderDate = DateTime.Now.AddDays(-1),
                SubTotal = 235000,
                DiscountAmount = 0,
                TotalAmount = 235000,
                Status = "Paid"
            },
            new Order 
            { 
                Id = 4, 
                CustomerId = 4, 
                UserId = 4, 
                OrderDate = DateTime.Now,
                SubTotal = 95000,
                DiscountAmount = 0,
                TotalAmount = 95000,
                Status = "New"
            }
        };
        context.Orders.AddRange(orders);
        context.SaveChanges();

        // ===== OrderDetails =====
        var orderDetails = new List<OrderDetail>
        {
            new OrderDetail { Id = 1, OrderId = 1, ProductId = 2, Quantity = 2, UnitPrice = 45000 },
            new OrderDetail { Id = 2, OrderId = 1, ProductId = 11, Quantity = 1, UnitPrice = 85000 },
            new OrderDetail { Id = 3, OrderId = 2, ProductId = 3, Quantity = 2, UnitPrice = 50000 },
            new OrderDetail { Id = 4, OrderId = 2, ProductId = 14, Quantity = 2, UnitPrice = 45000 },
            new OrderDetail { Id = 5, OrderId = 3, ProductId = 5, Quantity = 1, UnitPrice = 55000 },
            new OrderDetail { Id = 6, OrderId = 3, ProductId = 15, Quantity = 1, UnitPrice = 65000 },
            new OrderDetail { Id = 7, OrderId = 3, ProductId = 12, Quantity = 1, UnitPrice = 95000 },
            new OrderDetail { Id = 8, OrderId = 4, ProductId = 1, Quantity = 2, UnitPrice = 30000 },
            new OrderDetail { Id = 9, OrderId = 4, ProductId = 16, Quantity = 1, UnitPrice = 60000 }
        };
        context.OrderDetails.AddRange(orderDetails);
        context.SaveChanges();

        // ===== Promotions =====
        var promotions = new List<Promotion>
        {
            new Promotion 
            { 
                Id = 1, 
                PromoCode = "MORNING10", 
                PromoType = "Percent", 
                DiscountValue = 10,
                MinOrderAmount = 50000,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(90),
                TargetAudience = "All",
                IsActive = true
            },
            new Promotion 
            { 
                Id = 2, 
                PromoCode = "BUY5GET1", 
                PromoType = "FixedAmount", 
                DiscountValue = 35000,
                MinOrderAmount = 0,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(60),
                TargetAudience = "All",
                IsActive = true
            },
            new Promotion 
            { 
                Id = 3, 
                PromoCode = "WELCOME25", 
                PromoType = "FixedAmount", 
                DiscountValue = 25000,
                MinOrderAmount = 0,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(120),
                TargetAudience = "All",
                IsActive = true
            }
        };
        context.Promotions.AddRange(promotions);
        context.SaveChanges();

        System.Diagnostics.Debug.WriteLine("✅ Coffee Shop seed data initialized successfully!");
    }
}
