# 📋 Yêu Cầu Chung Cho Tất Cả Các Ứng Dụng

## 🎯 Tổng Quát

Tài liệu này định nghĩa các yêu cầu chung mà tất cả các ứng dụng phải tuân thủ, dựa trên kiến trúc MVVM đã được thiết lập.

---

## 1️⃣ YÊU CẦU KIẾN TRÚC

### 1.1 Mô Hình MVVM

Tất cả các ứng dụng **PHẢI** tuân theo mô hình **Model-View-ViewModel**:

```
View Layer           →  Hiển thị dữ liệu (XAML)
   ↕ Data Binding
ViewModel Layer      →  Quản lý logic & state (C#)
   ↕ Service Call
Service Layer        →  Business logic (C#)
   ↕ Repository Call
DataAccess Layer     →  Truy cập dữ liệu (C#)
   ↕ Database Query
Data Layer           →  Entity Models (C#)
```

**Quy tắc:**
- ❌ **KHÔNG** được có logic trong Code-Behind (*.xaml.cs)
- ✅ **PHẢI** move tất cả logic vào ViewModel
- ✅ **PHẢI** dùng Data Binding thay vì code-behind

### 1.2 Cấu Trúc Thư Mục

```
ProjectName/
├── Model/
│   ├── BaseModel.cs              (Lớp cơ sở)
│   ├── Entity1.cs
│   ├── Entity2.cs
│   └── ...
├── ViewModel/
│   ├── BaseViewModel.cs          (Lớp cơ sở)
│   ├── RelayCommand.cs           (Command implementation)
│   ├── MainViewModel.cs
│   ├── ViewModel1.cs
│   └── ...
├── View/
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── Page1.xaml
│   ├── Page1.xaml.cs
│   ├── Control/
│   │   ├── CustomControl1.xaml
│   │   └── ...
│   └── Form/
│       ├── AddForm.xaml
│       └── ...
├── Service/
│   ├── Services.cs               (Service registration)
│   ├── IService1.cs
│   ├── Service1.cs
│   └── ...
├── DataAccess/
│   ├── IRepository.cs            (Generic interface)
│   ├── Repository.cs
│   ├── PagedResult.cs
│   ├── PagingRequest.cs
│   └── ...
├── Extension/
│   └── CustomBehaviors.cs        (Attached behaviors)
├── Converter/
│   └── CustomConverters.cs       (XAML converters)
└── Assets/
    ├── Images/
    ├── Icons/
    └── ...
```

---

## 2️⃣ YÊU CẦU MODEL LAYER

### 2.1 Entity Base Class

**PHẢI** có lớp `BaseModel` implement `INotifyPropertyChanged`:

```csharp
namespace YourApp.Model
{
    public abstract class BaseModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
```

### 2.2 Entity Properties

**Quy tắc cho Properties:**

```csharp
public class Student : BaseModel
{
    // ❌ KHÔNG
    // public string Name { get; set; }
    
    // ✅ PHẢI (với backing field)
    private string _name = "";
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }
    
    // ✅ CÓ THỂ (nếu không cần thay đổi)
    public int ID { get; init; }
    
    // ✅ PHẢI - Tất cả public properties
    public string Email { get; set; }
}
```

---

## 3️⃣ YÊU CẦU VIEWMODEL LAYER

### 3.1 BaseViewModel

**PHẢI** có:

```csharp
public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
```

### 3.2 RelayCommand

**PHẢI** implement `ICommand` với signature sau:

```csharp
public class RelayCommand : ICommand
{
    private readonly Action<object?>? _execute;
    private readonly Func<object?, bool>? _canExecute;
    
    public event EventHandler? CanExecuteChanged;
    
    public RelayCommand(
        Action<object?>? execute, 
        Func<object?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }
    
    public bool CanExecute(object? parameter) 
        => _canExecute?.Invoke(parameter) ?? true;
    
    public void Execute(object? parameter) 
        => _execute?.Invoke(parameter);
    
    public void RaiseCanExecuteChanged() 
        => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
```

### 3.3 ViewModel Class

**PHẢI** inherits từ `BaseViewModel`:

