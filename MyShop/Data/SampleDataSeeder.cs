using System;
using System.Collections.Generic;
using MyShop.Models;

namespace MyShop.Data;

/// <summary>
/// Lớp hỗ trợ nạp dữ liệu mẫu vào Repository
/// </summary>
public static class SampleDataSeeder
{
    //public static void Seed(
    //    IRepository<Product> productRepo,
    //    IRepository<Category> categoryRepo,
    //    IRepository<Order> orderRepo,
    //    IRepository<Customer> customerRepo,
    //    IRepository<User> userRepo)
    //{
    //    // 1. Seed Categories if empty
    //    if (categoryRepo.GetAll().Items.Any()) return;

    //    var coffeeCat = categoryRepo.Add(new Category { Name = "Coffee", IconGlyph = "☕" });
    //    var teaCat = categoryRepo.Add(new Category { Name = "Tea", IconGlyph = "🍵" });
    //    var pastriesCat = categoryRepo.Add(new Category { Name = "Pastries", IconGlyph = "🥐" });

    //    // 2. Seed Products
    //    var coffee = productRepo.Add(new Product { Name = "Caramel Macchiato", Price = 5.50m, CategoryId = coffeeCat.Id });
    //    var tea = productRepo.Add(new Product { Name = "Green Tea", Price = 4.00m, CategoryId = teaCat.Id });
    //    var muffin = productRepo.Add(new Product { Name = "Blueberry Muffin", Price = 3.25m, CategoryId = pastriesCat.Id });

    //    // 3. Seed Customers
    //    var customer = customerRepo.Add(new Customer { Name = "Guest", Phone = "0123456789" });

    //    // 4. Seed Users (Staff)
    //    var staff = userRepo.Add(new User { FullName = "Staff member", Role = "2" });

    //    // 5. Seed Orders (Sample data from VM)
    //    var fulfillmentStatuses = new[] { "None", "Preparing", "Ready", "Completed" };
    //    var paymentStatuses = new[] { "Draft", "PendingPayment", "Paid", "Cancelled", "Refunded" };
    //    var products = new[] { coffee, tea, muffin };

    //    for (int i = 1; i <= 50; i++)
    //    {
    //        // Randomize order data
    //        int fillIndex = random.Next(fulfillmentStatuses.Length);
    //        int payIndex = random.Next(paymentStatuses.Length);
            
    //        // Bias towards active for testing (70% active)
    //        if (random.Next(10) < 7) 
    //        {
    //           fillIndex = random.Next(3); // None, Preparing, Ready
    //           payIndex = random.Next(2); // Draft, PendingPayment
    //        }
    //        else
    //        {
    //            fillIndex = 3; // Completed
    //            payIndex = random.Next(2, 5); // Paid, Cancelled, Refunded
    //        }

    //        int hoursAgo = random.Next(1, 48); // Last 48 hours
            
    //        var order = new Order
    //        {
    //            CustomerId = customer.Id,
    //            UserId = staff.Id,
    //            OrderDate = DateTime.Now.AddHours(-hoursAgo).AddMinutes(random.Next(-60, 0)),
    //            FulfillmentStatus = fulfillmentStatuses[fillIndex],
    //            PaymentStatus = paymentStatuses[payIndex],
    //            OrderDetails = new List<OrderDetail>()
    //        };

    //        int numItems = random.Next(1, 4);
    //        for (int j = 0; j < numItems; j++)
    //        {
    //            var product = products[random.Next(products.Length)];
    //            order.OrderDetails.Add(new OrderDetail
    //            {
    //                ProductId = product.Id,
    //                Quantity = random.Next(1, 4),
    //                UnitPrice = product.Price
    //            });
    //        }

    //        orderRepo.Add(order);
    //    }
    //}
}
