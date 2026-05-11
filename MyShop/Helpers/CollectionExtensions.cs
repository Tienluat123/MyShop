using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MyShop.Helpers;

public static class CollectionExtensions
{
    public static void ReplaceRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        if (collection == null) return;
        
        collection.Clear();
        if (items != null)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }

    // A slightly more advanced version that avoids clearing if items are the same
    // but for simplicity in WinUI, a Clear and Add is often fine IF wrapped in a way 
    // that the UI doesn't refresh on every Add.
    // Note: Standard ObservableCollection doesn't support suppression of notifications.
}