```csharp
public class MyViewModel : BaseViewModel
{
    // ✅ PHẢI - Có các command
    public RelayCommand LoadCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand DeleteCommand { get; }
    
    // ✅ PHẢI - Có ObservableCollection
    public ObservableCollection<Item> Items { get; set; } = new();
    
    // ✅ PHẢI - Có binding properties
    private Item? _selectedItem;
    public Item? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (_selectedItem != value)
            {
                _selectedItem = value;
                OnPropertyChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }
    }
    
    public MyViewModel()
    {
        LoadCommand = new RelayCommand(_ => OnLoad());
        SaveCommand = new RelayCommand(_ => OnSave(), _ => CanSave);
        DeleteCommand = new RelayCommand(
            _ => OnDelete(), 
            _ => _selectedItem != null);
    }
    
    // ✅ PHẢI - Có private methods
    private void OnLoad() { /* logic */ }
    private void OnSave() { /* logic */ }
    private void OnDelete() { /* logic */ }
    private bool CanSave => !string.IsNullOrEmpty(/* some condition */);
}
```

### 3.4 Naming Convention

| Loại | Format | Ví Dụ |
|------|--------|-------|
| **Public Property** | PascalCase | `SelectedStudent`, `IsLoading` |
| **Public Command** | PascalCase + "Command" | `DeleteCommand`, `SaveCommand` |
| **Private Field** | _camelCase | `_selectedStudent`, `_service` |
| **Private Method** | _camelCase | `_loadData()`, `_validateInput()` |
| **Collection** | Plural + PascalCase | `Students`, `Orders`, `Items` |

---

## 4️⃣ YÊU CẦU VIEW LAYER

### 4.1 Code-Behind

**QUY TẮC VÀNG:**

```csharp
// ❌ LOẠI BỎ 100%
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();  // ✅ CHỈ CÓI DÒNG NÀY
    }
}
```

**KHÔNG** được:
- ❌ Viết event handler trong code-behind
- ❌ Update UI trực tiếp từ code-behind
- ❌ Gọi Service từ code-behind
- ❌ Có business logic trong code-behind

### 4.2 XAML Binding

**Yêu cầu bắt buộc:**

```xml
<!-- ✅ PHẢI: Binding to ViewModel -->
<TextBlock Text="{Binding StudentName}" />

<!-- ✅ PHẢI: Command binding -->
<Button Command="{Binding DeleteCommand}" Content="Xóa" />

<!-- ✅ PHẢI: ObservableCollection binding -->
<ListBox ItemsSource="{Binding Students}" />

<!-- ✅ PHẢI: Two-way binding khi cần update -->
<TextBox Text="{Binding Name, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />

<!-- ❌ KHÔNG: Code-behind event -->
<!-- <Button Click="Button_Click" /> -->

<!-- ❌ KHÔNG: Direct access to View -->
<!-- <Button Click="{Binding DoSomething}" /> (Sai) -->
```

### 4.3 ItemsSource cho Collection

```xml
<!-- ✅ PHẢI -->
<ListBox ItemsSource="{Binding Students}"
         SelectedItem="{Binding SelectedStudent, Mode=TwoWay}">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <StackPanel>
                <TextBlock Text="{Binding Name}" />
                <TextBlock Text="{Binding Email}" />
            </StackPanel>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

---

## 5️⃣ YÊU CẦU SERVICE LAYER

### 5.1 Service Interface

**Mỗi Service PHẢI có Interface:**

```csharp
public interface IStudentService
{
    PagedResult<Student> GetAll(PagingRequest? paging = null);
    Student? GetById(int id);
    Student Add(Student student);
    void Update(Student student);
    void Delete(int id);
}

public class StudentService : IStudentService
{
    private readonly IRepository<Student> _repository;
    
    public StudentService(IRepository<Student> repository)
    {
        _repository = repository;
    }
    
    public PagedResult<Student> GetAll(PagingRequest? paging = null)
    {
        return _repository.GetAll(paging);
    }
    
    public Student? GetById(int id)
    {
        return _repository.GetById(id);
    }
    
