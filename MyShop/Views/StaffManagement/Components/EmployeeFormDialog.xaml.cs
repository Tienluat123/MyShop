using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Di;
using MyShop.Models;
using MyShop.ViewModels.StaffManagement;

namespace MyShop.Views.StaffManagement.Dialogs;

public sealed partial class EmployeeFormDialog : ContentDialog
{
    public EmployeeFormViewModel ViewModel { get; private set; }
    public User? SavedUser { get; private set; }
    private User? _userToEdit;

    public EmployeeFormDialog(EmployeeFormViewModel? viewModel = null)
    {
        this.InitializeComponent();

        ViewModel = viewModel ?? ServiceContainer.Instance.Get<EmployeeFormViewModel>();
        this.DataContext = ViewModel;


        this.Loaded += EmployeeFormDialog_Loaded;
    }

    private void EmployeeFormDialog_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Initialize(_userToEdit);
        DialogTitleText.Text = _userToEdit == null ? "New Employee" : "Edit Employee";

        // Nếu là chế độ Edit, không bắt buộc nhập lại pass trừ khi muốn đổi
        if (_userToEdit != null)
        {
            TxtPassword.Password = "";
        }
    }

    private void CustomSaveButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Password = TxtPassword.Password;

        ViewModel.SaveCommand.Execute(null);

        // 3. Kiểm tra lỗi Validation
        if (!string.IsNullOrEmpty(ViewModel.ErrorMessage))
        {
            return;
        }
        else
        {
            SavedUser = ViewModel.GetUpdatedUser();

            this.Hide();
        }
    }

    // Xử lý khi bấm nút "Cancel" màu xám
    private void CustomCancelButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CancelCommand.Execute(null);
        SavedUser = null;

        // Trigger secondary button close result
        this.Hide();
    }

    public void SetEmployee(User? user)
    {
        _userToEdit = user;
        if (this.IsLoaded)
        {
            ViewModel.Initialize(user);
            DialogTitleText.Text = user == null ? "New Employee" : "Edit Employee";
        }
    }
}