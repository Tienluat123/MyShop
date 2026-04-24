using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.ViewModels;

public abstract class ListManagementViewModelBase : ObservableObject
{
    private CancellationTokenSource? _debounceToken;

    protected void RunDebounced(Func<Task> action, int delayMs = 300)
    {
        _debounceToken?.Cancel();
        _debounceToken = new CancellationTokenSource();

        _ = RunDebouncedInternalAsync(action, _debounceToken.Token, delayMs);
    }

    protected static string BuildPageInfoDisplay(int currentPage, int totalPages)
    {
        var safeTotal = Math.Max(1, totalPages);
        var safeCurrent = Math.Clamp(currentPage, 1, safeTotal);
        return $"Page {safeCurrent} / {safeTotal}";
    }

    protected static string BuildPagingSummary(
        int currentPage,
        int pageSize,
        int totalItems,
        int currentItemsCount,
        string emptyMessage,
        string itemLabel)
    {
        if (totalItems <= 0 || currentItemsCount <= 0)
        {
            return emptyMessage;
        }

        var start = ((Math.Max(currentPage, 1) - 1) * pageSize) + 1;
        var end = Math.Min(start + currentItemsCount - 1, totalItems);
        return $"Showing {start}-{end} of {totalItems} {itemLabel}";
    }

    private static async Task RunDebouncedInternalAsync(Func<Task> action, CancellationToken token, int delayMs)
    {
        try
        {
            await Task.Delay(delayMs, token);
            await action();
        }
        catch (OperationCanceledException)
        {
            // Ignore canceled debounce runs.
        }
    }
}