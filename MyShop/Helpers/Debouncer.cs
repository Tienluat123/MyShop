using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyShop.Helpers;

public class Debouncer
{
    private CancellationTokenSource? _cts;

    public void Debounce(Action action, int milliseconds = 300)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        Task.Delay(milliseconds, token).ContinueWith(t =>
        {
            if (!t.IsCanceled)
            {
                action();
            }
        }, token);
    }

    public async Task DebounceAsync(Func<Task> action, int milliseconds = 300)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        try
        {
            await Task.Delay(milliseconds, token);
            if (!token.IsCancellationRequested)
            {
                await action();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
