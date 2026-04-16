using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MyShop.Views.StaffManagement.Dialogs;

public sealed partial class SelectStaffDialog : ContentDialog
{
    public ObservableCollection<User> StaffList { get; } = new();

    // Thuộc tính trả về cho ViewModel
    public User? SelectedUser { get; private set; }
    public string SelectedStatus { get; private set; } = "Scheduled";
    public string SelectedNote { get; private set; } = string.Empty;

    // Biến phụ để biết người dùng nhấn nút nào (vì mình dùng nút tự chế)
    public ContentDialogResult Result { get; private set; } = ContentDialogResult.None;
    public bool IsDeleteRequested { get; private set; } = false;

    public SelectStaffDialog(List<User>? staff, User? existingUser = null, string? existingStatus = null, string? existingNote = null)
    {
        System.Diagnostics.Debug.WriteLine($"[SelectStaffDialog] CONSTRUCTOR called. Staff count: {staff?.Count ?? 0}");

        this.InitializeComponent();

        if (staff != null)
        {
            foreach (var user in staff)
            {
                StaffList.Add(user);
            }
        }

        System.Diagnostics.Debug.WriteLine($"   StaffList now has: {StaffList.Count} items");

        if (existingUser != null)
        {
            Title = "Edit Assignment";
            StaffListView.SelectedItem = StaffList.FirstOrDefault(u => u.Id == existingUser.Id);
            StatusCombo.SelectedItem = StatusCombo.Items.Cast<ComboBoxItem>()
                                        .FirstOrDefault(i => i.Content.ToString() == existingStatus);
            NoteBox.Text = existingNote ?? "";

            // Hiện nút xóa (mình sẽ thêm ở XAML)
            DeleteButton.Visibility = Visibility.Visible;
            CustomAssignButton.Content = "Update"; // Đổi chữ nút Cam
        }

        System.Diagnostics.Debug.WriteLine($"   Constructor complete. Ready to show.");

        // Add Loaded event
        this.Loaded += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"[SelectStaffDialog] LOADED event fired!");
            System.Diagnostics.Debug.WriteLine($"   CustomAssignButton: {CustomAssignButton}");
            System.Diagnostics.Debug.WriteLine($"   StaffListView.Items.Count: {StaffListView.Items.Count}");
        };
    }

    private void CustomAssignButton_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[SelectStaffDialog] ASSIGN button clicked!");
        System.Diagnostics.Debug.WriteLine($"   StaffListView.SelectedItem = {StaffListView.SelectedItem}");
        System.Diagnostics.Debug.WriteLine($"   StaffListView.SelectedIndex = {StaffListView.SelectedIndex}");
        System.Diagnostics.Debug.WriteLine($"   StaffList.Count = {StaffList.Count}");

        SelectedUser = StaffListView.SelectedItem as User;

        if (SelectedUser == null)
        {
            System.Diagnostics.Debug.WriteLine("[SelectStaffDialog] ERROR: No user selected!");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[SelectStaffDialog] Selected user: ID={SelectedUser.Id}, Name={SelectedUser.FullName}");

        // Lưu Note & Status
        SelectedNote = NoteBox.Text;
        if (StatusCombo.SelectedItem is ComboBoxItem item)
        {
            SelectedStatus = item.Content.ToString() ?? "Scheduled";
        }

        System.Diagnostics.Debug.WriteLine($"[SelectStaffDialog] Setting Result=Primary and calling Hide()");

        // Đánh dấu là thành công và đóng bảng
        this.Result = ContentDialogResult.Primary;
        this.Hide();
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[SelectStaffDialog] DELETE button clicked!");
        IsDeleteRequested = true;
        this.Result = ContentDialogResult.Secondary;
        this.Hide();
    }

    // 2. Xử lý khi bấm nút "Cancel" tự chế
    private void CustomCancelButton_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[SelectStaffDialog] CANCEL button clicked!");
        this.Result = ContentDialogResult.None;
        this.SelectedUser = null;
        this.Hide();
    }
}