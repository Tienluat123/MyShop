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
    private const decimal TAX_RATE = 0.0875m;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IOrderService _orderService;
    private readonly IPromotionService _promoService;
    private readonly ILoyaltyService _loyaltyService;
    private readonly ISessionService _sessionService;
    private readonly Helpers.Debouncer _filterDebouncer = new();

    private Order? _currentDraftOrder;
    private Order? _parkedDraftOrder;
    private bool _isEditingExistingOrder;

    public ObservableCollection<Product> Products { get; } = new Helpers.BulkObservableCollection<Product>();
    public ObservableCollection<CategoryItem> Categories { get; } = new();
    public Helpers.BulkObservableCollection<OrderItem> Order { get; } = new();
    public ObservableCollection<PosOrder> ActiveOrders { get; } = new();
    public Helpers.BulkObservableCollection<Product> FilteredProducts { get; } = new();
    public Helpers.BulkObservableCollection<Product> PagedProducts { get; } = new();

    private const int ProductPageSize = 6;

    public Order? CurrentDraftOrder => _currentDraftOrder;
    public bool IsEditingExistingOrder => _isEditingExistingOrder;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string activeCategory = "Coffee";

    [ObservableProperty]
    private string searchQuery = "";

    [ObservableProperty]
    private int currentProductPage = 1;

    [ObservableProperty]
    private int totalProductPages;

    public string ProductPageDisplay => TotalProductPages == 0
        ? "Page 0 of 0"
        : $"Page {CurrentProductPage} of {TotalProductPages}";

    [ObservableProperty]
    private decimal subtotal;

    [ObservableProperty]
    private decimal discountAmount;

    [ObservableProperty]
    private decimal tax;

    [ObservableProperty]
    private decimal total;

    [ObservableProperty]
    private string orderType = "Dine-in";

    [ObservableProperty]
    private string? tableNumber;

    [ObservableProperty]
    private decimal loyaltyDiscount;

    partial void OnLoyaltyDiscountChanged(decimal value) => CalculateTotals();

    [ObservableProperty]
    private Customer? selectedCustomer;

    partial void OnSelectedCustomerChanged(Customer? value)
    {
        if (_currentDraftOrder != null)
        {
            _currentDraftOrder.CustomerId = value?.Id; // Now nullable, null means guest
            _currentDraftOrder.Customer = value;
            _ = SaveDraftOrder();
        }
    }

    [ObservableProperty]
    private int pointsToRedeem;

    public string TotalFormatted => $"Total: {Total:F2}";

    public event EventHandler<OrderItem>? EditItemRequested;
    public event EventHandler? CheckoutRequested;
    public event EventHandler<Order>? OrderFinalized;

    public PosViewModel() : this(
        App.Services.GetRequiredService<IProductService>(),
        App.Services.GetRequiredService<ICategoryService>(),
        App.Services.GetRequiredService<IOrderService>(),
        App.Services.GetRequiredService<IPromotionService>(),
        App.Services.GetRequiredService<ILoyaltyService>(),
        App.Services.GetRequiredService<ISessionService>())
    {
    }

    public PosViewModel(
        IProductService productService,
        ICategoryService categoryService,
        IOrderService orderService,
        IPromotionService promoService,
        ILoyaltyService loyaltyService,
        ISessionService sessionService)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _promoService = promoService ?? throw new ArgumentNullException(nameof(promoService));
        _loyaltyService = loyaltyService ?? throw new ArgumentNullException(nameof(loyaltyService));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));

        // Initial data loading
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
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

        await LoadCategories();
        await LoadProductsForSale();

        RebuildFilter();

        // Final explicit check for command enablement
        CalculateTotals();
        PlaceOrderCommand.NotifyCanExecuteChanged();
    }

    public async Task InitializeSessionAsync()
    {
        await LoadCurrentDraftOrder();
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
        _filterDebouncer.Debounce(() => 
        {
            if (Microsoft.UI.Xaml.Application.Current is App app && app.MainWindow != null)
            {
                app.MainWindow.DispatcherQueue.TryEnqueue(() => 
                {
                    var filtered = Products.Where(p =>
                        (ActiveCategory == "All" || p.Category?.Name == ActiveCategory) &&
                        (string.IsNullOrWhiteSpace(SearchQuery) || p.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                    FilteredProducts.ReplaceAll(filtered);
                    CurrentProductPage = 1;
                    UpdatePagedProducts();
                });
            }
        });
    }

    partial void OnSearchQueryChanged(string value) => RebuildFilter();
    partial void OnActiveCategoryChanged(string value) => RebuildFilter();
    partial void OnCurrentProductPageChanged(int value) => OnPropertyChanged(nameof(ProductPageDisplay));
    partial void OnTotalProductPagesChanged(int value) => OnPropertyChanged(nameof(ProductPageDisplay));

    private void UpdatePagedProducts()
    {
        if (FilteredProducts.Count == 0)
        {
            TotalProductPages = 0;
            CurrentProductPage = 1;
            PagedProducts.Clear();
            UpdateProductPageCommandStates();
            return;
        }

        TotalProductPages = (int)Math.Ceiling(FilteredProducts.Count / (double)ProductPageSize);
        if (CurrentProductPage < 1) CurrentProductPage = 1;
        if (CurrentProductPage > TotalProductPages) CurrentProductPage = TotalProductPages;

        var pageItems = FilteredProducts
            .Skip((CurrentProductPage - 1) * ProductPageSize)
            .Take(ProductPageSize)
            .ToList();

        PagedProducts.ReplaceAll(pageItems);
        UpdateProductPageCommandStates();
    }

    private bool CanPreviousProductPage() => CurrentProductPage > 1;

    private bool CanNextProductPage() => CurrentProductPage < TotalProductPages;

    private void UpdateProductPageCommandStates()
    {
        PreviousProductPageCommand.NotifyCanExecuteChanged();
        NextProductPageCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanPreviousProductPage))]
    private void PreviousProductPage()
    {
        if (CurrentProductPage <= 1) return;

        CurrentProductPage--;
        UpdatePagedProducts();
    }

    [RelayCommand(CanExecute = nameof(CanNextProductPage))]
    private void NextProductPage()
    {
        if (CurrentProductPage >= TotalProductPages) return;

        CurrentProductPage++;
        UpdatePagedProducts();
    }

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
                        IconKey = cat.IconKey ?? "IconCoffee",
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
                if (Products is Helpers.BulkObservableCollection<Product> bulkProducts)
                {
                    bulkProducts.ReplaceAll(result.Items);
                }
                else
                {
                    Products.Clear();
                    foreach (var product in result.Items) Products.Add(product);
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
                UnitCostPrice = product.BaseCostPrice,
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

        var hasCustomization = param.Product.Category?.HasCustomization == true;

        // For simplicity, treating customized items as new entries or matching by notes
        var notes = hasCustomization && param.Options != null ? SerializeOptions(param.Options) : null;

        // Compute unit price and cost including size and addons, when applicable
        decimal unitPrice = param.Product.Price;
        decimal unitCost = param.Product.BaseCostPrice;

        if (hasCustomization && param.Options != null)
        {
            if (param.Options.Size == "M")
            {
                unitPrice += 15000;
                unitCost += 5000;
            }
            else if (param.Options.Size == "L")
            {
                unitPrice += 25000;
                unitCost += 8000;
            }
            
            var addOnCount = param.Options.AddOns?.Count ?? 0;
            unitPrice += addOnCount * 10000;
            unitCost += addOnCount * 3000;
        }

        var detail = _currentDraftOrder!.OrderDetails.FirstOrDefault(od =>
            od.ProductId == param.Product.Id && od.Notes == notes);
        if (detail != null)
        {
            detail.Quantity += param.Quantity;
        }
        else
        {
            _currentDraftOrder.OrderDetails.Add(new OrderDetail
            {
                ProductId = param.Product.Id,
                Quantity = param.Quantity,
                UnitPrice = unitPrice,
                UnitCostPrice = unitCost,
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
            paymentStatuses: new List<string> { "Draft" },
            userId: _sessionService.CurrentUser?.Id);

        _currentDraftOrder = result.Items.FirstOrDefault();

        if (_currentDraftOrder != null)
        {
            SyncCartFromDraft();
            return;
        }

        _currentDraftOrder = new Order
        {
            CustomerId = null, // Guest
            UserId = _sessionService.CurrentUser?.Id ?? 1,     // Current Staff or fallback
            PaymentStatus = "Draft",
            // Set defaults
            OrderDate = DateTime.UtcNow,
            OrderDetails = new List<OrderDetail>()
        };

        _currentDraftOrder = await _orderService.Add(_currentDraftOrder);
    }

    private async Task LoadCurrentDraftOrder()
    {
        if (_sessionService.CurrentUser == null) return;
        if (_isEditingExistingOrder) return;
        try
        {
            var result = await _orderService.GetPagedOrdersByStatuses(
                new PagingRequest { PageNumber = 1, PageSize = 1 },
                true,
                paymentStatuses: new List<string> { "Draft" },
                userId: _sessionService.CurrentUser?.Id);

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

    public async Task UpdateCustomizedItem(OrderItem originalItem, OrderItem editedItem)
    {
        if (_currentDraftOrder == null) return;

        var detail = _currentDraftOrder.OrderDetails.FirstOrDefault(od => od.Id == originalItem.OrderDetailId);

        if (detail != null)
        {
            detail.Quantity = editedItem.Quantity;
            detail.UnitPrice = editedItem.BasePrice;
            detail.Notes = editedItem.Product.Category?.HasCustomization == true
                ? SerializeOptions(editedItem.DrinkOptions)
                : null;

            await SaveDraftOrder();
            SyncCartFromDraft();
        }
    }

    private async Task SaveDraftOrder()
    {
        if (_currentDraftOrder == null) return;
        await _orderService.UpdateWithDetails(_currentDraftOrder, _currentDraftOrder.OrderDetails.ToList());
    }

    public async Task BeginEditOrderAsync(int orderId)
    {
        if (orderId <= 0) return;

        if (_currentDraftOrder?.Id != orderId && _currentDraftOrder?.PaymentStatus == "Draft")
        {
            await SaveDraftOrder();
            _parkedDraftOrder = _currentDraftOrder;
        }

        var orderToEdit = _currentDraftOrder?.Id == orderId
            ? _currentDraftOrder
            : await _orderService.GetById(orderId);

        if (orderToEdit == null) return;

        _currentDraftOrder = orderToEdit;
        _isEditingExistingOrder = true;
        SelectedCustomer = orderToEdit.Customer;
        LoyaltyDiscount = orderToEdit.ManualAdjustment;
        PointsToRedeem = 0;

        SyncCartFromDraft();
        OnPropertyChanged(nameof(CurrentDraftOrder));
        OnPropertyChanged(nameof(IsEditingExistingOrder));
        PlaceOrderCommand.NotifyCanExecuteChanged();
    }

    private void RestoreParkedDraftAfterEdit()
    {
        _isEditingExistingOrder = false;
        _currentDraftOrder = _parkedDraftOrder;
        _parkedDraftOrder = null;
        SelectedCustomer = _currentDraftOrder?.Customer;
        LoyaltyDiscount = 0;
        PointsToRedeem = 0;

        if (_currentDraftOrder != null)
        {
            SyncCartFromDraft();
        }
        else
        {
            Order.Clear();
            CalculateTotals();
        }

        OnPropertyChanged(nameof(CurrentDraftOrder));
        OnPropertyChanged(nameof(IsEditingExistingOrder));
        PlaceOrderCommand.NotifyCanExecuteChanged();
    }

    private void SyncCartFromDraft()
    {
        if (_currentDraftOrder == null) return;

        OrderType = _currentDraftOrder.Type;
        TableNumber = string.IsNullOrEmpty(_currentDraftOrder.TableNumber) ? null : _currentDraftOrder.TableNumber;

        var newItems = _currentDraftOrder.OrderDetails.Select(detail => new OrderItem
        {
            OrderDetailId = detail.Id,
            Product = detail.Product!,
            BasePrice = detail.UnitPrice,
            Quantity = detail.Quantity,
            DrinkOptions = ParseOptions(detail.Notes)
        }).ToList();

        Order.ReplaceAll(newItems);
        CalculateTotals();
    }

    [RelayCommand]
    public async Task RemoveFromCart(OrderItem item)
    {
        if (_currentDraftOrder == null) return;

        var detail = _currentDraftOrder.OrderDetails.FirstOrDefault(od => od.Id == item.OrderDetailId);

        if (detail != null)
        {
            _currentDraftOrder.OrderDetails.Remove(detail);
            await SaveDraftOrder();
            SyncCartFromDraft();
        }
    }

    [RelayCommand]
    private void EditItem(OrderItem item)
    {
        if (item == null) return;

        // Trigger the event so the View can open the Customization Modal
        EditItemRequested?.Invoke(this, item);
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

        var detail = _currentDraftOrder.OrderDetails.FirstOrDefault(od => od.Id == param.Item.OrderDetailId);

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
    private async Task ClearCart()
    {
        if (_currentDraftOrder != null)
        {
            try
            {
                if (_isEditingExistingOrder)
                {
                    _currentDraftOrder.PaymentStatus = "Cancelled";
                    await _orderService.Update(_currentDraftOrder);
                }
                else
                {
                    await _orderService.Delete(_currentDraftOrder.Id);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing cart from DB: {ex.Message}");
            }
            _currentDraftOrder = null;
        }

        if (_isEditingExistingOrder)
        {
            RestoreParkedDraftAfterEdit();
            return;
        }

        Order.Clear();
        CalculateTotals();
    }

    [RelayCommand]
    private async Task SetOrderType(string type)
    {
        OrderType = type;
        if (type != "Dine-in")
            TableNumber = null;

        if (_currentDraftOrder != null)
        {
            _currentDraftOrder.Type = type;
            if (type != "Dine-in")
                _currentDraftOrder.TableNumber = "";
            await SaveDraftOrder();
        }
    }

    [RelayCommand]
    private async Task SetTableNumber(string table)
    {
        TableNumber = table;
        if (_currentDraftOrder != null)
        {
            _currentDraftOrder.TableNumber = table ?? "";
            await SaveDraftOrder();
        }
    }

    private void CalculateTotals()
    {
        Subtotal = Order.Sum(i => i.TotalPrice);

        decimal promoDiscount = 0;
        if (_currentDraftOrder != null)
        {
            _currentDraftOrder.SubTotal = Subtotal;
            if (_currentDraftOrder.Promotion != null)
            {
                var promo = _currentDraftOrder.Promotion;
                if (promo.PromoType == "Percent")
                {
                    promoDiscount = Subtotal * (promo.DiscountValue / 100m);
                }
                else
                {
                    promoDiscount = promo.DiscountValue;
                }
            }
        }

        // Combine promo discount and loyalty redemption discount
        decimal totalDiscount = promoDiscount + LoyaltyDiscount;
        
        // Cap discount at subtotal
        totalDiscount = Math.Min(totalDiscount, Subtotal);
        DiscountAmount = totalDiscount;

        // Cart Summary values (Tax is decoupled from promotions here for stable UI)
        Tax = Math.Max(0, Subtotal * TAX_RATE);
        
        // Final Payment Value
        var postDiscountBase = Subtotal - totalDiscount;
        Total = Math.Max(0, postDiscountBase + Tax);

        if (_currentDraftOrder != null)
        {
            _currentDraftOrder.DiscountAmount = promoDiscount;
            _currentDraftOrder.ManualAdjustment = LoyaltyDiscount;
            _currentDraftOrder.ManualAdjustmentReason = LoyaltyDiscount > 0 ? $"Redeemed {PointsToRedeem} points" : "";
            _currentDraftOrder.SubTotal = Subtotal;

            // Payment values (Post-discount)
            postDiscountBase = Subtotal - totalDiscount;
            var paymentTax = Math.Max(0, postDiscountBase * TAX_RATE);
            var paymentTotal = Math.Max(0, postDiscountBase + paymentTax);

            _currentDraftOrder.TaxAmount = paymentTax;
            _currentDraftOrder.TotalAmount = paymentTotal;
        }
    }

    public async Task ApplyPromotion(Promotion? promo)
    {
        if (_currentDraftOrder == null) return;

        if (promo != null && !_promoService.IsPromotionApplicable(promo, Subtotal, SelectedCustomer, out var errorMessage))
        {
            System.Diagnostics.Debug.WriteLine(errorMessage);
            return;
        }

        _currentDraftOrder.PromotionId = promo?.Id;
        _currentDraftOrder.Promotion = promo;

        CalculateTotals();
        await SaveDraftOrder();
    }

    private bool CanPlaceOrder() => Order.Any();

    [RelayCommand(CanExecute = nameof(CanPlaceOrder))]
    private async Task PlaceOrder()
    {
        if (_currentDraftOrder != null)
        {
            _currentDraftOrder.PaymentStatus = "PendingPayment";
            await SaveDraftOrder();
        }
        CheckoutRequested?.Invoke(this, EventArgs.Empty);
    }

    public async Task RevertOrderToDraft()
    {
        if (_currentDraftOrder != null)
        {
            _currentDraftOrder.PaymentStatus = "Draft";
            await SaveDraftOrder();
            if (_isEditingExistingOrder)
            {
                RestoreParkedDraftAfterEdit();
            }
            else
            {
                PlaceOrderCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public async Task<PosOrder?> FinalizeOrderAsync(PaymentMethod method, Customer? customer = null)
    {
        if (!Order.Any()) return null;

        var subtotal = Subtotal;
        var tax = Tax;
        var total = Total;

        if (_currentDraftOrder != null)
        {
            _currentDraftOrder.CustomerId = customer?.Id;
            _currentDraftOrder.Customer = customer;
            subtotal = _currentDraftOrder.SubTotal;
            tax = _currentDraftOrder.TaxAmount;
            total = _currentDraftOrder.TotalAmount;
        }

        // 1. Handle Point Redemption if any
        if (customer != null && PointsToRedeem > 0)
        {
            try 
            {
                await _loyaltyService.RedeemPointsAsync(customer, PointsToRedeem);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Redeem error: {ex.Message}");
            }
        }

        // 2. Handle Point Reward (Calculated on final amount)
        if (customer != null)
        {
            try
            {
                await _loyaltyService.CalculatePointsAsync(customer, subtotal - DiscountAmount);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Reward points error: {ex.Message}");
            }
        }

        var newOrder = new PosOrder
        {
            OrderId = _currentDraftOrder?.Id.ToString() ?? $"ID-{DateTime.Now:Hmmss}",
            Subtotal = subtotal,
            Tax = tax,
            Total = total,
            Type = OrderType,
            Table = TableNumber,
            CreatedAt = DateTime.Now,
            PaymentStatus = PaymentStatus.Paid,
            FulfillmentStatus = FulfillmentStatus.None,
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
        var completedOrder = _currentDraftOrder;
        _currentDraftOrder = null;
        Order.Clear();
        SelectedCustomer = null;
        LoyaltyDiscount = 0;
        PointsToRedeem = 0;
        CalculateTotals();

        if (completedOrder != null)
        {
            OrderFinalized?.Invoke(this, completedOrder);
        }

        if (_isEditingExistingOrder)
        {
            RestoreParkedDraftAfterEdit();
        }

        return newOrder;
    }
}
