# Hướng dẫn deploy

Hệ thống gồm **ứng dụng ASP.NET Core** và **cơ sở dữ liệu SQL Server**. Cần chuẩn bị cả hai nơi chạy.

## 1. Cấu hình bằng biến môi trường (không ghi mật khẩu vào mã nguồn)

| Biến | Ý nghĩa | Ví dụ |
|---|---|---|
| `ConnectionStrings__ThoiTrangNam` | Chuỗi kết nối SQL Server | `Server=tenmaychu,1433;Database=ThoiTrangNam;User Id=app_user;Password=...;TrustServerCertificate=True;` |
| `ASPNETCORE_ENVIRONMENT` | Môi trường | `Production` |
| `Auth__DevBypass` | Bỏ qua đăng nhập (chỉ để phát triển) | luôn `false` khi deploy |
| `Seed__ResetDemoPasswords` | Tự đặt mật khẩu cho tài khoản mẫu | `false` khi chạy thật (xem mục 6) |
| `PORT` | Cổng (một số host tự cấp) | do host cấp |

## 2. Chuẩn bị CSDL trên máy chủ SQL
1. Tạo database và chạy lần lượt trong thư mục `Database/`: `01_create_schema.sql`, `02_alter_audit.sql`, rồi `03_seed_demo_data.sql` (dữ liệu mẫu, có thể bỏ qua khi chạy thật).
2. Tạo **tài khoản SQL riêng** cho ứng dụng (không dùng `sa`), chỉ cấp quyền đọc/ghi trên database `ThoiTrangNam`.
3. Mở firewall/cổng 1433 cho địa chỉ của nơi chạy ứng dụng. Nếu dùng SQL Server Express trên máy cá nhân, phải bật giao thức TCP/IP trong SQL Server Configuration Manager.

## 3. Cách A: Docker (khuyên dùng)
Kiểm tra trên máy trước:
```
docker build -t thoitrangnam .
docker run --rm -p 8080:8080 -e "ConnectionStrings__ThoiTrangNam=Server=host.docker.internal,1433;Database=ThoiTrangNam;User Id=app_user;Password=MAT_KHAU;TrustServerCertificate=True;" thoitrangnam
```
Mở `http://localhost:8080/healthz` (phải trả về `ok`) rồi `http://localhost:8080/Health/Db`.

Đưa lên host: chọn dịch vụ cho chạy container từ repo GitHub (ví dụ Render, Railway, Azure App Service dạng Container, hoặc máy chủ ảo tự cài Docker). Trỏ tới nhánh `main`, đặt các biến ở mục 1. Điều kiện miễn phí và giới hạn của từng dịch vụ thay đổi theo thời gian, nhóm cần kiểm tra trang giá hiện tại của dịch vụ đã chọn.

## 4. Cách B: không dùng Docker
```
dotnet publish src/ThoiTrangNam.Web/ThoiTrangNam.Web.csproj -c Release -o ./publish
```
Tải thư mục `publish` lên host (IIS, Azure App Service...), cài runtime .NET 10 trên máy chủ nếu host chưa có, đặt biến môi trường ở mục 1.

## 5. Kiểm tra sau khi deploy
1. `/healthz` trả về `ok`.
2. `/Health/Db` báo kết nối thành công, 19 bảng.
3. Trang chủ hiển thị sản phẩm.
4. Đăng nhập Admin, thêm một sản phẩm, kiểm tra bản ghi trong CSDL (đầy đủ ngày/người tạo).
5. Chụp ảnh các bước trên làm bằng chứng cho báo cáo.

## 6. Bảo mật trước khi mở cho người ngoài
- Tài khoản mẫu (script có sẵn) được ứng dụng tự đặt mật khẩu `Demo@123` khi khởi động nếu `Seed__ResetDemoPasswords` là `true`. Khi mở công khai: đặt biến này `false` và đổi hoặc xóa các tài khoản mẫu.
- `Auth__DevBypass` phải là `false`.
- Không đưa chuỗi kết nối, mật khẩu lên GitHub.

## 7. Lỗi thường gặp
| Triệu chứng | Nguyên nhân thường gặp |
|---|---|
| Trang báo "Không thể kết nối cơ sở dữ liệu" | Sai chuỗi kết nối, firewall chưa mở, sai user/password, SQL Server chưa bật TCP/IP |
| Lỗi chứng chỉ SSL khi kết nối | Thêm `TrustServerCertificate=True` (chỉ khi CSDL nội bộ) |
| Lỗi 500 sau khi deploy | Xem log của host; thường thiếu runtime .NET 10 hoặc thiếu biến môi trường |
| Đăng nhập xong bị đưa về trang đăng nhập | Host chạy sau proxy HTTPS: đặt `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` (đã có trong Dockerfile) |
| Chạy local được, lên host không | Khác biến môi trường, khác phiên bản SQL Server, chưa chạy script `02` và `03` |

## 8. Nhật ký deploy
| Ngày giờ | Người deploy | Commit | Ghi chú |
|---|---|---|---|
| | | | |
