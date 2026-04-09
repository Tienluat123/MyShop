# Loyalty Service Integration Guide

Tài liệu này hướng dẫn các lập trình viên khác trong dự án cách tích hợp và sử dụng `ILoyaltyService` vào các module chức năng khác (như Quản lý Đơn hàng - Order, Quản lý Khách hàng - Customer).

## 1. Giới thiệu chung (`ILoyaltyService`)

Module Loyalty (thẻ thành viên/tích điểm) đã được cấu hình hoàn thiện và đăng ký vào DI Container (`App.xaml.cs`). Hệ thống tự động tính điểm, quy đổi điểm ra tiền, và **tự động phân hạng** thẻ cho khách hàng (Bronze, Silver, Gold, Platinum).

Để sử dụng, bạn chỉ cần Inject `ILoyaltyService` vào ViewModel hoặc Service của bạn.

```csharp
public class OrderService : IOrderService
{
    private readonly ILoyaltyService _loyaltyService;

    public OrderService(ILoyaltyService loyaltyService)
    {
        _loyaltyService = loyaltyService;
    }
}
```

---

## 2. Các hàm cốt lõi dành cho Module Khác

### 2.1. Tích điểm sau khi mua hàng (`CalculatePointsAsync`)
Hàm này cộng điểm cho khách hàng dựa trên tổng tiền đơn hàng và tính năng tự động nâng hạng thẻ.
**Lưu ý:** Hàm này sẽ tự động lưu thông tin khách hàng vào Database (gọi SaveChanges ngầm), bạn không cần phải manual Update Customer lần nữa.

```csharp
// Gọi hàm này sau khi đơn hàng thanh toán thành công
int pointsEarned = await _loyaltyService.CalculatePointsAsync(currentCustomer, orderTotalAmount);

// pointsEarned là số điểm khách hàng vừa nhận được, bạn có thể in ra hoá đơn.
```

### 2.2. Đổi điểm lấy mã giảm giá (`RedeemPointsAsync`)
Khi khách hàng muốn dùng điểm để trừ tiền thanh toán, gọi hàm này **trước** khi tính tổng tiền cuối cùng của đơn hàng.

```csharp
try 
{
    int pointsToUse = 500; // Số điểm khách muốn dùng
    
    // Hệ thống sẽ trừ điểm của khách và trả về số tiền VND được giảm
    decimal discountValue = await _loyaltyService.RedeemPointsAsync(currentCustomer, pointsToUse);
    
    // Áp dụng discountValue vào hoá đơn...
    finalTotal = orderTotalAmount - discountValue;
}
catch (InvalidOperationException ex)
{
    // Bắt lỗi nếu khách không đủ điểm hoặc nhập số âm
    ShowError(ex.Message); 
}
```

---

## 3. Quy trình thực tế ráp vào Order Management
Khi xử lý nghiệp vụ Thanh toán đơn hàng (Checkout), trình tự chuẩn nên là:

1. **Khách hàng áp dụng điểm (tuỳ chọn):**
   Nếu khách muốn xài điểm, gọi `RedeemPointsAsync(...)` để lấy số tiền giảm giá và trừ điểm của khách.
   
2. **Thanh toán đơn hàng:**
   Tạo record `Order` vào database với tổng tiền cuối cùng.

3. **Cộng điểm thưởng:**
   Dựa trên phần tiền khách hàng KHÔNG DÙNG ĐIỂM HOẶC VOUCHER (SubTotal Payment), gọi `CalculatePointsAsync(..., finalAmount)` để cộng điểm thưởng và xét duyệt thăng hạng.

---

## 4. Những thay đổi về Model cần lưu ý
Bảng `Customers` đã được thay đổi thuật toán lưu trữ điểm, cụ thể có hai cột tách biệt:
*   `TotalAccumulatedPoints` (Tổng điểm tích luỹ từ xưa tới nay - Vĩnh viễn tăng, KHÔNG BAO GIỜ TRỪ). Dùng để **xét hạng thành viên**.
*   `CurrentAvailablePoints` (Điểm khả dụng hiện tại - Tăng khi nhận thưởng, BỊ TRỪ khi đổi thưởng). Dùng để **mua sắm**.

Nếu bạn code phần hiển thị ở UI `CustomerProfile` hay `PosView`, hãy nhớ dùng `CurrentAvailablePoints` để hiển thị điểm mà khách có thể xài! Đừng dùng `TotalAccumulatedPoints` nhé.
