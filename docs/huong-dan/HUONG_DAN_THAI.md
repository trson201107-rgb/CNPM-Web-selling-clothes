# Hướng dẫn cho Thái (Back-end 2)

Gói này phải merge **sau** gói của Sơn (dùng `Data/DbReaderExtensions.cs`, `ProductListItem`, `ICurrentUser` của Sơn).

## Gói này gồm gì
| Nhóm | Tệp | Thẻ Trello |
|---|---|---|
| Danh mục | `Models/DanhMuc.cs`, `Repositories/IDanhMucRepository.cs`, `DanhMucRepository.cs`, `Controllers/DanhMucAdminController.cs` | #2, #3 |
| Tìm kiếm, lọc, sắp xếp, phân trang | `Models/SearchModels.cs`, `Repositories/ISanPhamQueryRepository.cs`, `SanPhamQueryRepository.cs`, `Controllers/ShopController.cs`, `HomeController.cs` | #5 |
| Cảnh báo tồn kho | `Controllers/TonKhoAdminController.cs`, `AdminController.cs` | #7 |
| Đăng nhập, phân quyền | `Models/AccountModels.cs`, `Repositories/TaiKhoanRepository.cs`, `Services/AuthService.cs`, `Services/DemoAccountSeeder.cs`, `Controllers/AccountController.cs` | #8 |
| Giỏ hàng | `Models/CartModels.cs`, `Repositories/CartRepository.cs`, `Controllers/CartController.cs`, `ViewComponents/CartBadgeViewComponent.cs` | #15 |

## Chạy thử
Dùng bản gộp `0_TatCa_ChayThuNgay.zip` hoặc merge đủ ba gói rồi `dotnet run`. Lần đầu khởi động, ứng dụng tự đặt mật khẩu `Demo@123` cho các tài khoản mẫu (xem log: "Đã đặt mật khẩu mẫu cho ... tài khoản demo").

## Kiểm tra theo tiêu chí
| Chức năng | Cách thử | Kết quả mong đợi |
|---|---|---|
| Tìm theo tên | `/Shop?q=polo` | Chỉ sản phẩm có chữ "polo" |
| Lọc | Chọn danh mục, khoảng giá, loại Hot hoặc Mới, chỉ còn hàng | Kết quả đúng điều kiện |
| Sắp xếp | Chọn giá tăng, giá giảm, tên, danh mục | Thứ tự đúng |
| Phân trang | Danh sách có hơn 12 sản phẩm (sau script 03 là 16) | Có thanh trang, trang 2 có sản phẩm còn lại |
| Danh sách trống | `/Shop?q=khongcosanpham` | Thông báo "Không tìm thấy sản phẩm phù hợp" |
| Kết hợp điều kiện | Lọc + sắp xếp + sang trang 2 | Điều kiện được giữ khi chuyển trang |
| SQL Injection | `/Shop?q=' OR 1=1 --` | Không lỗi, không lộ dữ liệu |
| Cảnh báo tồn kho | `/Admin/TonKho` | Có danh sách hết hàng và sắp hết hàng (script 03 đã tạo sẵn) |
| Đăng nhập đúng/sai | `/Account/Login` | Đúng thì vào được; sai báo "Email hoặc mật khẩu không đúng" |
| Khóa tài khoản | Nhập sai mật khẩu 5 lần | Khóa 15 phút, báo số phút còn lại |
| Phân quyền | Chưa đăng nhập vào `/Admin` | Chuyển về trang đăng nhập; đăng nhập bằng tài khoản khách thì báo không có quyền |
| Danh mục: xóa khi còn sản phẩm | `/Admin/DanhMuc`, xóa danh mục đã có sản phẩm | Bị từ chối, giải thích rõ |
| Danh mục: ẩn | Ẩn một danh mục | Sản phẩm thuộc danh mục biến mất khỏi `/Shop`, hiện lại thì trở lại |
| Giỏ hàng | Thêm, sửa số lượng, xóa; thử số lượng lớn hơn tồn kho | Tổng tiền đúng; vượt tồn kho bị chặn |

## Khái niệm cần nắm để trả lời thầy
- **Phân trang ngay trong SQL**: dùng `OFFSET ... FETCH NEXT ...` để chỉ lấy đúng một trang, kèm một câu `COUNT(*)` để biết tổng số trang (xem `SanPhamQueryRepository.TimKiemAsync`).
- **Sắp xếp an toàn**: kiểu sắp xếp người dùng chọn được ánh xạ qua danh sách trắng (`SapXep`), không bao giờ nối chuỗi người dùng vào `ORDER BY`.
- **Lọc động nhưng vẫn tham số hóa**: điều kiện `WHERE` được ghép từng phần, nhưng mọi giá trị đều là tham số.
- **Mật khẩu**: băm bằng `PasswordHasher` (PBKDF2, có muối), không lưu mật khẩu thô. Đăng nhập sai 5 lần khóa 15 phút.
- **Cookie đăng nhập và vai trò**: sau khi đăng nhập, hệ thống ghi cookie có các claim `MaTK`, tên, vai trò; thuộc tính `[Authorize(Roles = "Admin,Staff")]` chặn người không đủ quyền.
- **Quy tắc danh mục**: ẩn danh mục thì sản phẩm trong đó không hiện cho khách; chỉ xóa được danh mục không còn sản phẩm nào.
- **Giỏ hàng trong CSDL**: mỗi tài khoản một giỏ, thêm hàng chạy trong transaction và kiểm tra tồn kho trước.

## Đưa lên GitHub (làm đúng quy trình nhóm)
Thứ tự merge của cả nhóm: **Sơn, rồi Thái, rồi Trung** (gói sau dùng code của gói trước).

```powershell
cd D:\CNPM_N04_Dailyman
git checkout develop
git pull
git checkout -b feature/thai-danhmuc-timkiem-giohang
```
Giải nén file zip của bạn, chép **toàn bộ nội dung bên trong** vào thư mục repo, chọn Replace nếu Windows hỏi ghi đè (đừng chép cả thư mục bọc ngoài). Sau đó:

```powershell
git add .
git status
```
Trong `git status` phải **không có** `appsettings.Development.json`, `bin/`, `obj/`. Rồi:

```powershell
git commit -m "feat(be2): danh muc, tim kiem loc phan trang, canh bao ton kho, dang nhap, gio hang"
git push -u origin feature/thai-danhmuc-timkiem-giohang
```
Vào GitHub, bấm **Compare & pull request**, đổi nhánh đích thành **develop**, điền mẫu PR, nhờ Sơn review rồi merge. Sau khi merge, kéo thẻ Trello tương ứng sang **Done** (Sơn sẽ chuyển sang Finish khi Tân kiểm thử đạt).
