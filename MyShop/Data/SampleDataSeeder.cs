using System;
using System.Collections.Generic;
using MyShop.Models;

namespace MyShop.Data;

/// <summary>
/// Lớp hỗ trợ nạp dữ liệu mẫu vào Repository
/// </summary>
public static class SampleDataSeeder
{
    public static void Seed(
        IRepository<Product> productRepo,
        IRepository<Category> categoryRepo,
        IRepository<Order> orderRepo,
        IRepository<Customer> customerRepo,
        IRepository<User> userRepo)
    {
        // 1. Seed Categories if empty
        if (categoryRepo.GetAll().Items.Any()) return;

        var coffeeCat = categoryRepo.Add(new Category { Name = "Coffee", IconGlyph = "☕" });
        var teaCat = categoryRepo.Add(new Category { Name = "Tea", IconGlyph = "🍵" });
        var pastriesCat = categoryRepo.Add(new Category { Name = "Pastries", IconGlyph = "🥐" });

        // 2. Seed Products
        var coffee = productRepo.Add(new Product { Name = "Caramel Macchiato", Price = 5.50m, CategoryId = coffeeCat.Id });
        var tea = productRepo.Add(new Product { Name = "Green Tea", Price = 4.00m, CategoryId = teaCat.Id });
        var muffin = productRepo.Add(new Product { Name = "Blueberry Muffin", Price = 3.25m, CategoryId = pastriesCat.Id });

        // 3. Seed Customers
        var customer = customerRepo.Add(new Customer { Name = "Guest", Phone = "0123456789" });

        // 4. Seed Users (Staff)
        var staff = userRepo.Add(new User { FullName = "Staff member", Role = "2" });

        // 5. Seed Orders (Sample data from VM)
        var order1 = new Order
        {
            CustomerId = customer.Id,
            UserId = staff.Id,
            OrderDate = DateTime.Now.AddMinutes(-5),
            Status = "Pending",
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail { ProductId = coffee.Id, Quantity = 2, UnitPrice = 5.50m }
            }
        };
        orderRepo.Add(order1);

        var order2 = new Order
        {
            CustomerId = customer.Id,
            UserId = staff.Id,
            OrderDate = DateTime.Now.AddMinutes(-12),
            Status = "Preparing",
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail { ProductId = tea.Id, Quantity = 1, UnitPrice = 4.00m },
                new OrderDetail { ProductId = muffin.Id, Quantity = 1, UnitPrice = 3.25m }
            }
        };
        orderRepo.Add(order2);

        var order3 = new Order
        {
            CustomerId = customer.Id,
            UserId = staff.Id,
            OrderDate = DateTime.Now.AddHours(-1),
            Status = "Paid",
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail { ProductId = coffee.Id, Quantity = 1, UnitPrice = 5.50m }
            }
        };
        orderRepo.Add(order3);
    }
}
