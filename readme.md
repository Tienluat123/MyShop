# MyShop 2026 - Coffee Shop POS / Management System

## 1. Thông Tin Thành Viên Nhóm

| STT | Họ tên   | Mã số sinh viên         |
| --- | -------- | ----------------------- |
| 1   | 23127104 | Nguyễn Bình Minh Phương |
| 2   | 23127221 | Nguyễn Tiến Luật        |
| 3   | 23127228 | Phạm Văn Minh           |
| 4   | 23127281 | Đặng Nghi Văn           |
| 5   | 23127395 | Trần Anh Khoa           |

## 2. Tóm Tắt Đồ Án

MyShop là ứng dụng hỗ trợ chủ cửa hàng nhỏ quản lý hoạt động bán hàng. Ứng dụng tập trung vào các nghiệp vụ chính như bán hàng tại quầy, quản lý sản phẩm, quản lý đơn hàng, quản lý khách hàng, quản lý khuyến mãi, quản lý nhân viên, lịch làm việc và báo cáo doanh thu.

Ứng dụng được xây dựng theo hướng client - server, sử dụng cơ sở dữ liệu PostgreSQL khi demo. Kiến trúc code chính sử dụng MVVM, Dependency Injection, Service Layer và Repository để tách riêng giao diện, xử lý nghiệp vụ và truy xuất dữ liệu.

## 3. Các Chức Năng Đã Thực Hiện Theo Yêu Cầu Đồ Án

