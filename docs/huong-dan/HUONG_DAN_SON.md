# Hướng dẫn cho Sơn (Nhóm trưởng và Back-end 1)

Gói này là **nền tảng** của cả nhóm, phải merge đầu tiên.

## Gói này gồm gì
| Nhóm | Tệp | Thẻ Trello |
|---|---|---|
| Cơ sở dữ liệu | `Database/02_alter_audit.sql` (cột audit, trạng thái danh mục, cờ Hot, chỉ mục), `03_seed_demo_data.sql` (dữ liệu mẫu), `00_reset_database.sql` | #2, #4 |
| Entity, ViewModel | `Models/SanPham.cs`, `Models/SanPhamViewModels.cs` | #2 |
| Truy cập dữ liệu | `Repositories/ISanPhamRepository.cs`, `SanPhamRepository.cs`, `Data/DbReaderExtensions.cs`, `Data/SqlText.cs` | #2, #4 |
| Điều khiển | `Controllers/SanPhamAdminController.cs`, `SanPhamController.cs` | #4, #6 |
| Dùng chung | `Program.cs` (đăng nhập bằng cookie, tự đăng ký Repository/Service, chống CSRF, hỗ trợ cổng PORT), `Services/ICurrentUser.cs`, `Services/NghiepVuException.cs`, `Middleware/DevBypassAuthMiddleware.cs` | #1 |
| Deploy | `Dockerfile`, `.dockerignore`, `.github/workflows/ci.yml`, `docs/DEPLOY.md` | #11, #12 |
| Tài liệu | `README.md`, `docs/QUY_TAC_PHU_THUOC.md`, hướng dẫn này | #13 |

## Cài đặt và chạy thử
1. **Cơ sở dữ liệu.** Database của bạn đã chạy `01_create_schema.sql` từ trước. Trong SSMS mở và Execute lần lượt `Database/02_alter_audit.sql` rồi `Database/03_seed_demo_data.sql`. Kết quả cuối của file 03 phải cho ít nhất 16 sản phẩm.
2. **Cấu hình máy bạn.** Mở `src/ThoiTrangNam.Web/appsettings.Development.json` (file của bạn đã có chuỗi kết nối với `SQLEXPRESS05`), giữ nguyên chuỗi kết nối, không cần thêm gì khác.
3. **Chạy.** Gói của Sơn chỉ có phần xử lý, giao diện nằm ở gói của Trung. Hãy dùng **bản gộp** `0_TatCa_ChayThuNgay.zip` để chạy thử toàn bộ, hoặc merge cả ba gói theo thứ tự rồi chạy:
   ```
   cd D:\CNPM_N04_Dailyman\src\ThoiTrangNam.Web
   dotnet run
   ```
4. Đăng nhập `duc.pham@shop.vn` / `Demo@123` rồi vào `/Admin`.

## Kiểm tra theo tiêu chí nghiệm thu (B3, B4, B5)
| Tiêu chí | Cách thử | Kết quả mong đợi |
|---|---|---|
| Thêm sản phẩm + biến thể | `/Admin/SanPham/Create`, điền đủ thông tin | Sản phẩm xuất hiện trong danh sách và trong bảng `SAN_PHAM`, `BIEN_THE` |
| Từ chối dữ liệu sai | Để trống tên, giá âm, không chọn danh mục | Thông báo lỗi tiếng Việt, không lưu |
| **Transaction** | Thêm sản phẩm có 2 biến thể cùng SKU với một sản phẩm đã có | Báo lỗi SKU trùng; chạy `SELECT * FROM SAN_PHAM WHERE TenSP = N'tên bạn nhập'` thì **không còn dòng nào** (đã rollback) |
| Sửa | Đổi giá và tồn kho | Cập nhật đúng |
| Ẩn / Hiện | Bấm biểu tượng mắt gạch | Trạng thái `Ngừng bán`, trang cửa hàng không còn thấy, Admin vẫn thấy |
| Xóa hẳn có tham chiếu | Xóa hẳn sản phẩm số 1 (đã có đơn hàng mẫu) | Bị từ chối, gợi ý dùng Ẩn |
| Xóa hẳn không tham chiếu | Tạo sản phẩm mới rồi xóa hẳn | Xóa được, cả biến thể cũng mất |
| Chống trùng | Tạo hai biến thể cùng màu và size | Bị từ chối |
| Audit khi thêm | `SELECT NgayTao, NguoiTao FROM SAN_PHAM WHERE TenSP = ...` | Có ngày và mã người tạo |
| Audit khi sửa | Sửa sản phẩm rồi xem lại | `NgayCapNhat`, `NguoiCapNhat` có giá trị, `NgayTao`, `NguoiTao` không đổi |
| SQL Injection | Nhập `' OR 1=1 --` vào ô tìm kiếm ở `/Admin/SanPham` | Không lộ thêm dữ liệu, không lỗi hệ thống |

