namespace MyShop.Di;

/// <summary>
/// Simple Service Container để quản lý Dependency Injection
/// Giúp dễ dàng register và retrieve services
/// </summary>
public class ServiceContainer : IServiceProvider
{
    private static readonly Dictionary<Type, object> _services = new();
    private static ServiceContainer? _instance;

    public static ServiceContainer Instance => _instance ??= new ServiceContainer();

    /// <summary>
    /// Register service vào container
    /// </summary>
    public void Register<TInterface>(object implementation) where TInterface : class
    {
        if (implementation == null)
            throw new ArgumentNullException(nameof(implementation));

        _services[typeof(TInterface)] = implementation;
    }

    /// <summary>
    /// Register service với interface
    /// </summary>
    public void Register<TInterface, TImplementation>(TImplementation implementation)
        where TInterface : class
        where TImplementation : TInterface
    {
        if (implementation == null)
            throw new ArgumentNullException(nameof(implementation));

        _services[typeof(TInterface)] = implementation;
    }

    /// <summary>
    /// Lấy service từ container
    /// </summary>
    public T Get<T>() where T : class
    {
        var serviceType = typeof(T);
        if (!_services.TryGetValue(serviceType, out var service))
            throw new InvalidOperationException($"Service {serviceType.Name} has not been registered");

        return (T)service;
    }

    /// <summary>
    /// Lấy service với GetRequiredService (IServiceProvider pattern)
    /// </summary>
    public T GetRequiredService<T>() where T : notnull
    {
        var serviceType = typeof(T);
        if (!_services.TryGetValue(serviceType, out var service))
            throw new InvalidOperationException($"Service {serviceType.Name} has not been registered");

        return (T)service;
    }

    /// <summary>
    /// IServiceProvider.GetService implementation
    /// </summary>
    public object? GetService(Type serviceType)
    {
        return _services.TryGetValue(serviceType, out var service) ? service : null;
    }

    /// <summary>
    /// Kiểm tra service có được register không
    /// </summary>
    public bool IsRegistered<T>() where T : class
    {
        return _services.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Clear tất cả services
    /// </summary>
    public void Clear()
    {
        _services.Clear();
    }
}
