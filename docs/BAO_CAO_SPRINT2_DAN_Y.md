# Dàn ý báo cáo Sprint 2 (Leader hoàn thiện, đáp ứng tiêu chí L17)

## Chương I. Tổng quan Sprint 2
- Mục tiêu theo yêu cầu của thầy: kết nối CSDL, CRUD, MVC, deploy.
- Phạm vi chức năng làm được / chưa làm được (và lý do).
- Công nghệ: ASP.NET Core MVC (.NET 8), ADO.NET, SQL Server.

## Chương II. Tổ chức nhóm và kế hoạch
- Phân vai 5 người, nhiệm vụ cụ thể (lấy từ sheet Backlog).
- Quy trình Git, quy ước code (tham chiếu CONTRIBUTING.md).
- Tiến độ thực tế so với kế hoạch (số task Done / tổng, ảnh bảng Trello).

## Chương III. Thiết kế kiến trúc MVC
- Sơ đồ các lớp: Controller → (Service) → Repository → Database.
- Cây thư mục dự án.
- Cách kết nối CSDL (IDbConnectionFactory, cấu hình ngoài code, connection pooling).

## Chương IV. Cơ sở dữ liệu
- Các thay đổi so với Sprint 1 (cột trạng thái, 4 cột audit, script `02_alter_*.sql`).
- Quy tắc phụ thuộc: xóa danh mục / xóa sản phẩm / Ẩn vs Xóa hẳn.

## Chương V. Các chức năng đã thực hiện
Mỗi chức năng gồm: mô tả, ảnh giao diện, luồng xử lý, quy tắc nghiệp vụ.
- Thêm / Sửa / Xem chi tiết sản phẩm và biến thể
- Ẩn / Hiện / Xóa hẳn
- Quản lý danh mục
- Phân trang, sắp xếp, tìm kiếm, lọc
- Cảnh báo sắp hết hàng / hết hàng (Admin)
- Giỏ hàng (nếu thực hiện)

## Chương VI. Kiểm thử
- Kế hoạch và test case (từ Tester).
- Kết quả: số test đạt / không đạt, danh sách bug đã sửa và còn tồn.
- Kiểm thử bảo mật cơ bản: SQL Injection, dữ liệu không hợp lệ.

## Chương VII. Deploy
- Nơi host, địa chỉ truy cập, các bước deploy (tham chiếu docs/DEPLOY.md).
- Ảnh chụp bản chạy thật.

## Chương VIII. Đánh giá và hướng phát triển Sprint 3
- Bảng nghiệm thu (tổng hợp từ file Excel).
- Khó khăn, bài học.
- Việc còn lại và kế hoạch Sprint 3.

## Phụ lục
- Phân công và đóng góp từng thành viên (số commit, số PR, task hoàn thành).
- Script SQL, link GitHub, link bản deploy.
