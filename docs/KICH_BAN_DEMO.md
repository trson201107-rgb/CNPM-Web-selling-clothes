# Kịch bản demo Sprint 2 (khoảng 10 phút, chạy thử trước ít nhất một lần)

**Chuẩn bị trước buổi demo**
- Mở sẵn bản deploy và bản local. Có sẵn dữ liệu mẫu trong DB (ít nhất 15 sản phẩm, 3 danh mục).
- Mở sẵn SSMS để đối chiếu dữ liệu.
- Có tài khoản Admin và một tài khoản khách.
- Có phương án dự phòng (video quay sẵn hoặc ảnh chụp) nếu mạng/host lỗi.

| Phút | Nội dung | Người trình bày | Điểm cần nhấn |
|---|---|---|---|
| 0–1 | Giới thiệu mục tiêu Sprint 2, phân vai | Leader | Đã làm gì so với yêu cầu thầy |
| 1–2 | Kiến trúc MVC và kết nối CSDL | BE1 | Cấu hình ngoài code, `/Health/Db` |
| 2–4 | Thêm sản phẩm + biến thể, sửa, xem chi tiết | BE1 + FE | Kiểm tra DB sau khi thêm; thử nhập sai để thấy báo lỗi |
| 4–5 | Ẩn / Hiện / Xóa hẳn | BE1 | SP ẩn không hiện ở trang khách; xóa SP đã có đơn bị từ chối |
| 5–7 | Phân trang, sắp xếp, tìm kiếm, lọc | BE2 | Kết hợp nhiều điều kiện; trang rỗng |
| 7–8 | Giao diện Admin và khách | UI/UX | Responsive, trạng thái lỗi/rỗng |
| 8–9 | Kết quả kiểm thử, bug còn tồn | Tester | Số liệu cụ thể |
| 9–10 | Kết luận, kế hoạch Sprint 3 | Leader | Việc còn lại |

**Câu hỏi thầy có thể hỏi (chuẩn bị trả lời)**
1. Khác nhau giữa Ẩn và Xóa hẳn? Khi nào dùng cái nào?
2. Xóa danh mục đang có sản phẩm thì sao?
3. Chống SQL Injection bằng cách nào? (tham số hóa)
4. Thêm sản phẩm và biến thể có dùng transaction không? Vì sao?
5. Chuỗi kết nối để ở đâu, deploy đổi cấu hình thế nào?
6. Phân trang làm ở tầng nào (SQL hay bộ nhớ)? Vì sao?
7. Ai làm gì, đóng góp của từng người thể hiện ở đâu? (lịch sử commit/PR)
