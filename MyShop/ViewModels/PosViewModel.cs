using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class PosViewModel : ObservableObject
{
    private const double TAX_RATE = 0.0875;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<CategoryItem> Categories { get; } = new();
    public ObservableCollection<OrderItem> Order { get; } = new();
    public ObservableCollection<PosOrder> ActiveOrders { get; } = new();

    public ObservableCollection<Product> FilteredProducts { get; } = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string activeCategory = "Coffee";

    [ObservableProperty]
    private string searchQuery = "";

    [ObservableProperty]
    private double subtotal;

    [ObservableProperty]
    private double tax;

    [ObservableProperty]
    private double total;

    [ObservableProperty]
    private string orderType = "Dine-in";

    [ObservableProperty]
    private string tableNumber;

    public string TotalFormatted => $"Total: {Total:F2}";

    public event EventHandler? PlaceOrderRequested;

    public PosViewModel() : this(
        App.Services.GetRequiredService<IProductService>(),
        App.Services.GetRequiredService<ICategoryService>())
    {
    }

    public PosViewModel(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
        
        // Initial data loading
        InitializeAsync();

        // Add some customized items for verification
        var product = Products.FirstOrDefault();
        if (product != null)
        {
            var options = new DrinkOptions
            {
                Size = "L",
                Sugar = "50%",
                Ice = "Less Ice",
                Notes = "No foam please"
            };
            options.AddOns.Add("Extra Shot");
            options.AddOns.Add("Oat Milk");

            Order.Add(new OrderItem
            {
                Product = product,
                BasePrice = (double)product.Price + 1.5,
                Quantity = 1,
                DrinkOptions = options
            });
        }

        var secondProduct = Products.FirstOrDefault(p => p.Id == 2);
        if (secondProduct != null)
        {
            Order.Add(new OrderItem
            {
                Product = secondProduct,
                BasePrice = (double)secondProduct.Price,
                Quantity = 2
            });
        }
    }

    private async void InitializeAsync()
    {
        await LoadCategories();
        await LoadProductsForSale();
        
        // Load mock data for things not yet in DB if needed (like initial Order)
        // LoadMockData(); 
        
        RebuildFilter();

        Order.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (OrderItem item in e.NewItems)
                {
                    item.PropertyChanged += (_, __) => CalculateTotals();
                }
            }
            CalculateTotals();
            PlaceOrderCommand.NotifyCanExecuteChanged();
        };
    }

    private void LoadMockData()
    {
        Categories.Add(new CategoryItem { Name = "Coffee", IconKey = "IconCoffee", IsActive = true });
        Categories.Add(new CategoryItem { Name = "Tea", IconKey = "IconTea" });
        Categories.Add(new CategoryItem { Name = "Smoothies", IconKey = "IconSmoothies" });
        Categories.Add(new CategoryItem { Name = "Pastries", IconKey = "IconPastries" });

        // Mock product data (temporary - will be replaced with database)
        // For UI development purposes only
        var coffeeCategory = new Category { Id = 1, Name = "Coffee", IconGlyph = "☕" };
        var teaCategory = new Category { Id = 2, Name = "Tea", IconGlyph = "🍵" };
        var pastriesCategory = new Category { Id = 3, Name = "Pastries", IconGlyph = "🥐" };

        Products.Add(new Product { Id = 1, Name = "Caramel Latte", Price = 4.50m, CategoryId = 1, Category = coffeeCategory, ImagePath = "ms-appx:///Assets/caramel-latte.jpg" });
        Products.Add(new Product { Id = 1, Name = "Caramel Latte", Price = 4.50m, CategoryId = 1, Category = coffeeCategory, ImagePath = "ms-appx:///Assets/caramel-latte.jpg" });
        Products.Add(new Product { Id = 1, Name = "Caramel Latte", Price = 4.50m, CategoryId = 1, Category = coffeeCategory, ImagePath = "ms-appx:///Assets/caramel-latte.jpg" });
        Products.Add(new Product { Id = 1, Name = "Caramel Latte", Price = 4.50m, CategoryId = 1, Category = coffeeCategory, ImagePath = "ms-appx:///Assets/caramel-latte.jpg" });
        Products.Add(new Product { Id = 1, Name = "Caramel Latte", Price = 4.50m, CategoryId = 1, Category = coffeeCategory, ImagePath = "ms-appx:///Assets/caramel-latte.jpg" });
        Products.Add(new Product { Id = 1, Name = "Caramel Latte", Price = 4.50m, CategoryId = 1, Category = coffeeCategory, ImagePath = "ms-appx:///Assets/caramel-latte.jpg" });
        Products.Add(new Product { Id = 1, Name = "Caramel Latte", Price = 4.50m, CategoryId = 1, Category = coffeeCategory, ImagePath = "ms-appx:///Assets/caramel-latte.jpg" });
        Products.Add(new Product { Id = 1, Name = "Caramel Latte", Price = 4.50m, CategoryId = 1, Category = coffeeCategory, ImagePath = "ms-appx:///Assets/caramel-latte.jpg" });
        Products.Add(new Product { Id = 2, Name = "Matcha Latte", Price = 5.00m, CategoryId = 2, Category = teaCategory, ImagePath = "ms-appx:///Assets/matcha-latte.jpg" });
        Products.Add(new Product { Id = 3, Name = "Blueberry Muffin", Price = 3.25m, CategoryId = 3, Category = pastriesCategory, ImagePath = "ms-appx:///Assets/muffin.jpg" });
        Products.Add(new Product { Id = 4, Name = "Croissant", Price = 2.75m, CategoryId = 3, Category = pastriesCategory, ImagePath = "ms-appx:///Assets/croissant.jpg" });
    }

    private void RebuildFilter()
    {
        FilteredProducts.Clear();

        var filtered = Products.Where(p =>
            (ActiveCategory == "All" || p.Category?.Name == ActiveCategory) &&
            (string.IsNullOrWhiteSpace(SearchQuery) || p.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)));

        foreach (var p in filtered)
            FilteredProducts.Add(p);
    }

    partial void OnSearchQueryChanged(string value) => RebuildFilter();
    partial void OnActiveCategoryChanged(string value) => RebuildFilter();

    [RelayCommand]
    public async Task LoadCategories()
    {
        try
        {
            var result = await _categoryService.GetAll();
            if (result != null && result.Any())
            {
                Categories.Clear();
                foreach (var cat in result)
                {
                    Categories.Add(new CategoryItem
                    {
                        Name = cat.Name,
                        IconKey = cat.IconGlyph ?? "IconCoffee",
                        IsActive = false
                    });
                }

                // Set first as active if none is active
                if (Categories.Any())
                {
                    SetCategory(Categories.First());
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading categories: {ex.Message}");
        }
    }

    [RelayCommand]
    private void SetCategory(CategoryItem category)
    {
        foreach (var c in Categories)
            c.IsActive = (c == category);

        ActiveCategory = category.Name;
    }

    [RelayCommand]
    public async Task LoadProductsForSale()
    {
        IsLoading = true;
        try
        {
            var result = await _productService.GetAll();
            if (result != null)
            {
                Products.Clear();
                foreach (var product in result.Items)
                {
                    Products.Add(product);
                }
                RebuildFilter();
            }
        }
        catch (Exception ex)
        {
            // Logging or error handling could go here
            System.Diagnostics.Debug.WriteLine($"Error loading products: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void AddToCart(Product product)
    {
        var existing = Order.FirstOrDefault(i => i.Product.Id == product.Id && i.DrinkOptions == null);

        if (existing != null)
        {
            existing.Quantity++;
        }
        else
        {
            Order.Add(new OrderItem
            {
                Product = product,
                BasePrice = (double)product.Price,
                Quantity = 1
            });
        }
    }

    [RelayCommand]
    public void AddCustomizedToCart(AddCustomizedParams param)
    {
        // For customized items, we might want to check if the exact same customization exists
        // but for now let's just add it as a new item to be safe, or match exactly.
        var existing = Order.FirstOrDefault(i =>
            i.Product.Id == param.Product.Id &&
            AreOptionsEqual(i.DrinkOptions, param.Options));

        if (existing != null)
        {
            existing.Quantity += param.Quantity;
        }
        else
        {
            Order.Add(new OrderItem
            {
                Product = param.Product,
                BasePrice = (double)param.Product.Price, // In a real app, calculate based on options
                Quantity = param.Quantity,
                DrinkOptions = param.Options
            });
        }
    }

    private bool AreOptionsEqual(DrinkOptions? o1, DrinkOptions? o2)
    {
        if (o1 == null && o2 == null) return true;
        if (o1 == null || o2 == null) return false;
        return o1.Size == o2.Size && o1.Sugar == o2.Sugar && o1.Ice == o2.Ice && o1.Notes == o2.Notes && o1.AddOns.SequenceEqual(o2.AddOns);
    }

    [RelayCommand]
    private void RemoveItem(OrderItem item)
    {
        Order.Remove(item);
    }

    [RelayCommand]
    private void ClearCart()
    {
        Order.Clear();
    }

    [RelayCommand]
    private void UpdateQuantity(UpdateQtyParams param)
    {
        if (param.Item == null) return;
        param.Item.Quantity += param.Delta;
        if (param.Item.Quantity <= 0)
            Order.Remove(param.Item);

        PlaceOrderCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void SetOrderType(string type)
    {
        OrderType = type;
        if (type != "Dine-in")
            TableNumber = null;
    }

    [RelayCommand]
    private void SetTableNumber(string table)
    {
        TableNumber = table;
    }

    private void CalculateTotals()
    {
        Subtotal = Order.Sum(i => i.TotalPrice);
        Tax = Subtotal * TAX_RATE;
        Total = Subtotal + Tax;
    }

    private bool CanPlaceOrder() => Order.Any();

    [RelayCommand(CanExecute = nameof(CanPlaceOrder))]
    private void PlaceOrder()
    {
        PlaceOrderRequested?.Invoke(this, EventArgs.Empty);
    }

    public PosOrder? FinalizeOrder(PaymentMethod method, Customer? customer = null)
    {
        if (!Order.Any()) return null;

        var newOrder = new PosOrder
        {
            OrderId = $"ID-{DateTime.Now:Hmmss}",
            Subtotal = Subtotal,
            Tax = Tax,
            Total = Total,
            Type = OrderType,
            Table = TableNumber,
            CreatedAt = DateTime.Now,
            Status = OrderStatus.Pending,
            Method = method,
            CustomerId = customer?.Id.ToString(),
            CustomerName = customer?.Name
        };

        foreach (var item in Order)
        {
            newOrder.Items.Add(new OrderItem
            {
                Product = item.Product,
                BasePrice = item.BasePrice,
                Quantity = item.Quantity,
                DrinkOptions = item.DrinkOptions
            });
        }

        ActiveOrders.Add(newOrder);
        Order.Clear();
        return newOrder;
    }
}
