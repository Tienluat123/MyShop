using System;
using System.Collections;
using System.Linq;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

public class CollectionContainsToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is IEnumerable collection && parameter != null)
        {
            foreach (var item in collection)
            {
                if (item.ToString() == parameter.ToString()) return true;
            }
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