## Khái niệm cần nắm để trả lời thầy
- **Transaction**: `SanPhamRepository.ThemAsync` mở một giao dịch, thêm sản phẩm rồi từng biến thể, lỗi ở bất kỳ bước nào thì `Rollback`, thành công mới `Commit`. Nhờ vậy không bao giờ có sản phẩm "mồ côi" không biến thể.
- **Truy vấn tham số hóa**: mọi giá trị người dùng đi qua `SqlCommand.Parameters.AddWithValue`, không nối vào chuỗi SQL, nên không bị SQL Injection.
- **Ẩn và Xóa hẳn**: Ẩn chỉ đổi `TrangThai`, dữ liệu và đơn hàng cũ còn nguyên; Xóa hẳn chỉ khi không còn đơn hàng hoặc đánh giá tham chiếu (xem `docs/QUY_TAC_PHU_THUOC.md`).
- **Audit**: `NgayTao`, `NguoiTao` ghi khi thêm; `NgayCapNhat`, `NguoiCapNhat` ghi khi sửa; người thực hiện lấy từ `ICurrentUser` (đọc từ cookie đăng nhập).
- **Tự đăng ký lớp**: `Program.cs` quét các lớp `XxxRepository` và `XxxService`, nên thêm lớp mới không cần sửa `Program.cs`.
- **Lớp kết nối dùng chung**: `IDbConnectionFactory` (đã làm ở B1), chuỗi kết nối nằm ngoài code.

## Việc của Leader sau khi merge cả nhóm
1. Tạo Branch protection cho `main` và `develop`.
2. Điền kết quả thực tế (link PR, ảnh chụp) vào các thẻ Trello, kéo sang Finish khi Tân xác nhận.
3. Chốt nơi deploy, thử deploy theo `docs/DEPLOY.md`, ghi nhật ký deploy.
4. Chụp bảng Trello và lịch sử Git đưa vào báo cáo.

## Đưa lên GitHub (làm đúng quy trình nhóm)
Thứ tự merge của cả nhóm: **Sơn, rồi Thái, rồi Trung** (gói sau dùng code của gói trước).

```powershell
cd D:\CNPM_N04_Dailyman
git checkout develop
git pull
git checkout -b feature/son-nen-tang-san-pham
```
Giải nén file zip của bạn, chép **toàn bộ nội dung bên trong** vào thư mục repo, chọn Replace nếu Windows hỏi ghi đè (đừng chép cả thư mục bọc ngoài). Sau đó:

```powershell
git add .
git status
```
Trong `git status` phải **không có** `appsettings.Development.json`, `bin/`, `obj/`. Rồi:

```powershell
git commit -m "feat(be1): nen tang MVC, CRUD san pham va bien the, audit, deploy"
git push -u origin feature/son-nen-tang-san-pham
```
Vào GitHub, bấm **Compare & pull request**, đổi nhánh đích thành **develop**, điền mẫu PR, nhờ Sơn review rồi merge. Sau khi merge, kéo thẻ Trello tương ứng sang **Done** (Sơn sẽ chuyển sang Finish khi Tân kiểm thử đạt).
