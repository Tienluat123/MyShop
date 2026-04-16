namespace MyShop.Models;

/// <summary>
/// Wrapper cho page number với computed IsActive property
/// Dùng để highlight active page button trong pagination
/// </summary>
public class PageNumberItem
{
    private readonly int _currentPage;

    public int PageNumber { get; }

    /// <summary>
    /// Kiểm tra xem page này có phải trang hiện tại ko
    /// </summary>
    public bool IsActive => PageNumber == _currentPage;

    public PageNumberItem(int pageNumber, int currentPage)
    {
        PageNumber = pageNumber;
        _currentPage = currentPage;
    }
}
