-- =====================================================================
-- 03_seed_demo_data.sql  (Sprint 2)
-- Thêm dữ liệu mẫu để demo: tổng cộng >= 15 sản phẩm, 5 danh mục, có sản phẩm
-- Hot / mới / cũ, có biến thể sắp hết hàng (tồn <= 5) và hết hàng (tồn = 0).
-- Chạy được nhiều lần. Chạy SAU 02_alter_audit.sql.
-- =====================================================================
USE ThoiTrangNam;
GO

-- 4 sản phẩm mẫu có sẵn: đánh dấu Hot + thêm ảnh minh họa
UPDATE SAN_PHAM SET LaNoiBat = 1 WHERE MaSP IN (1, 3);
UPDATE SAN_PHAM SET AnhDaiDien = 'https://picsum.photos/seed/sp-' + CAST(MaSP AS VARCHAR(10)) + '/480/600' WHERE AnhDaiDien IS NULL;
GO

IF OBJECT_ID('tempdb..#sp') IS NOT NULL DROP TABLE #sp;
CREATE TABLE #sp (Slug VARCHAR(30), TenSP NVARCHAR(150), MoTa NVARCHAR(1000), Gia DECIMAL(12,0), MaDM INT, Hot BIT, NgayTruoc INT);
INSERT INTO #sp VALUES
('ao-thun-polo',     N'Áo thun polo pique',          N'Vải cá sấu thoáng mát, cổ bẻ lịch sự',      259000, 1, 0, 5),
('ao-thun-oversize', N'Áo thun oversize local brand', N'Form rộng, in họa tiết nhỏ',                229000, 1, 1, 12),
('ao-thun-tay-dai',  N'Áo thun tay dài basic',        N'Cotton dày vừa, mặc trong hoặc ngoài',      219000, 1, 0, 70),
('so-mi-linen',      N'Áo sơ mi linen ngắn tay',      N'Linen mềm, thoáng, hợp mùa hè',             389000, 2, 0, 20),
('so-mi-flannel',    N'Áo sơ mi caro flannel',        N'Vải flannel ấm, họa tiết caro',             329000, 2, 1, 65),
('jeans-straight',   N'Quần jeans straight',          N'Ống đứng, denim dày dặn',                   489000, 3, 0, 8),
('jeans-rach-goi',   N'Quần jeans rách gối',          N'Phong cách đường phố, co giãn nhẹ',         529000, 3, 0, 90),
('kaki-slim',        N'Quần kaki slim',               N'Kaki co giãn, ôm vừa',                      419000, 4, 1, 3),
('short-kaki',       N'Quần short kaki',              N'Dài trên gối, túi hộp',                     289000, 4, 0, 25),
('that-lung-da',     N'Thắt lưng da bò',              N'Da bò thật, khóa kim loại',                 249000, 5, 0, 40),
('non-luoi-trai',    N'Nón lưỡi trai',                N'Cotton, khóa điều chỉnh',                   159000, 5, 0, 15),
('vi-da-nam',        N'Ví da nam',                    N'Ví gập, nhiều ngăn thẻ',                    299000, 5, 1, 2);

INSERT INTO SAN_PHAM (TenSP, MoTa, GiaBan, AnhDaiDien, LaNoiBat, MaDM, NgayTao)
SELECT s.TenSP, s.MoTa, s.Gia, 'https://picsum.photos/seed/' + s.Slug + '/480/600', s.Hot, s.MaDM,
       DATEADD(DAY, -s.NgayTruoc, SYSDATETIME())
FROM #sp s
WHERE NOT EXISTS (SELECT 1 FROM SAN_PHAM x WHERE x.TenSP = s.TenSP);

IF OBJECT_ID('tempdb..#bt') IS NOT NULL DROP TABLE #bt;
CREATE TABLE #bt (SKU VARCHAR(30), Ton INT, Slug VARCHAR(30), TenMau NVARCHAR(30), TenSize VARCHAR(10));
INSERT INTO #bt VALUES
('AT-POLO-TRG-M',  20, 'ao-thun-polo',     N'Trắng',     'M'),
('AT-POLO-DEN-L',   3, 'ao-thun-polo',     N'Đen',       'L'),
('AT-OVS-DEN-L',   14, 'ao-thun-oversize', N'Đen',       'L'),
('AT-OVS-TRG-XL',   0, 'ao-thun-oversize', N'Trắng',     'XL'),
('AT-TD-NAV-M',     9, 'ao-thun-tay-dai',  N'Xanh navy', 'M'),
('AT-TD-DEN-L',    11, 'ao-thun-tay-dai',  N'Đen',       'L'),
('SM-LINEN-TRG-M', 16, 'so-mi-linen',      N'Trắng',     'M'),
('SM-LINEN-TRG-L',  5, 'so-mi-linen',      N'Trắng',     'L'),
('SM-FLAN-NAV-L',   8, 'so-mi-flannel',    N'Xanh navy', 'L'),
('SM-FLAN-DEN-XL',  2, 'so-mi-flannel',    N'Đen',       'XL'),
('QJ-STR-NAV-L',   12, 'jeans-straight',   N'Xanh navy', 'L'),
('QJ-STR-DEN-M',    7, 'jeans-straight',   N'Đen',       'M'),
('QJ-RACH-DEN-L',   6, 'jeans-rach-goi',   N'Đen',       'L'),
('QJ-RACH-NAV-XL',  0, 'jeans-rach-goi',   N'Xanh navy', 'XL'),
('QK-SLIM-DEN-M',  18, 'kaki-slim',        N'Đen',       'M'),
('QK-SLIM-NAV-L',  10, 'kaki-slim',        N'Xanh navy', 'L'),
('QK-SHORT-DEN-M', 22, 'short-kaki',       N'Đen',       'M'),
('QK-SHORT-NAV-L', 13, 'short-kaki',       N'Xanh navy', 'L'),
('TL-DA-DEN-M',    30, 'that-lung-da',     N'Đen',       'M'),
('TL-DA-DEN-L',     4, 'that-lung-da',     N'Đen',       'L'),
('NON-LT-DEN-M',   40, 'non-luoi-trai',    N'Đen',       'M'),
('NON-LT-NAV-M',   25, 'non-luoi-trai',    N'Xanh navy', 'M'),
('VI-DA-DEN-M',    15, 'vi-da-nam',        N'Đen',       'M');

INSERT INTO BIEN_THE (SKU, SoLuongTon, MaSP, MaMau, MaSize)
SELECT b.SKU, b.Ton, sp.MaSP, ms.MaMau, kc.MaSize
FROM #bt b
JOIN #sp s        ON s.Slug = b.Slug
JOIN SAN_PHAM sp  ON sp.TenSP = s.TenSP
JOIN MAU_SAC ms   ON ms.TenMau = b.TenMau
JOIN KICH_CO kc   ON kc.TenSize = b.TenSize
WHERE NOT EXISTS (SELECT 1 FROM BIEN_THE x WHERE x.SKU = b.SKU);

DROP TABLE #bt;
DROP TABLE #sp;
GO

SELECT (SELECT COUNT(*) FROM SAN_PHAM) AS SoSanPham,
       (SELECT COUNT(*) FROM BIEN_THE) AS SoBienThe,
       (SELECT COUNT(*) FROM DANH_MUC) AS SoDanhMuc;
GO
