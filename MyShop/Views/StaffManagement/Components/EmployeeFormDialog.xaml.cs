namespace MyShop.Views.StaffManagement.Components
{
    public sealed partial class EmployeeFormDialog : ContentDialog
    {
        public EmployeeFormDialog()
        {
            this.InitializeComponent();
        }

        // HÀM BƠM DỮ LIỆU CŨ VÀO FORM
        public void LoadDuLieuCu(string ten, string sdt, int viTriChucVu, int viTriTrangThai)
        {
            TxtHoTen.Text = ten;            // Gắn tên vào ô Họ Tên
            TxtSDT.Text = sdt;              // Gắn sdt vào ô SĐT
            CmbChucVu.SelectedIndex = viTriChucVu;      // Chọn sẵn chức vụ (0 là dòng đầu tiên)
            CmbTrangThai.SelectedIndex = viTriTrangThai; // Chọn sẵn trạng thái
        }
    }
}