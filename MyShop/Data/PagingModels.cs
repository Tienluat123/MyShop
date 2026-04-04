namespace MyShop.Data;

/// <summary>
/// Yêu cầu phân trang từ client
/// </summary>
public class PagingRequest
{
    /// <summary>
    /// Số trang (1-based, mặc định = 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Số mục trên mỗi trang (mặc định = 10, tối đa = 100)
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Validate và điều chỉnh giá trị
    /// </summary>
    public string? SearchKeyword { get; set; }
    public void Validate()
    {
        if (PageNumber < 1) PageNumber = 1;
        if (PageSize < 1) PageSize = 10;
        if (PageSize > 100) PageSize = 100; // Giới hạn tối đa để tránh quá tải
    }

    /// <summary>
    /// Tính toán số mục cần bỏ qua (offset)
    /// </summary>
    /// <returns>Số mục cần bỏ qua</returns>
    public int GetSkip() => (PageNumber - 1) * PageSize;
}

/// <summary>
/// Metadata phân trang
/// </summary>
public class PagingMetadata
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((decimal)TotalItems / PageSize);
    public bool HasNext => PageNumber < TotalPages;
    public bool HasPrevious => PageNumber > 1;
}

/// <summary>
/// Kết quả phân trang - chứa dữ liệu + metadata
/// </summary>
/// <typeparam name="TData">Kiểu dữ liệu trong danh sách</typeparam>
public class PagedResult<TData> where TData : class
{
    public List<TData> Items { get; set; } = new();
    public PagingMetadata Pagination { get; set; } = new();
}