    public Student Add(Student student)
    {
        // ✅ PHẢI: Validation
        if (string.IsNullOrEmpty(student.Name))
            throw new ArgumentException("Name is required");
        
        // ✅ PHẢI: Business logic
        return _repository.Add(student);
    }
    
    public void Update(Student student)
    {
        if (student.Id <= 0)
            throw new ArgumentException("Invalid ID");
        
        _repository.Update(student);
    }
    
    public void Delete(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid ID");
        
        _repository.Delete(id);
    }
}
```

### 5.2 Service Registration

```csharp
public class Services
{
    private static readonly Dictionary<Type, object> _services = new();
    
    public static void Register<T>(T service) where T : class
    {
        _services[typeof(T)] = service ?? throw new ArgumentNullException(nameof(service));
    }
    
    public static T Get<T>() where T : class
    {
        if (!_services.TryGetValue(typeof(T), out var service))
            throw new InvalidOperationException($"Service {typeof(T).Name} not registered");
        
        return (T)service;
    }
}

// Usage in App.xaml.cs
public partial class App : Application
{
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // ✅ PHẢI: Register services ở đây
        Services.Register<IRepository<Student>>(new StudentRepository());
        Services.Register<IStudentService>(new StudentService(
            Services.Get<IRepository<Student>>()
        ));
        
        base.OnLaunched(args);
    }
}
```

---

## 6️⃣ YÊU CẦU DATA ACCESS LAYER

### 6.1 Generic Repository Interface

```csharp
public interface IRepository<TEntity> where TEntity : class
{
    PagedResult<TEntity> GetAll(PagingRequest? paging = null);
    TEntity? GetById(int id);
    TEntity Add(TEntity entity);
    void Update(TEntity entity);
    void Delete(int id);
    void SaveChanges();
}
```

### 6.2 Paging Support

**PHẢI** có:

```csharp
public class PagingRequest
{
    public int PageNumber { get; set; } = 1;  // ✅ Mặc định = 1
    public int PageSize { get; set; } = 10;   // ✅ Mặc định = 10
    
    public void Validate()
    {
        if (PageNumber < 1) PageNumber = 1;
        if (PageSize < 1) PageSize = 10;
        if (PageSize > 100) PageSize = 100;  // ✅ Giới hạn max
    }
}

public class PagedResult<TData> where TData : class
{
    public List<TData> Items { get; set; } = new();
    public PagingMetadata Pagination { get; set; } = new();
}

public class PagingMetadata
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((decimal)TotalItems / PageSize);
    public bool HasNext => PageNumber < TotalPages;
    public bool HasPrevious => PageNumber > 1;
}
```

---

## 7️⃣ YÊU CẦU ERROR HANDLING

### 7.1 Exception Handling

```csharp
// ✅ PHẢI: Try-catch ở layer thích hợp
public async void _loadData()
{
    try
    {
        IsLoading = true;
        ErrorMessage = "";
        
        var result = _service.GetAll();
        
        Items.Clear();
        foreach(var item in result.Items)
        {
            Items.Add(item);
        }
    }
    catch (InvalidOperationException ex)
    {
        ErrorMessage = $"Lỗi: {ex.Message}";
    }
    catch (Exception ex)
    {
        ErrorMessage = "Lỗi hệ thống. Vui lòng thử lại.";
        Debug.WriteLine($"ERROR: {ex}");
    }
    finally
    {
        IsLoading = false;
    }
}
```

### 7.2 Validation

```csharp
// ✅ PHẢI: Validate trong ViewModel
public RelayCommand SaveCommand { get; }

private void _save()
{
    // Validate
    if (string.IsNullOrWhiteSpace(Name))
    {
        ErrorMessage = "Tên không được trống";
        return;
    }
    
    if (Email.Length < 5)
    {
        ErrorMessage = "Email không hợp lệ";
        return;
    }
    
    try
    {
        _service.Add(new Student { Name = Name, Email = Email });
        SuccessMessage = "Thêm thành công";
        _clearForm();
    }
    catch (Exception ex)
    {
        ErrorMessage = ex.Message;
    }
}
```

---

## 8️⃣ YÊU CẦU PERFORMANCE

### 8.1 Collection Updates

```csharp
// ❌ KHÔNG: Clear rồi add từng cái (N events)
Items.Clear();
foreach(var item in newItems)
{
    Items.Add(item);
}