| Tên yêu cầu                        | Loại yêu cầu | Chi tiết đã thực hiện / logic chính                                                                                                                                                                                                                                                                                                       |     Điểm |
| ---------------------------------- | ------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------: |
| Đăng nhập và cấu hình server       | Cơ bản       | Có màn hình đăng nhập, kiểm tra tài khoản người dùng, phân quyền theo vai trò và màn hình cấu hình thông tin database/server lần đầu để kết nối PostgreSQL. Ứng dụng kiểm tra cấu hình database đã lưu để bỏ qua onboarding khi mở lại. Mật khẩu người dùng được xác thực bằng BCrypt, mật khẩu database trong file cấu hình được mã hóa. |     0.25 |
| Dashboard                          | Cơ bản       | Hiển thị tổng số sản phẩm, top sản phẩm sắp hết hàng, top sản phẩm bán chạy, tổng số đơn hàng trong ngày, doanh thu trong ngày và các đơn hàng gần nhất.                                                                                                                                                                                  |      0.5 |
| Products - quản lý sản phẩm        | Cơ bản       | Quản lý sản phẩm và danh mục; thêm, sửa, xóa; xem danh sách theo danh mục; tìm kiếm theo tên; lọc theo danh mục/trạng thái/tồn kho; phân trang; sắp xếp theo tiêu chí; import dữ liệu sản phẩm từ Excel; hỗ trợ bảng ProductImages và dữ liệu mẫu 3 hình cho mỗi sản phẩm.                                                                |     1.25 |
| Orders/POS - quản lý đơn hàng      | Cơ bản       | Tạo đơn hàng từ màn hình POS, thêm/xóa/cập nhật sản phẩm trong giỏ hàng, thanh toán, xem danh sách đơn hàng có phân trang, xem chi tiết, lọc theo ngày và cập nhật trạng thái đơn hàng.                                                                                                                                                   |      1.5 |
| Reports - báo cáo thống kê         | Cơ bản       | Báo cáo doanh thu, lợi nhuận, số đơn hàng theo khoảng thời gian; hỗ trợ xem theo ngày/tuần/tháng; thống kê sản phẩm và danh mục bán chạy; ghi nhận chi phí/overhead.                                                                                                                                                                      |      1.0 |
| Settings - cấu hình chương trình   | Cơ bản       | Cho phép cấu hình số lượng item mỗi trang khi phân trang, áp dụng cho các màn hình quản lý như Products, Customers, Promotions và Staff. Có lưu màn hình/chức năng cuối cùng người dùng mở để lần sau đăng nhập quay lại đúng màn hình đó.                                                                                                |     0.25 |
| Responsive layout                  | Nâng cao     | Một số màn hình sử dụng layout thích ứng, scroll container và visual state để giao diện hiển thị ổn định trên nhiều kích thước màn hình.                                                                                                                                                                                                  |      0.5 |
| Promotions - khuyến mãi giảm giá   | Nâng cao     | Quản lý promotion gồm thêm, sửa, xóa, bật/tắt trạng thái, mã khuyến mãi, loại giảm giá, giá trị giảm, thời gian hiệu lực, đơn hàng tối thiểu và áp dụng khi thanh toán.                                                                                                                                                                   |      1.0 |
| MVVM architecture                  | Nâng cao     | Ứng dụng tổ chức theo mô hình MVVM, tách View, ViewModel, command và binding để hạn chế xử lý nghiệp vụ trực tiếp trong giao diện.                                                                                                                                                                                                        |      0.5 |
| Dependency Injection               | Nâng cao     | Đăng ký và inject các service chính như ProductService, OrderService, CustomerService, PromotionService, EmployeeService, AppConfigService, NotificationService.                                                                                                                                                                          |      0.5 |
| Phân quyền Admin/Sale              | Nâng cao     | Có vai trò người dùng Admin/Sale, dùng để xác định quyền truy cập và hiển thị thông tin phù hợp trong hệ thống.                                                                                                                                                                                                                           |      0.5 |
| KPI/hoa hồng nhân viên             | Nâng cao     | Có logic tính lương theo giờ làm và thưởng KPI: +10% cho nhân viên trên 100 giờ, +15% cho nhân viên top 1 có trên 100 giờ.                                                                                                                                                                                                                |     0.25 |
| Khách hàng thân thiết              | Nâng cao     | Quản lý khách hàng, điểm tích lũy, hạng thành viên và cho phép sử dụng loyalty points trong quá trình thanh toán.                                                                                                                                                                                                                         |      0.5 |
| In/xuất hóa đơn                    | Nâng cao     | Có receipt sau thanh toán, hiển thị thông tin đơn hàng, sản phẩm, tổng tiền, phương thức thanh toán và hỗ trợ luồng in hóa đơn.                                                                                                                                                                                                           |      0.5 |
| Sắp xếp nhiều tiêu chí             | Nâng cao     | Hỗ trợ thêm nhiều cấp sắp xếp, tùy chỉnh tăng/giảm trên các danh sách Products, Customers, Promotions và Staff.                                                                                                                                                                                                                           |      0.5 |
| Tìm kiếm nâng cao/full text search | Nâng cao     | Kết hợp search text với nhiều điều kiện lọc như trạng thái, danh mục, vai trò, tồn kho; một số nghiệp vụ có full text search để tìm kiếm tốt hơn.                                                                                                                                                                                         |      1.0 |
| Onboarding/cấu hình lần đầu        | Nâng cao     | Có luồng hướng dẫn/cấu hình lần đầu để người dùng nhập thông tin PostgreSQL trước khi vào ứng dụng.                                                                                                                                                                                                                                       |      0.5 |
| AI/LLM assistant                   | Nâng cao     | Tích hợp AI assistant với hai ngữ cảnh riêng: hỏi đáp thông thường và hỗ trợ tạo promotion, tránh nhầm lẫn giữa chat thường và tạo khuyến mãi.                                                                                                                                                                                            |      2.0 |
| Đóng gói/publish ứng dụng          | Cơ bản       | Có cấu hình publish profile cho Windows theo các runtime như win-x64, win-x86 và win-arm64 để xuất bản ứng dụng.                                                                                                                                                                                                                          |     0.25 |
| Đóng gói                           | Cơ bản       |                                                                                                                                                                                                                                                                                                                                           |     0.25 |
| **Tổng điểm đạt được**             |              |                                                                                                                                                                                                                                                                                                                                           | **13.5** |

## 4. Các Chức Năng Chưa Thực Hiện

