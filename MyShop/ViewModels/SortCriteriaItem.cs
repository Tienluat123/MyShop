using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MyShop.ViewModels;

public partial class SortCriteriaItem : ObservableObject
{
    [ObservableProperty]
    private string columnName = string.Empty;

    [ObservableProperty]
    private string direction = "ASC"; // ASC or DESC

    public ObservableCollection<string> AvailableColumns { get; set; } = new ObservableCollection<string>();

    public SortCriteriaItem()
    {
    }

    public SortCriteriaItem(ObservableCollection<string> availableColumns)
    {
        AvailableColumns = availableColumns;
        if (availableColumns.Count > 0)
        {
            ColumnName = availableColumns[0];
        }
    }
}