// ✅ PHẢI: Batch update (1 event)
private void UpdateCollection(IEnumerable<Item> newItems)
{
    Items.Clear();
    foreach(var item in newItems)
    {
        Items.Add(item);
    }
    
    // ✅ Hoặc dùng CollectionViewSource
    // var view = CollectionViewSource.GetDefaultView(Items);
    // view.Refresh();
}
```

### 8.2 Async Operations

```csharp
// ✅ PHẢI: Async/await cho heavy operations
public async void LoadDataAsync()
{
    try
    {
        IsLoading = true;
        var result = await Task.Run(() => _service.GetAll());
        
        Items.Clear();
        foreach(var item in result.Items)
        {
            Items.Add(item);
        }
    }
    finally
    {
        IsLoading = false;
    }
}

// ✅ PHẢI: async void chỉ cho event handlers
public async void _onSaveClicked()
{
    await SaveAsync();
}

// ✅ PUBLIC methods phải return Task
public async Task SaveAsync()
{
    await _service.SaveAsync();
}
```

### 8.3 Lazy Loading

```csharp
// ✅ PHẢI: Load khi cần
public ObservableCollection<Item> Items 
{ 
    get
    {
        if (_items == null)
        {
            _items = new ObservableCollection<Item>(_service.GetAll().Items);
        }
        return _items;
    }
}
```

---

## 9️⃣ YÊU CẦU DEPENDENCY INJECTION

### 9.1 Constructor Injection

```csharp
// ✅ PHẢI: Inject dependencies qua constructor
public class StudentViewModel : BaseViewModel
{
    private readonly IStudentService _service;
    private readonly ILogger _logger;
    
    public StudentViewModel(IStudentService service, ILogger logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}

// ❌ KHÔNG: New service trong ViewModel
public class StudentViewModel : BaseViewModel
{
    private StudentService _service = new();  // KHÔNG
}
```

### 9.2 Service Locator (Fallback)

```csharp
// ✅ Nếu không có DI container, dùng Service Locator
public class StudentViewModel : BaseViewModel
{
    private readonly IStudentService _service;
    
