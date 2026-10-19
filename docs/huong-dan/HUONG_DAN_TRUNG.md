# Hướng dẫn cho Trung (Front-end)

Gói này phải merge **sau cùng** (các màn hình dùng dữ liệu và controller của Sơn và Thái).

## Gói này gồm gì
| Nhóm | Tệp |
|---|---|
| Khung trang | `Views/Shared/_Layout.cshtml` (menu, tìm kiếm, giỏ hàng, đăng nhập), `_AdminLayout.cshtml` (menu quản trị), `_Alerts.cshtml` (thông báo), `_ProductCard.cshtml`, `NotFound.cshtml`, `Components/CartBadge/Default.cshtml` |
| Trang khách | `Views/Home/Index.cshtml`, `Views/Shop/Index.cshtml` (bộ lọc, lưới sản phẩm, phân trang), `Views/SanPham/Details.cshtml` (chọn màu, size), `Views/Cart/Index.cshtml`, `Views/Account/*` |
| Trang quản trị | `Views/Admin/Index.cshtml`, `Views/SanPhamAdmin/*` (danh sách, form thêm/sửa có bảng biến thể), `Views/DanhMucAdmin/*`, `Views/TonKhoAdmin/*` |
| Giao diện | `wwwroot/css/site.css`, `wwwroot/js/site.js` |
| Hỗ trợ | `Helpers/Money.cs` (định dạng tiền Việt, kiểm tra mã màu), `Views/_ViewImports.cshtml` |

Các thẻ Trello: #3, #4, #5, #6, #8, #9, #15 (phần giao diện).

## Chạy thử
Dùng bản gộp `0_TatCa_ChayThuNgay.zip` hoặc merge đủ ba gói rồi `dotnet run`, mở `http://localhost:5001`.

## Kiểm tra giao diện (đối chiếu thẻ Trello)
| Trang | Cần kiểm tra |
|---|---|
| Trang chủ | Có phần Hot và Mới; bấm chip danh mục sang `/Shop` đúng danh mục |
| `/Shop` | Bộ lọc giữ giá trị sau khi bấm Áp dụng; có thông báo khi không có kết quả; thanh phân trang giữ nguyên điều kiện lọc |
| Chi tiết sản phẩm | Chọn màu thì size không có màu đó bị làm mờ; hiển thị tồn kho; nút "Thêm vào giỏ" chỉ bật khi chọn đủ màu và size; chưa đăng nhập thì hiện nút "Đăng nhập để mua hàng" |
| Giỏ hàng | Trạng thái giỏ trống; cập nhật số lượng; xóa có hộp thoại xác nhận |
| Đăng nhập, đăng ký | Thông báo lỗi hiện rõ; không đăng ký trùng email |
| Quản trị sản phẩm | Thêm, gỡ dòng biến thể; đánh dấu xóa biến thể; nút Ẩn, Hiện, Xóa hẳn có hộp thoại xác nhận |
| Điện thoại | Thu nhỏ trình duyệt, menu thu gọn, lưới sản phẩm còn 2 cột |

## Khái niệm cần nắm để trả lời thầy
- **Razor view và tag helper**: `asp-controller`, `asp-action`, `asp-for` tự sinh đường dẫn và tên ô nhập đúng với controller; form POST tự có token chống CSRF.
- **Layout lồng nhau**: `_AdminLayout` dùng lại `_Layout`, chỉ thêm thanh menu quản trị.
- **Partial view**: `_ProductCard` dùng lại ở trang chủ, cửa hàng, sản phẩm liên quan; `_Form` dùng chung cho thêm và sửa.
- **Bảng biến thể động**: `site.js` thêm dòng từ thẻ `<template>`, gỡ dòng, và đánh lại chỉ số `BienThes[0]`, `BienThes[1]`... trước khi gửi để máy chủ nhận đủ danh sách.
- **Chọn màu và size**: `site.js` đọc danh sách biến thể (JSON trong thuộc tính `data-variants`), tìm biến thể khớp màu và size để điền mã `maBT` gửi lên giỏ hàng.
- **Hiển thị an toàn**: Razor tự mã hóa HTML; mã màu từ CSDL qua `Css.Hex` chỉ nhận dạng `#RRGGBB`.

## Tùy chỉnh nhanh
Đổi màu chủ đạo ở đầu `wwwroot/css/site.css` (biến `--brand`, `--brand-2`, `--accent`).

## Đưa lên GitHub (làm đúng quy trình nhóm)
Thứ tự merge của cả nhóm: **Sơn, rồi Thái, rồi Trung** (gói sau dùng code của gói trước).

```powershell
cd D:\CNPM_N04_Dailyman
git checkout develop
git pull
git checkout -b feature/trung-giao-dien
```
Giải nén file zip của bạn, chép **toàn bộ nội dung bên trong** vào thư mục repo, chọn Replace nếu Windows hỏi ghi đè (đừng chép cả thư mục bọc ngoài). Sau đó:

```powershell
git add .
git status
```
Trong `git status` phải **không có** `appsettings.Development.json`, `bin/`, `obj/`. Rồi:

```powershell
git commit -m "feat(fe): giao dien khach va quan tri, bo loc, gio hang, bang bien the"
git push -u origin feature/trung-giao-dien
```
Vào GitHub, bấm **Compare & pull request**, đổi nhánh đích thành **develop**, điền mẫu PR, nhờ Sơn review rồi merge. Sau khi merge, kéo thẻ Trello tương ứng sang **Done** (Sơn sẽ chuyển sang Finish khi Tân kiểm thử đạt).
