using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyShop.ViewModels;

// Class này là móng nhà, các ViewModel khác sẽ kế thừa nó
public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}