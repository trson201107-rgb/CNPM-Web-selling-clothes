# Quy trình làm việc nhóm (Leader quy định)

## 1. Nhánh Git
| Nhánh | Mục đích | Ai được push |
|---|---|---|
| `main` | Bản ổn định, dùng để deploy và nộp thầy | Chỉ merge từ `develop` qua PR, do Leader duyệt |
| `develop` | Nhánh tích hợp hằng ngày | Chỉ merge qua PR |
| `feature/<ten-viec>` | Mỗi task một nhánh, ví dụ `feature/crud-san-pham` | Người làm task |
| `fix/<ten-loi>` | Sửa bug | Người sửa |

**Không ai push thẳng vào `main` hoặc `develop`.** Nên bật Branch protection trên GitHub
(Settings → Branches → Add rule → "Require a pull request before merging" + 1 approval).

## 2. Quy trình một task
1. Nhận task trên bảng backlog, chuyển sang **Doing**.
2. `git checkout develop && git pull` rồi `git checkout -b feature/<ten-viec>`.
3. Code, commit nhỏ và thường xuyên.
4. Trước khi tạo PR: `git pull origin develop` vào nhánh của mình, giải quyết conflict, chạy thử.
5. Tạo Pull Request vào `develop`, điền đủ mẫu PR, chuyển task sang **Review**.
6. Leader (hoặc người được chỉ định) review. Sửa theo góp ý. Duyệt xong thì merge, task sang **Done**.
7. Tester kiểm thử bản trên `develop`, báo bug vào sheet **Bug**.

## 3. Quy ước commit
Dạng: `<loai>(<pham-vi>): <mo ta ngan>`

- `feat(san-pham): them chuc nang them san pham`
- `fix(danh-muc): sua loi xoa danh muc con san pham`
- `docs(readme): cap nhat huong dan cai dat`
- `refactor`, `test`, `chore` dùng tương tự.

## 4. Quy ước code
- Tên class, method, property: `PascalCase`. Biến cục bộ, tham số: `camelCase`. Field private: `_camelCase`.
- Tên Entity khớp bảng: `SAN_PHAM` → `SanPham`, `DANH_MUC` → `DanhMuc`, `BIEN_THE` → `BienThe`.
- Mỗi lớp truy cập dữ liệu: `<Entity>Repository` (interface `I<Entity>Repository`).
- **Không viết SQL trong Controller/View.** SQL chỉ nằm trong Repository.
- **Mọi truy vấn dùng tham số** (`SqlParameter`), không nối chuỗi.
- Không hard-code chuỗi kết nối, mật khẩu, đường dẫn máy cá nhân.
- Mỗi file chỉ một class chính. Comment bằng tiếng Việt có dấu, giải thích "vì sao", không lặp lại code.

## 5. Definition of Done (một task chỉ Done khi đủ)
1. Chạy được trên máy mình và trên bản deploy.
2. Đã test: dữ liệu hợp lệ, sai/thiếu, rỗng, id không tồn tại.
3. Tester đã kiểm thử, không còn bug mức cao.
4. Có PR và đã được review.
5. Không lộ mật khẩu hay dữ liệu nhạy cảm.
6. Đã cập nhật README/tài liệu nếu đổi cách chạy hoặc cấu trúc DB.

## 6. Thay đổi cấu trúc CSDL
- Không sửa trực tiếp file `01_create_schema.sql` sau khi nhóm đã chạy.
- Mọi thay đổi viết thành script mới có đánh số trong `Database/`, ví dụ `02_alter_audit.sql`, rồi thông báo cả nhóm chạy.

## 7. Họp và báo cáo
- Daily 10 phút (hoặc nhắn trong nhóm): hôm qua làm gì, hôm nay làm gì, đang vướng gì. Ghi vào sheet **Daily**.
- Cuối mỗi giai đoạn: Leader cập nhật trạng thái backlog và báo cáo tiến độ.