    public StudentViewModel()
    {
        _service = Services.Get<IStudentService>();
    }
}
```

---

## 🔟 YÊU CẦU NAMING CONVENTIONS

### 10.1 Classes

```csharp
// ✅ PHẢI: PascalCase
public class StudentViewModel { }
public class StudentService { }
public interface IStudentRepository { }

// ❌ KHÔNG
public class student_view_model { }
public class studentService { }
```

### 10.2 Properties & Methods

```csharp
// ✅ PHẢI: PascalCase
public string StudentName { get; set; }
public void LoadStudents() { }
public bool HasSelection { get; }

// ❌ KHÔNG
public string student_name { get; set; }
public void load_students() { }
```

### 10.3 Private Members

```csharp
// ✅ PHẢI: _camelCase
private string _studentName;
private IRepository<Student> _repository;
private void _loadStudents() { }

// ❌ KHÔNG
private string studentName;
private IRepository<Student> Repository;
```

### 10.4 Event Handlers

```csharp
// ✅ PHẢI: [ObjectName]_[EventName]
private void OnStdentSelected(object sender, SelectionChangedEventArgs e) { }

// ✅ PHẢI: On[EventName]
private void OnLoad() { }
private void OnSave() { }
private void OnDelete() { }
```

---

## 1️⃣1️⃣ YÊU CẦU DOCUMENTATION

### 11.1 XML Comments

```csharp
/// <summary>
/// Tải danh sách sinh viên từ database
/// </summary>
/// <param name="paging">Yêu cầu phân trang</param>
/// <returns>Kết quả phân trang với danh sách sinh viên</returns>
public PagedResult<Student> GetAll(PagingRequest? paging = null)
{
    return _repository.GetAll(paging);
}

/// <summary>
/// Lấy sinh viên theo ID
/// </summary>
/// <param name="id">ID sinh viên</param>
/// <returns>Student object hoặc null nếu không tìm thấy</returns>
/// <exception cref="ArgumentException">Nếu ID không hợp lệ</exception>
public Student? GetById(int id)
{
    if (id <= 0)
        throw new ArgumentException($"ID không hợp lệ: {id}");
    
    return _repository.GetById(id);
}
```

### 11.2 Comments

```csharp
// ✅ PHẢI: Giải thích TẠI SAO, không phải CÁI GÌ
public void _loadStudents()
{
    // Clear rồi add từng phần tử vì List<T> không trigger binding update
    // ObservableCollection mới mỗi lân sẽ tạo ra duplicate binding
    Students.Clear();
    foreach(var item in result.Items)
    {
        Students.Add(item);
    }
}

// ❌ KHÔNG: Giải thích code hiển nhiên
public void _loadStudents()
{
    // Lặp qua mỗi item
    foreach(var item in result.Items)
    {
        // Thêm vào Students
        Students.Add(item);
    }
}
```

---

## 1️⃣2️⃣ YÊU CẦU TESTING

### 12.1 Testable Design

```csharp
// ✅ PHẢI: Có interface để mock
public interface IStudentService
{
    PagedResult<Student> GetAll(PagingRequest? paging = null);
}

public class StudentViewModelTests
{
    [TestMethod]
    public void LoadStudents_ShouldPopulateStudents()
    {
        // Arrange
        var mockService = new Mock<IStudentService>();
        mockService
            .Setup(s => s.GetAll(It.IsAny<PagingRequest>()))
            .Returns(new PagedResult<Student> 
            { 
                Items = new() { new Student { ID = 1, Name = "Test" } }
            });
        
        var viewModel = new StudentViewModel(mockService.Object);
        
        // Act
        viewModel.LoadCommand.Execute(null);
        
        // Assert
        Assert.AreEqual(1, viewModel.Students.Count);
        Assert.AreEqual("Test", viewModel.Students[0].Name);
    }
}
```

---

## 1️⃣3️⃣ YÊU CẦU SECURITY

### 13.1 Input Validation

```csharp
// ✅ PHẢI: Validate tất cả input
public void AddStudent(string name, string email)
{
    if (string.IsNullOrWhiteSpace(name))
        throw new ArgumentException("Name is required");
    
    if (string.IsNullOrWhiteSpace(email))
        throw new ArgumentException("Email is required");
    
    if (!email.Contains("@"))
        throw new ArgumentException("Email format is invalid");
    
    _service.Add(new Student { Name = name, Email = email });
}
```

### 13.2 Access Control

```csharp
// ✅ PHẢI: Check permission trước action
public void DeleteStudent(int id)
{
    if (!CurrentUser.CanDelete)
        throw new UnauthorizedAccessException("You don't have permission to delete");
    
    _service.Delete(id);
}
```

---

## 1️⃣4️⃣ CHECKLIST TRƯỚC KHI COMMIT

### Trước khi commit code, PHẢI kiểm tra:

- [ ] **Architecture**
  - [ ] Code follow MVVM pattern
  - [ ] Không có logic trong code-behind
  - [ ] Đúng cấu trúc thư mục

- [ ] **ViewModel**
  - [ ] Inherits từ BaseViewModel
  - [ ] Có INotifyPropertyChanged
  - [ ] Có RelayCommand cho actions
  - [ ] Naming convention đúng

- [ ] **View (XAML)**
  - [ ] Binding đúng cú pháp
  - [ ] ItemsSource cho collections
  - [ ] SelectedItem two-way binding

- [ ] **Service & Repository**
  - [ ] Có interface
  - [ ] Có validation
  - [ ] Error handling đúng

- [ ] **Code Quality**
  - [ ] Không có magic numbers/strings
  - [ ] Có XML comments
  - [ ] Follow naming convention
  - [ ] Không có dead code

- [ ] **Testing**
  - [ ] Unit test cho ViewModel
  - [ ] Unit test cho Service
  - [ ] Test validation logic

- [ ] **Performance**
  - [ ] Async/await cho heavy operations
  - [ ] Proper collection updates
  - [ ] No memory leaks

- [ ] **Security**
  - [ ] Validate tất cả input
  - [ ] Check access control
  - [ ] No sensitive data in logs

---

## 1️⃣5️⃣ COMMON MISTAKES & SOLUTIONS

| Lỗi | Vấn Đề | Giải Pháp |
|-----|--------|----------|
| **UI không update** | Binding property không trigger PropertyChanged | Implement INotifyPropertyChanged đúng cách |
| **Nút không enable/disable** | Không gọi RaiseCanExecuteChanged() | Gọi trong setter khi state thay đổi |
| **Danh sách trống** | Assign `Items = newList` thay vì Clear + Add | Dùng `Clear()` rồi `Add()` từng phần tử |
| **Lag UI** | Sync call trong ViewModel | Dùng `async/await` cho heavy operations |
| **Circular reference** | ViewModel reference View | Chỉ View reference ViewModel qua DataContext |
| **Service not found** | Quên register service | Register trong `App.xaml.cs` |
| **NullReferenceException** | Access null property | Kiểm tra null trước khi access |

---

## 📚 REFERENCE TEMPLATES

### Template: New ViewModel

```csharp
using YourApp.Model;
using YourApp.Service;
using System.Collections.ObjectModel;

namespace YourApp.ViewModel
{
    public class YourViewModel : BaseViewModel
    {
        // Commands
        public RelayCommand LoadCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand DeleteCommand { get; }
        
