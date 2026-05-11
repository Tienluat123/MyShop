using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace MyShop.Helpers;

public class BulkObservableCollection<T> : ObservableCollection<T>
{
    private bool _isSuppressingNotifications;

    public void BeginBulkUpdate()
    {
        _isSuppressingNotifications = true;
    }

    public void EndBulkUpdate()
    {
        _isSuppressingNotifications = false;
        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (!_isSuppressingNotifications)
        {
            base.OnCollectionChanged(e);
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (!_isSuppressingNotifications)
        {
            base.OnPropertyChanged(e);
        }
    }

    public void AddRange(IEnumerable<T> items)
    {
        if (items == null) return;

        BeginBulkUpdate();
        try
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }
        finally
        {
            EndBulkUpdate();
        }
    }

    public void ReplaceAll(IEnumerable<T> items)
    {
        BeginBulkUpdate();
        try
        {
            Clear();
            if (items != null)
            {
                foreach (var item in items)
                {
                    Add(item);
                }
            }
        }
        finally
        {
            EndBulkUpdate();
        }
    }
}
