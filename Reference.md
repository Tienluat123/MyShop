## 1. Giải thích cấu trúc

Dưới đây là các thành phần cốt lõi được sử dụng trong mã nguồn:

| **Thành phần** | **Giải thích** |
| --- | --- |
| MVVM | Tách biệt Giao diện (XAML) và Logic xử lý (ViewModel). |
| [ObservableProperty] | Tự động cập nhật UI khi biến trong C# thay đổi (Binding). |
| [RelayCommand] | Biến hàm thành một "Lệnh" để nút bấm trên giao diện gọi tới. |
| IRepository<T> | Lớp trung gian quản lý dữ liệu (CRUD), giúp ViewModel không phụ thuộc vào Database. |
| Paging Logic | Sử dụng Skip() và Take() để tối ưu khi hiển thị danh sách lớn. |

## 2. ViewModel Implementation

## Properties

```csharp
[ObservableProperty] private ObservableCollection<Customer> customers = new();
[ObservableProperty] private string searchText = string.Empty;
[ObservableProperty] private string selectedStatus = "All";
[ObservableProperty] private int currentPage = 1;
[ObservableProperty] private int pageSize = 10;

private readonly IRepository<Customer> _repository;
```

## LoadCustomers (Logic lọc & Phân trang)

```
[RelayCommand]
private async Task LoadCustomers()
{
    var result = await _repository.GetAllAsync();
    
    // Lọc theo tên
    if (!string.IsNullOrWhiteSpace(SearchText))
        result = result.Where(c => c.FirstName.Contains(SearchText)).ToList();
        
    // Lọc theo trạng thái
    if (SelectedStatus != "All")
        result = result.Where(c => c.Status == SelectedStatus).ToList();
        
    // Logic phân trang
    var paged = result.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
    Customers = new ObservableCollection<Customer>(paged);
}
```

## Search & Filter Commands

```csharp
[RelayCommand]
private async Task SearchCustomers()
{
    CurrentPage = 1;
    await LoadCustomers();
}

[RelayCommand]
private async Task FilterByStatus(string status)
{
    SelectedStatus = status;
    CurrentPage = 1;
    await LoadCustomers();
}
```

## Save & Validation (Upsert Logic)

[RelayCommand]
private async Task SaveCustomer()
{
    if (!ValidateForm()) return;
    
    var customer = EditingCustomer ?? new Customer();
    customer.FirstName = FirstName;
    customer.Email = Email;
    customer.Phone = Phone;
    
    if (customer.Id == 0) // Thêm mới
    {
        customer.CreatedDate = DateTime.Now;
        await _repository.AddAsync(customer);
    }
    else // Cập nhật
    {
        await _repository.UpdateAsync(customer);
    }
    
    await Shell.Current.GoToAsync("../");
}

private bool ValidateForm()
{
    if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(Email))
        return false;
        
    try { new System.Net.Mail.MailAddress(Email); }
    catch { return false; }
    
    return System.Text.RegularExpressions.Regex.IsMatch(Phone, @"^\d{10}$");
}

## 3. UI (XAML)

## ListTab.xaml (Danh sách & Tìm kiếm)

```csharp
<Entry Text="{Binding SearchText, Mode=TwoWay}" Placeholder="Nhập tên khách hàng..."/>

<Button Text="All" Command="{x:Bind FilterByStatusCommand}" CommandParameter="All"/>
<Button Text="Active" Command="{x:Bind FilterByStatusCommand}" CommandParameter="Active"/>

<CollectionView ItemsSource="{x:Bind Customers}">
    <CollectionView.ItemTemplate>
        <DataTemplate>
            <StackLayout Padding="10">
                <Label Text="{x:Bind FirstName}" FontAttributes="Bold"/>
                <Label Text="{x:Bind Email}" FontSize="12" Color="Gray"/>
            </StackLayout>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>

<StackLayout Orientation="Horizontal" HorizontalOptions="Center">
    <Button Text="Prev" Command="{x:Bind LoadPageCommand}" CommandParameter="{x:Bind CurrentPage - 1}"/>
    <Label Text="{x:Bind CurrentPage, StringFormat='Trang {0}'}" VerticalOptions="Center"/>
    <Button Text="Next" Command="{x:Bind LoadPageCommand}" CommandParameter="{x:Bind CurrentPage + 1}"/>
</StackLayout>
```

## FormView.xaml (Form nhập liệu)
<Entry Placeholder="First Name" Text="{x:Bind FirstName, Mode=TwoWay}"/>
<Entry Placeholder="Email" Text="{x:Bind Email, Mode=TwoWay}" Keyboard="Email"/>
<Entry Placeholder="Phone" Text="{x:Bind Phone, Mode=TwoWay}" Keyboard="Telephone"/>

<Button Text="Lưu khách hàng" Command="{x:Bind SaveCustomerCommand}" BackgroundColor="Green"/>
