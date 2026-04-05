using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Models;
using MyShop.Services;
using MyShop.Data;

namespace MyShop.ViewModels;

public partial class PosViewModel : ObservableObject
{
    private const double TAX_RATE = 0.0875;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IOrderService _orderService;

    private Order? _currentDraftOrder;

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
        App.Services.GetRequiredService<ICategoryService>(),
        App.Services.GetRequiredService<IOrderService>())
    {
    }

    public PosViewModel(IProductService productService, ICategoryService categoryService, IOrderService orderService)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        
        // Initial data loading
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        await LoadCategories();
        await LoadProductsForSale();
        await LoadCurrentDraftOrder();
        
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

    //private void LoadMockData()
    //{
    //    Categories.Add(new CategoryItem { Name = "Coffee", IconKey = "IconCoffee", IsActive = true });
    //    Categories.Add(new CategoryItem { Name = "Tea", IconKey = "IconTea" });
    //    Categories.Add(new CategoryItem { Name = "Smoothies", IconKey = "IconSmoothies" });
    //    Categories.Add(new CategoryItem { Name = "Pastries", IconKey = "IconPastries" });

    //    // Mock product data (temporary - will be replaced with database)
    //    // For UI development purposes only
    //    var coffeeCategory = new Category { Id = 1, Name = "Coffee", IconGlyph = "☕" };
    //    var teaCategory = new Category { Id = 2, Name = "Tea", IconGlyph = "🍵" };
    //    var pastriesCategory = new Category { Id = 3, Name = "Pastries", IconGlyph = "🥐" };

    //    Products.Add(new Product { Id = 1, Name = "Caramel Latte", Price = 4.50m, CategoryId = 1, Category = coffeeCategory, ImagePath = "ms-appx:///Assets/caramel-latte.jpg" });
    //    Products.Add(new Product { Id = 1, Name = "Caramel Latte", Price = 4.50m, CategoryId = 1, Category = coffeeCategory, ImagePath = "ms-appx:///Assets/caramel-latte.jpg" });
    //    Products.Add(new Product { Id = 1, Name = "Caramel Latte", Price = 4.50m, CategoryId = 1, Category = coffeeCategory, ImagePath = "ms-appx:///Assets/caramel-latte.jpg" });
    //    Products.Add(new Product { Id = 1, Name = "Caramel Latte", Price = 4.50m, CategoryId = 1, Category = coffeeCategory, ImagePath = "ms-appx:///Assets/caramel-latte.jpg" });
    //    Products.Add(new Product { Id = 1, Name = "Caramel Latte", Price = 4.50m, CategoryId = 1, Category = coffeeCategory, ImagePath = "ms-appx:///Assets/caramel-latte.jpg" });
    //    Products.Add(new Product { Id = 1, Name = "Caramel Latte", Price = 4.50m, CategoryId = 1, Category = coffeeCategory, ImagePath = "ms-appx:///Assets/caramel-latte.jpg" });
    //    Products.Add(new Product { Id = 1, Name = "Caramel Latte", Price = 4.50m, CategoryId = 1, Category = coffeeCategory, ImagePath = "ms-appx:///Assets/caramel-latte.jpg" });
    //    Products.Add(new Product { Id = 1, Name = "Caramel Latte", Price = 4.50m, CategoryId = 1, Category = coffeeCategory, ImagePath = "ms-appx:///Assets/caramel-latte.jpg" });
    //    Products.Add(new Product { Id = 2, Name = "Matcha Latte", Price = 5.00m, CategoryId = 2, Category = teaCategory, ImagePath = "ms-appx:///Assets/matcha-latte.jpg" });
    //    Products.Add(new Product { Id = 3, Name = "Blueberry Muffin", Price = 3.25m, CategoryId = 3, Category = pastriesCategory, ImagePath = "ms-appx:///Assets/muffin.jpg" });
    //    Products.Add(new Product { Id = 4, Name = "Croissant", Price = 2.75m, CategoryId = 3, Category = pastriesCategory, ImagePath = "ms-appx:///Assets/croissant.jpg" });
    //}

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
    public async Task AddToCart(Product product)
    {
        await EnsureDraftOrder();

        var detail = _currentDraftOrder!.OrderDetails.FirstOrDefault(od => od.ProductId == product.Id);
        if (detail != null)
        {
            detail.Quantity++;
        }
        else
        {
            detail = new OrderDetail
            {
                ProductId = product.Id,
                Quantity = 1,
                UnitPrice = product.Price,
                Product = product
            };
            _currentDraftOrder.OrderDetails.Add(detail);
        }

        await SaveDraftOrder();
        SyncCartFromDraft();
    }

    [RelayCommand]
    public async Task AddCustomizedToCart(AddCustomizedParams param)
    {
        await EnsureDraftOrder();

        // For simplicity, treating customized items as new entries or matching by notes
        var notes = param.Options != null ? SerializeOptions(param.Options) : null;
        
        var detail = _currentDraftOrder!.OrderDetails.FirstOrDefault(od => 
            od.ProductId == param.Product.Id && od.Notes == notes);
        
        var existingDetail = _currentDraftOrder!.OrderDetails.FirstOrDefault(od => 
            od.ProductId == param.Product.Id && od.Notes == notes);
            
        if (existingDetail != null && param.Options == null)
        {
            existingDetail.Quantity += param.Quantity;
        }
        else
        {
            _currentDraftOrder.OrderDetails.Add(new OrderDetail
            {
                ProductId = param.Product.Id,
                Quantity = param.Quantity,
                UnitPrice = param.Product.Price,
                Product = param.Product,
                Notes = notes
            });
        }

        await SaveDraftOrder();
        SyncCartFromDraft();
    }

    private string? SerializeOptions(DrinkOptions? options)
    {
        if (options == null) return null;
        try
        {
            return JsonSerializer.Serialize(options);
        }
        catch
        {
            return null;
        }
    }

    private DrinkOptions? ParseOptions(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes)) return null;
        try
        {
            return JsonSerializer.Deserialize<DrinkOptions>(notes);
        }
        catch
        {
            return null;
        }
    }

    private async Task EnsureDraftOrder()
    {
        if (_currentDraftOrder != null) return;

        var result = await _orderService.GetPagedOrdersByStatuses(
            new PagingRequest { PageNumber = 1, PageSize = 1 }, 
            true, 
            paymentStatuses: new List<string> { "Draft" });
            
        _currentDraftOrder = result.Items.FirstOrDefault();

        if (_currentDraftOrder != null)
        {
            SyncCartFromDraft();
            return;
        }

        _currentDraftOrder = new Order
        {
            CustomerId = 1, // Default Guest
            UserId = 1,     // Default Staff
            PaymentStatus = "Draft",
            OrderDate = DateTime.UtcNow,
            OrderDetails = new List<OrderDetail>()
        };

        _currentDraftOrder = await _orderService.Add(_currentDraftOrder);
    }

    private async Task LoadCurrentDraftOrder()
    {
        try
        {
            var result = await _orderService.GetPagedOrdersByStatuses(
                new PagingRequest { PageNumber = 1, PageSize = 1 }, 
                true, 
                paymentStatuses: new List<string> { "Draft" });
                
            _currentDraftOrder = result.Items.FirstOrDefault();
            if (_currentDraftOrder != null)
            {
                SyncCartFromDraft();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading draft order: {ex.Message}");
        }
    }

    private async Task SaveDraftOrder()
    {
        if (_currentDraftOrder == null) return;
        
        _currentDraftOrder.SubTotal = _currentDraftOrder.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);
        _currentDraftOrder.TotalAmount = _currentDraftOrder.SubTotal - _currentDraftOrder.DiscountAmount;
        
        await _orderService.Update(_currentDraftOrder);
    }

    private void SyncCartFromDraft()
    {
        if (_currentDraftOrder == null) return;

        Order.Clear();
        foreach (var detail in _currentDraftOrder.OrderDetails)
        {
            Order.Add(new OrderItem
            {
                Product = detail.Product!,
                BasePrice = (double)detail.UnitPrice,
                Quantity = detail.Quantity,
                DrinkOptions = ParseOptions(detail.Notes)
            });
        }
        CalculateTotals();
    }

    [RelayCommand]
    public async Task RemoveFromCart(OrderItem item)
    {
        if (_currentDraftOrder == null) return;

        var notes = SerializeOptions(item.DrinkOptions);
        var detail = _currentDraftOrder.OrderDetails.FirstOrDefault(od => od.ProductId == item.Product.Id && od.Notes == notes);
        
        if (detail != null)
        {
            _currentDraftOrder.OrderDetails.Remove(detail);
            await SaveDraftOrder();
            SyncCartFromDraft();
        }
    }

    [RelayCommand]
    public async Task IncreaseQuantity(OrderItem item)
    {
        await UpdateCartQuantity(new UpdateQtyParams { Item = item, Delta = 1 });
    }

    [RelayCommand]
    public async Task DecreaseQuantity(OrderItem item)
    {
        await UpdateCartQuantity(new UpdateQtyParams { Item = item, Delta = -1 });
    }

    [RelayCommand]
    public async Task UpdateCartQuantity(UpdateQtyParams param)
    {
        if (_currentDraftOrder == null || param.Item == null) return;

        var notes = SerializeOptions(param.Item.DrinkOptions);
        var detail = _currentDraftOrder.OrderDetails.FirstOrDefault(od => od.ProductId == param.Item.Product.Id && od.Notes == notes);
        
        if (detail != null)
        {
            detail.Quantity += param.Delta;
            if (detail.Quantity <= 0)
            {
                _currentDraftOrder.OrderDetails.Remove(detail);
            }
            
            await SaveDraftOrder();
            SyncCartFromDraft();
        }
    }

    private bool AreOptionsEqual(DrinkOptions? o1, DrinkOptions? o2)
    {
        if (o1 == null && o2 == null) return true;
        if (o1 == null || o2 == null) return false;
        return o1.Size == o2.Size && o1.Sugar == o2.Sugar && o1.Ice == o2.Ice && o1.Notes == o2.Notes && o1.AddOns.SequenceEqual(o2.AddOns);
    }

    [RelayCommand]
    private async Task RemoveItem(OrderItem item) => await RemoveFromCart(item);

    [RelayCommand]
    private void ClearCart()
    {
        Order.Clear();
        _currentDraftOrder = null; // In a real app, maybe delete from DB or keep as cancelled
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