        // Properties
        public ObservableCollection<Item> Items { get; set; } = new();
        
        private Item? _selectedItem;
        public Item? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged();
                    DeleteCommand.RaiseCanExecuteChanged();
                }
            }
        }
        
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }
        
        // Services
        private readonly IYourService _service;
        
        public YourViewModel(IYourService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            
            LoadCommand = new RelayCommand(_ => OnLoad());
            SaveCommand = new RelayCommand(_ => OnSave(), _ => CanSave);
            DeleteCommand = new RelayCommand(_ => OnDelete(), _ => _selectedItem != null);
        }
        
        // Methods
        private void OnLoad()
        {
            try
            {
                IsLoading = true;
                var result = _service.GetAll();
                Items.Clear();
                foreach(var item in result)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                // Handle error
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private void OnSave()
        {
            if (_selectedItem == null) return;
            
            try
            {
                _service.Update(_selectedItem);
                // Show success message
            }
            catch (Exception ex)
            {
                // Handle error
            }
        }
        
        private void OnDelete()
        {
            if (_selectedItem == null) return;
            
            try
            {
                _service.Delete(_selectedItem.Id);
                Items.Remove(_selectedItem);
                _selectedItem = null;
            }
            catch (Exception ex)
            {
                // Handle error
            }
        }
        
        private bool CanSave => _selectedItem != null; // Add condition
    }
}
```

### Template: New Service

```csharp
using YourApp.Model;
using YourApp.DataAccess;

namespace YourApp.Service
{
    public interface IYourService
    {
        PagedResult<YourEntity> GetAll(PagingRequest? paging = null);
        YourEntity? GetById(int id);
        YourEntity Add(YourEntity entity);
        void Update(YourEntity entity);
        void Delete(int id);
    }
    
    public class YourService : IYourService
    {
        private readonly IRepository<YourEntity> _repository;
        
        public YourService(IRepository<YourEntity> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        
        public PagedResult<YourEntity> GetAll(PagingRequest? paging = null)
        {
            return _repository.GetAll(paging);
        }
        
        public YourEntity? GetById(int id)
        {
            if (id <= 0)
                throw new ArgumentException($"Invalid ID: {id}");
            
            return _repository.GetById(id);
        }
        
        public YourEntity Add(YourEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            
            // Add validation and business logic
            return _repository.Add(entity);
        }
        
        public void Update(YourEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            
            _repository.Update(entity);
        }
        
        public void Delete(int id)
        {
            if (id <= 0)
                throw new ArgumentException($"Invalid ID: {id}");
            
            _repository.Delete(id);
        }
    }
}
```

---

**Cập nhật lần cuối:** 27/03/2026  
**Phiên bản:** 1.0

