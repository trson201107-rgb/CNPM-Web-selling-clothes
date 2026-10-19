-- =====================================================================
-- 02_alter_audit.sql  (Sprint 2)
-- Bổ sung theo yêu cầu của giảng viên:
--   * Ngày tạo / ngày cập nhật / người tạo / người cập nhật (SAN_PHAM, DANH_MUC)
--   * Cờ sản phẩm nổi bật (Hot) để lọc "Hot"
--   * Trạng thái hiển thị của danh mục (1 = hiển thị, 0 = ẩn)
-- Chạy được nhiều lần (không báo lỗi nếu cột đã có).
-- Chạy SAU 01_create_schema.sql, TRƯỚC 03_seed_demo_data.sql.
-- =====================================================================
USE ThoiTrangNam;
GO

-- ---------- SAN_PHAM ----------
IF COL_LENGTH('SAN_PHAM', 'NgayCapNhat') IS NULL ALTER TABLE SAN_PHAM ADD NgayCapNhat DATETIME2 NULL;
IF COL_LENGTH('SAN_PHAM', 'NguoiTao') IS NULL ALTER TABLE SAN_PHAM ADD NguoiTao INT NULL;
IF COL_LENGTH('SAN_PHAM', 'NguoiCapNhat') IS NULL ALTER TABLE SAN_PHAM ADD NguoiCapNhat INT NULL;
IF COL_LENGTH('SAN_PHAM', 'LaNoiBat') IS NULL ALTER TABLE SAN_PHAM ADD LaNoiBat BIT NOT NULL CONSTRAINT DF_SP_LaNoiBat DEFAULT 0;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_SP_NguoiTao')
    ALTER TABLE SAN_PHAM ADD CONSTRAINT FK_SP_NguoiTao FOREIGN KEY (NguoiTao) REFERENCES TAI_KHOAN(MaTK);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_SP_NguoiCapNhat')
    ALTER TABLE SAN_PHAM ADD CONSTRAINT FK_SP_NguoiCapNhat FOREIGN KEY (NguoiCapNhat) REFERENCES TAI_KHOAN(MaTK);
GO

-- ---------- DANH_MUC ----------
IF COL_LENGTH('DANH_MUC', 'TrangThai') IS NULL ALTER TABLE DANH_MUC ADD TrangThai BIT NOT NULL CONSTRAINT DF_DM_TrangThai DEFAULT 1;
IF COL_LENGTH('DANH_MUC', 'NgayTao') IS NULL ALTER TABLE DANH_MUC ADD NgayTao DATETIME2 NOT NULL CONSTRAINT DF_DM_NgayTao DEFAULT SYSDATETIME();
IF COL_LENGTH('DANH_MUC', 'NgayCapNhat') IS NULL ALTER TABLE DANH_MUC ADD NgayCapNhat DATETIME2 NULL;
IF COL_LENGTH('DANH_MUC', 'NguoiTao') IS NULL ALTER TABLE DANH_MUC ADD NguoiTao INT NULL;
IF COL_LENGTH('DANH_MUC', 'NguoiCapNhat') IS NULL ALTER TABLE DANH_MUC ADD NguoiCapNhat INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DM_NguoiTao')
    ALTER TABLE DANH_MUC ADD CONSTRAINT FK_DM_NguoiTao FOREIGN KEY (NguoiTao) REFERENCES TAI_KHOAN(MaTK);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DM_NguoiCapNhat')
    ALTER TABLE DANH_MUC ADD CONSTRAINT FK_DM_NguoiCapNhat FOREIGN KEY (NguoiCapNhat) REFERENCES TAI_KHOAN(MaTK);
GO

-- ---------- Chỉ mục phục vụ tìm kiếm, lọc, phân trang ----------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SP_TrangThai_NgayTao' AND object_id = OBJECT_ID('SAN_PHAM'))
    CREATE INDEX IX_SP_TrangThai_NgayTao ON SAN_PHAM (TrangThai, NgayTao DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SP_MaDM' AND object_id = OBJECT_ID('SAN_PHAM'))
    CREATE INDEX IX_SP_MaDM ON SAN_PHAM (MaDM);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BT_MaSP' AND object_id = OBJECT_ID('BIEN_THE'))
    CREATE INDEX IX_BT_MaSP ON BIEN_THE (MaSP);
GO

PRINT N'02_alter_audit.sql: xong.';
GO
