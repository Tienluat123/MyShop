Hướng dẫn thiết lập dự án cho team

Mục đích
- Hướng dẫn nhanh để các thành viên trong team có thể cài đặt, build và chạy dự án MyShop trên máy của họ.

Yêu cầu trước
- .NET SDK 10 (hoặc phiên bản tương thích với `net10.0`)
- Git
- Visual Studio Code (hoặc Visual Studio)
- Khuyến nghị: `uno-check` nếu dùng Uno Platform, extension C# cho VS Code

Clone repo
```bash
git clone <REPO_URL>
cd MyShop
```

Khôi phục và build
- Khôi phục packages:
```bash
dotnet restore MyShop/MyShop.csproj
```

- Build cho WebAssembly (WASM):
```bash
dotnet build MyShop/MyShop.csproj -f net10.0-browserwasm
```

- Publish WASM (tạo artifact để deploy):
```bash
dotnet publish MyShop/MyShop.csproj -f net10.0-browserwasm
```

- Build cho Desktop:
```bash
dotnet build MyShop/MyShop.csproj -f net10.0-desktop
```

Tasks sẵn có (VS Code)
- Workspace đã có tasks để build/publish cho WASM và Desktop. Mở `Run Task...` trong VS Code và chọn `build-wasm`, `publish-wasm`, `build-desktop`, `publish-desktop`.

Chạy lần đầu và gỡ lỗi
- Nếu gặp lỗi thiếu SDK hoặc packages, chạy `dotnet --info` và `dotnet restore`.
- Kiểm tra `Platforms/` tương ứng nếu cần cấu hình platform-specific.

Quy ước commit & branch (tạm)
- Mở một branch riêng cho feature: `git checkout -b feature/<tên-feature>`
- Commit với message ngắn gọn theo dạng: `feat: thêm mô-đun sản phẩm` hoặc `fix: sửa lỗi build`.

Ghi chú cho team lead
- Nếu muốn, tạo branch `setup/team` để chứa các tài liệu thiết lập và cấu hình chung.

Ghi chú khác
- File chính: `MyShop/MyShop.csproj` và `MyShop.sln`.
- Nếu cần mình có thể tạo sẵn một branch hoặc tạo file `CONTRIBUTING.md` chi tiết hơn theo quy trình team.

Nếu muốn mình commit file này vào repo hoặc tạo branch `setup/team`, nói mình biết nhé.