# TÀI LIỆU YÊU CẦU: HỆ THỐNG KHÁCH HÀNG THÂN THIẾT (LOYALTY SYSTEM)

> **Mục tiêu:** Triển khai hạ tầng quản lý điểm thưởng, hạng thành viên và các quy tắc đổi thưởng trong hệ thống POS.

---

## 1. Danh sách công việc cần triển khai (Implementation Plan)

| Hạng mục | Tính năng / Phương thức | File ảnh hưởng | Trạng thái |
| :--- | :--- | :--- | :---: |
| **Setup** | Khởi tạo lớp `LoyaltyRulesViewModel` | `LoyaltyRulesViewModel.cs` | 📝 |
| **Logic** | `LoadLoyaltyRules()`: Tải cấu hình hiện tại | `LoyaltyRulesViewModel.cs` | ⬜ |
| **Logic** | `SetPointsPerVND()`: Thiết lập tỉ lệ điểm/VND | `LoyaltyRulesViewModel.cs` | ⬜ |
| **Logic** | `SetLoyaltyTiers()`: Cấu hình phân hạng thành viên | `LoyaltyRulesViewModel.cs` | ⬜ |
| **Logic** | `SetRedemptionRules()`: Quy tắc đổi điểm sang tiền | `LoyaltyRulesViewModel.cs` | ⬜ |
| **Logic** | `CalculatePoints()`: Tính điểm cho đơn hàng | `LoyaltyRulesViewModel.cs` | ⬜ |
| **UI** | Binding dữ liệu Loyalty Rules | `LoyaltyRulesView.xaml` | ⬜ |
| **UI** | Binding cấu hình phân hạng (Tier) | `LoyaltyRulesView.xaml` | ⬜ |

---

## 2. Chi tiết tính năng & Kiểm thử (Feature Requirements)

### ✅ Tính năng cốt lõi (Core Features)
- [ ] **Tải cấu hình:** Hiển thị đúng tỉ lệ tích điểm trên mỗi giao dịch.
- [ ] **Thiết lập hệ số hạng:** Cấu hình hệ số nhân điểm (Bronze 1x, Silver 1.5x, Gold 2x, Platinum 2.5x).
- [ ] **Tính toán điểm thưởng:** 
    - Ví dụ: Đơn hàng 100,000 VND.
    - Hạng Đồng: Thu về 100 điểm. (1000VND đổi được 1đ)
    - Hạng Kim cương: Thu về 250 điểm.
- [ ] **Đổi điểm thưởng:** Khấu trừ điểm tương ứng với giảm giá (1 điểm = 100 VND).
- [ ] **Tính toán phân hạng:** Tự động nâng hạng khi đạt mốc (5000+ điểm lên hạng Bạc).

### 🛠 Ràng buộc dữ liệu (Data Validation)
- [ ] Tỉ lệ điểm trên mỗi VND phải lớn hơn 0.
- [ ] Tên các hạng thành viên không được để trống.
- [ ] Không cho phép đổi số điểm lớn hơn số dư hiện có của khách hàng.

### 🎨 Giao diện & Trải nghiệm (UI/UX)
- [ ] Cập nhật lại UI cấu hình LoyalRules cho phù hợp với các phân hạng hiện tại
- [ ] Phân hạng hiển thị cùng thông tin rõ ràng khách hàng ở trang customer management 

---

## 3. Cấu trúc Kỹ thuật (Technical Structure)

### ViewModel: `LoyaltyRulesViewModel.cs`

```csharp
[RelayCommand] 
private async Task LoadLoyaltyRules() { /* Tải cấu hình từ DB */ }

[RelayCommand] 
private async Task SetPointsPerTransaction(decimal points) { /* Cập nhật tỉ lệ tích điểm */ }

[RelayCommand] 
private async Task SetLoyaltyTiers(List<LoyaltyTier> tiers) { /* Cập nhật danh sách hạng */ }

[RelayCommand] 
private async Task SetRedemptionRules(decimal pointValue) { /* Quy tắc đổi điểm */ }

[RelayCommand] 
private async Task CalculatePoints(decimal orderTotal) { /* Logic tính điểm dựa trên hạng */ }

[RelayCommand] 
private async Task RedeemPoints(int customerId, int points) { /* Thực hiện đổi điểm */ }
```

### Danh sách File chỉnh sửa
- **Logic:** `ViewModels/LoyaltyRulesViewModel.cs`
- **Giao diện:** `Views/CustomerManagement/Components/LoyaltyRulesView.xaml`

---

## 4. Dữ liệu mẫu & Nghiệp vụ (Data & Business Rules)

### Dữ liệu mẫu cấu hình hạng (Loyalty Tiers)
| Hạng | Hệ số nhân | Điểm tối thiểu (MinPoints) |
| :--- | :---: | :--- |
| **Bronze** | 1.0x | 0 |
| **Silver** | 1.5x | 5,000 |
| **Gold** | 2.0x | 15,000 |
| **Platinum** | 2.5x | 30,000 |

### Quy định nghiệp vụ
1. **Nâng hạng tự động:** Hệ thống quét và tự động cập nhật hạng thành viên ngay khi khách hàng đạt đủ cột mốc `MinPoints`.
2. **Công thức tính điểm:** `Số điểm = (Giá trị đơn hàng / Tỉ lệ cấu hình) * Hệ số nhân của hạng`.
3. **Ưu tiên:** Quy tắc Loyalty được áp dụng sau khi đã trừ các khuyến mãi giảm giá trực tiếp trên đơn hàng.
