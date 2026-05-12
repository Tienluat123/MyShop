using System.Threading;

namespace MyShop.Data;

public static class DbThrottle
{
    // Giới hạn 5 truy vấn đồng thời cho TOÀN BỘ ứng dụng
    public static readonly SemaphoreSlim GlobalSemaphore = new SemaphoreSlim(5, 5);
}
