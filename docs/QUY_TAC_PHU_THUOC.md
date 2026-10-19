# Quy tắc xử lý dữ liệu phụ thuộc (Ẩn và Xóa hẳn)

Thầy yêu cầu phân biệt rõ **Ẩn** (đổi trạng thái, dữ liệu còn trong CSDL) và **Xóa hẳn** (Remove), đồng thời xử lý các dữ liệu phụ thuộc. Nhóm áp dụng các quy tắc sau.

## Sản phẩm
| Thao tác | Ai được làm | Quy tắc |
|---|---|---|
| Ẩn | Admin, Staff | Đổi `TrangThai` thành `Ngừng bán`. Khách không thấy và không thêm vào giỏ được. Đơn hàng, đánh giá cũ giữ nguyên. Admin/Staff vẫn xem được và có thể Hiện lại. |
| Hiện lại | Admin, Staff | Đổi `TrangThai` về `Đang bán`. |
| Xóa hẳn | Chỉ Admin | Chỉ khi sản phẩm **chưa có trong đơn hàng nào và chưa có đánh giá**. Khi đó hệ thống xóa luôn các biến thể và dọn các dòng giỏ hàng liên quan (trong một transaction). Nếu còn tham chiếu, hệ thống từ chối và gợi ý dùng Ẩn. |

## Biến thể (màu + size)
- Không tạo trùng (cùng sản phẩm + màu + size) và không trùng SKU.
- Không xóa biến thể đã nằm trong đơn hàng; muốn ngừng bán thì đặt tồn kho về 0.
- Xóa biến thể chưa có trong đơn thì tự dọn dòng giỏ hàng tương ứng.
- Sản phẩm luôn phải còn ít nhất một biến thể.

## Danh mục
| Thao tác | Quy tắc |
|---|---|
| Ẩn danh mục | Các sản phẩm thuộc danh mục không hiển thị ở trang khách. Trạng thái của từng sản phẩm không bị đổi. Hiện lại danh mục thì sản phẩm hiện lại. |
| Xóa danh mục | Chỉ khi danh mục **không còn sản phẩm nào** (kể cả sản phẩm đã ẩn). Nếu còn, hệ thống từ chối và hướng dẫn chuyển hoặc xóa sản phẩm trước. |

## Dấu vết thay đổi (audit)
| Cột | Khi nào ghi |
|---|---|
| `NgayTao`, `NguoiTao` | Khi thêm mới. Không bao giờ bị ghi đè. |
| `NgayCapNhat`, `NguoiCapNhat` | Khi sửa, ẩn, hiện. Người thực hiện lấy từ tài khoản đang đăng nhập. |

Áp dụng cho `SAN_PHAM` và `DANH_MUC` (script `Database/02_alter_audit.sql`).
