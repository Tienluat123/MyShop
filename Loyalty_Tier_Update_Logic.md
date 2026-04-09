# Cơ Chế Cập Nhật Hạng Thành Viên Liên Tục

Tài liệu này giải thích logic kỹ thuật đằng sau cách hệ thống MyShop tự động cập nhật hạng thành viên (Loyalty Tier) cho khách hàng một cách liên tục và chính xác.

## 1. Nguyên lý: Cập nhật dựa trên Sự kiện (Event-Driven)

Thay vì chạy một tiến trình ngầm kiểm tra toàn bộ database (gây tốn tài nguyên), hệ thống sử dụng cơ chế **Cập nhật ngay khi phát sinh giao dịch**. Mỗi lần khách hàng mua hàng và tích điểm, đó chính là "sự kiện" để hệ thống tái đánh giá hạng của họ.

## 2. Quy trình Kỹ thuật (Workflow)

Logic này tập trung hoàn toàn tại hàm `CalculatePointsAsync` trong `LoyaltyService`.

### Bước 1: Thu thập cấu hình mới nhất
Hệ thống không hard-code (viết chết) các mốc điểm trong mã nguồn. Mỗi khi tính điểm, Service sẽ lấy dữ liệu cấu hình từ bảng `LoyaltyTierConfigs` (do Admin quản lý trên UI) lên:
- Tỷ lệ tích điểm hiện tại.
- Các ngưỡng điểm của từng hạng (Bronze, Silver, Gold, Platinum).

### Bước 2: Tính toán và Tích lũy
Dựa trên giá trị đơn hàng, hệ thống tính số điểm thưởng mới và cộng dồn vào thuộc tính:
- `TotalAccumulatedPoints`: Tổng điểm tích lũy trọn đời.

### Bước 3: Tái đánh giá hạng (Tier Re-evaluation)
Hệ thống gọi hàm nội bộ `DetermineTier`. Hàm này thực hiện:
1. Sắp xếp các hạng thành viên theo số điểm yêu cầu giảm dần (từ Platinum xuống Bronze).
2. So sánh `TotalAccumulatedPoints` của khách hàng với các ngưỡng này.
3. Hạng cao nhất mà khách đạt đủ điều kiện sẽ được chọn làm hạng mới.

### Bước 4: Kiểm tra sự thay đổi và Cập nhật
```csharp
var newTier = DetermineTier(tiers, customer.TotalAccumulatedPoints);

// Chỉ cập nhật nếu có sự thay đổi hạng (ví dụ từ Bronze -> Silver)
if (newTier != customer.Tier)
{
    customer.Tier = newTier;
}
```

### Bước 5: Lưu trữ đồng bộ
Cuối cùng, lệnh `_customerRepo.SaveChanges()` được gọi để lưu cả **Điểm mới** và **Hạng mới** xuống Database trong cùng một lượt xử lý.

## 3. Lợi ích của cơ chế này

1.  **Hiệu năng cao:** Chỉ tính toán cho đúng khách hàng vừa phát sinh mua sắm, không làm chậm hệ thống.
2.  **Tính nhất quán (Real-time):** Khách hàng thấy hạng mới của mình ngay lập tức trên màn hình thanh toán hoặc hóa đơn.
3.  **Linh hoạt:** Nếu Admin thay đổi chính sách hạng (ví dụ: hạ mốc lên Gold từ 20k xuống 15k điểm), khách hàng sẽ được "nâng hạng tự động" ngay trong lần mua hàng tiếp theo mà không cần can thiệp thủ công.

---
*Ghi chú: Để đảm bảo cơ chế này hoạt động, hãy luôn sử dụng `ILoyaltyService.CalculatePointsAsync` mỗi khi kết thúc một giao dịch thành công.*