| Tên yêu cầu                                       | Loại yêu cầu | Điểm |
| ------------------------------------------------- | ------------ | ---: |
| Auto save khi tạo đơn hàng hoặc thêm mới sản phẩm | Nâng cao     | 0.25 |
| Kiến trúc plugin mở rộng động                     | Nâng cao     |  1.0 |
| Làm rối mã nguồn bằng obfuscator                  | Nâng cao     | 0.25 |
| Chế độ dùng thử 15 ngày và cơ chế kích hoạt       | Nâng cao     |  0.5 |
| Backup/restore database trực tiếp từ ứng dụng     | Nâng cao     | 0.25 |
| Sử dụng GraphQL API thay cho REST                 | Nâng cao     |  1.0 |
| Tạo test case kiểm thử chức năng                  | Nâng cao     |  0.5 |
| Kiểm thử giao diện tự động                        | Nâng cao     |  1.0 |
| Tích hợp Machine Learning riêng ngoài LLM         | Nâng cao     |  2.0 |

## 5. Các Tính Năng Bổ Sung Nhóm Đã Thực Hiện

Ngoài các yêu cầu chính trong đề, nhóm có đầu tư thêm một số chức năng mở rộng để ứng dụng gần hơn với quy trình vận hành thực tế của một cửa hàng nhỏ.

| STT | Tính năng bổ sung               | Nội dung đã thực hiện                                                                                                                                  |
| --- | ------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1   | POS bán hàng tại quầy           | Xây dựng màn hình POS riêng để chọn sản phẩm, lọc theo danh mục, tìm kiếm món, thêm/xóa/cập nhật số lượng trong giỏ hàng và tạo đơn hàng trực tiếp.    |
| 2   | Thanh toán                      | Hỗ trợ quy trình thanh toán với nhiều phương thức như tiền mặt, thẻ và QR; có tính promotion, loyalty points, tax, tổng tiền cần trả và tiền thối lại. |
| 3   | Quản lý nhân viên               | Bổ sung phân hệ quản lý nhân viên gồm thêm, sửa, vô hiệu hóa nhân viên, lọc theo vai trò/trạng thái, tìm kiếm, phân trang và sắp xếp nhiều tiêu chí.   |
| 4   | Quản lý lịch làm việc nhân viên | Xây dựng lịch làm việc theo tuần, hỗ trợ thêm/sửa/xóa ca làm, cập nhật trạng thái ca và cho phép nhân viên có nhiều ca khác nhau trong cùng một ngày.  |
| 5   | AI tạo mã khuyến mãi            | Tích hợp AI assistant để hỗ trợ tạo mã khuyến mãi từ mô tả tự nhiên, đồng thời tách riêng chế độ hỏi đáp thông thường và chế độ tạo promotion.         |

## 6. Điểm Tự Đánh Giá Cho Từng Thành Viên

| STT | Họ tên   | Mã số sinh viên         | Điểm tự đánh giá |
| --- | -------- | ----------------------- | ---------------- |
| 1   | 23127104 | Nguyễn Bình Minh Phương | 10.0             |
| 2   | 23127221 | Nguyễn Tiến Luật        | 10.0             |
| 3   | 23127228 | Phạm Văn Minh           | 10.0             |
| 4   | 23127281 | Đặng Nghi Văn           | 10.0             |
| 5   | 23127395 | Trần Anh Khoa           | 10.0             |

## 7. Video Demo

Link YouTube demo ở chế độ Unlisted: TODO - dán link YouTube vào đây

Lưu ý khi quay video:

- Không lồng tiếng.
- Không lồng nhạc.
- Chỉ sử dụng hình nền mặc định của hệ điều hành.
- Nếu cần giải thích thêm, ghi trong file này hoặc PowerPoint.
- Video nên dưới 5 phút nếu có thể.

## 8. Hướng Dẫn Chạy Ứng Dụng

- Mở solution `MyShop.sln` bằng Visual Studio hoặc IDE hỗ trợ .NET/Uno Platform.
- Đảm bảo PostgreSQL đã chạy.
- Lần đầu mở ứng dụng, nhập thông tin database trong màn hình cấu hình.
- Nếu cần dữ liệu mẫu, chạy các script trong thư mục `SeedData` theo thứ tự hoặc dùng script `run-all.sh`.
- Đăng nhập bằng tài khoản mẫu trong seed data để demo các chức năng.

