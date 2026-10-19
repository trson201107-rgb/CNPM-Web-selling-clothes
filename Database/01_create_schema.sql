-- =====================================================================
-- CSDL: WEBSITE QUẢN LÝ THỜI TRANG NAM THƯỜNG NHẬT
-- Hệ quản trị CSDL: Microsoft SQL Server (T-SQL)
-- Chuẩn hóa: 3NF
-- =====================================================================
CREATE DATABASE ThoiTrangNam;
GO
USE ThoiTrangNam;
GO

-- ---------------------------------------------------------------------
-- 1. NHÓM TÀI KHOẢN VÀ THÀNH VIÊN
-- ---------------------------------------------------------------------
CREATE TABLE VAI_TRO (
    MaVaiTro    INT IDENTITY(1,1) NOT NULL,
    TenVaiTro   NVARCHAR(30)      NOT NULL,
    CONSTRAINT PK_VAI_TRO PRIMARY KEY (MaVaiTro),
    CONSTRAINT UQ_VAI_TRO_Ten UNIQUE (TenVaiTro)
);

CREATE TABLE HANG_THANH_VIEN (
    MaHang           INT IDENTITY(1,1) NOT NULL,
    TenHang          NVARCHAR(30)      NOT NULL,
    ChiTieuToiThieu  DECIMAL(12,0)     NOT NULL DEFAULT 0,
    PhanTramUuDai    INT               NOT NULL DEFAULT 0,
    CONSTRAINT PK_HANG_THANH_VIEN PRIMARY KEY (MaHang),
    CONSTRAINT UQ_HANG_Ten UNIQUE (TenHang),
    CONSTRAINT CK_HANG_UuDai CHECK (PhanTramUuDai BETWEEN 0 AND 100)
);

CREATE TABLE TAI_KHOAN (
    MaTK              INT IDENTITY(1,1) NOT NULL,
    HoTen             NVARCHAR(100)     NOT NULL,
    Email             VARCHAR(100)      NOT NULL,
    MatKhauHash       VARCHAR(255)      NOT NULL,
    SoDienThoai       VARCHAR(15)       NOT NULL,
    TrangThai         NVARCHAR(20)      NOT NULL DEFAULT N'Hoạt động',
    SoLanDangNhapSai  INT               NOT NULL DEFAULT 0,
    KhoaDen           DATETIME2         NULL,
    NgayTao           DATETIME2         NOT NULL DEFAULT SYSDATETIME(),
    MaVaiTro          INT               NOT NULL,
    MaHang            INT               NULL,      -- chỉ áp dụng cho Customer
    CONSTRAINT PK_TAI_KHOAN PRIMARY KEY (MaTK),
    CONSTRAINT UQ_TK_Email UNIQUE (Email),
    CONSTRAINT UQ_TK_SDT UNIQUE (SoDienThoai),
    CONSTRAINT CK_TK_TrangThai CHECK (TrangThai IN (N'Hoạt động', N'Bị khóa')),
    CONSTRAINT FK_TK_VaiTro FOREIGN KEY (MaVaiTro) REFERENCES VAI_TRO(MaVaiTro),
    CONSTRAINT FK_TK_Hang   FOREIGN KEY (MaHang)   REFERENCES HANG_THANH_VIEN(MaHang)
);

-- ---------------------------------------------------------------------
-- 2. NHÓM SẢN PHẨM VÀ TỒN KHO
-- ---------------------------------------------------------------------
CREATE TABLE DANH_MUC (
    MaDM    INT IDENTITY(1,1) NOT NULL,
    TenDM   NVARCHAR(50)      NOT NULL,
    MoTa    NVARCHAR(255)     NULL,
    CONSTRAINT PK_DANH_MUC PRIMARY KEY (MaDM),
    CONSTRAINT UQ_DM_Ten UNIQUE (TenDM)
);

CREATE TABLE SAN_PHAM (
    MaSP        INT IDENTITY(1,1) NOT NULL,
    TenSP       NVARCHAR(150)     NOT NULL,
    MoTa        NVARCHAR(1000)    NULL,
    GiaBan      DECIMAL(12,0)     NOT NULL,
    AnhDaiDien  VARCHAR(255)      NULL,
    TrangThai   NVARCHAR(20)      NOT NULL DEFAULT N'Đang bán',
    NgayTao     DATETIME2         NOT NULL DEFAULT SYSDATETIME(),
    MaDM        INT               NOT NULL,
    CONSTRAINT PK_SAN_PHAM PRIMARY KEY (MaSP),
    CONSTRAINT CK_SP_Gia CHECK (GiaBan >= 0),
    CONSTRAINT CK_SP_TrangThai CHECK (TrangThai IN (N'Đang bán', N'Ngừng bán')),
    CONSTRAINT FK_SP_DM FOREIGN KEY (MaDM) REFERENCES DANH_MUC(MaDM)
);

CREATE TABLE MAU_SAC (
    MaMau   INT IDENTITY(1,1) NOT NULL,
    TenMau  NVARCHAR(30)      NOT NULL,
    MaHex   CHAR(7)           NULL,
    CONSTRAINT PK_MAU_SAC PRIMARY KEY (MaMau),
    CONSTRAINT UQ_MAU_Ten UNIQUE (TenMau)
);

CREATE TABLE KICH_CO (
    MaSize     INT IDENTITY(1,1) NOT NULL,
    TenSize    VARCHAR(10)       NOT NULL,
    ThuTu      INT               NOT NULL,          -- để sắp xếp S < M < L < XL
    CONSTRAINT PK_KICH_CO PRIMARY KEY (MaSize),
    CONSTRAINT UQ_SIZE_Ten UNIQUE (TenSize)
);

CREATE TABLE BIEN_THE (
    MaBT        INT IDENTITY(1,1) NOT NULL,
    SKU         VARCHAR(30)       NOT NULL,
    SoLuongTon  INT               NOT NULL DEFAULT 0,
    MaSP        INT               NOT NULL,
    MaMau       INT               NOT NULL,
    MaSize      INT               NOT NULL,
    CONSTRAINT PK_BIEN_THE PRIMARY KEY (MaBT),
    CONSTRAINT UQ_BT_SKU UNIQUE (SKU),
    CONSTRAINT UQ_BT_SP_Mau_Size UNIQUE (MaSP, MaMau, MaSize),
    CONSTRAINT CK_BT_Ton CHECK (SoLuongTon >= 0),   -- không bán vượt tồn kho
    CONSTRAINT FK_BT_SP   FOREIGN KEY (MaSP)   REFERENCES SAN_PHAM(MaSP),
    CONSTRAINT FK_BT_Mau  FOREIGN KEY (MaMau)  REFERENCES MAU_SAC(MaMau),
    CONSTRAINT FK_BT_Size FOREIGN KEY (MaSize) REFERENCES KICH_CO(MaSize)
);

-- ---------------------------------------------------------------------
-- 3. NHÓM GIỎ HÀNG
-- ---------------------------------------------------------------------
CREATE TABLE GIO_HANG (
    MaGH          INT IDENTITY(1,1) NOT NULL,
    NgayCapNhat   DATETIME2         NOT NULL DEFAULT SYSDATETIME(),
    MaTK          INT               NOT NULL,
    CONSTRAINT PK_GIO_HANG PRIMARY KEY (MaGH),
    CONSTRAINT UQ_GH_TK UNIQUE (MaTK),               -- quan hệ 1-1 với TAI_KHOAN
    CONSTRAINT FK_GH_TK FOREIGN KEY (MaTK) REFERENCES TAI_KHOAN(MaTK)
);

CREATE TABLE CHI_TIET_GIO_HANG (
    MaGH     INT NOT NULL,
    MaBT     INT NOT NULL,
    SoLuong  INT NOT NULL DEFAULT 1,
    CONSTRAINT PK_CT_GIO_HANG PRIMARY KEY (MaGH, MaBT),
    CONSTRAINT CK_CTGH_SoLuong CHECK (SoLuong > 0),
    CONSTRAINT FK_CTGH_GH FOREIGN KEY (MaGH) REFERENCES GIO_HANG(MaGH) ON DELETE CASCADE,
    CONSTRAINT FK_CTGH_BT FOREIGN KEY (MaBT) REFERENCES BIEN_THE(MaBT)
);

-- ---------------------------------------------------------------------
-- 4. NHÓM KHUYẾN MÃI
-- ---------------------------------------------------------------------
CREATE TABLE VOUCHER (
    MaVoucher          INT IDENTITY(1,1) NOT NULL,
    MaCode             VARCHAR(30)       NOT NULL,
    LoaiGiam           VARCHAR(10)       NOT NULL,     -- PHAN_TRAM | TIEN
    GiaTriGiam         DECIMAL(12,0)     NOT NULL,
    DonToiThieu        DECIMAL(12,0)     NOT NULL DEFAULT 0,
    SoLuotConLai       INT               NOT NULL,
    NgayBatDau         DATE              NOT NULL,
    NgayHetHan         DATE              NOT NULL,
    CONSTRAINT PK_VOUCHER PRIMARY KEY (MaVoucher),
    CONSTRAINT UQ_VOUCHER_Code UNIQUE (MaCode),
    CONSTRAINT CK_VOUCHER_Loai CHECK (LoaiGiam IN ('PHAN_TRAM', 'TIEN')),
    CONSTRAINT CK_VOUCHER_Ngay CHECK (NgayHetHan >= NgayBatDau)
);

CREATE TABLE VI_VOUCHER (
    MaTK        INT       NOT NULL,
    MaVoucher   INT       NOT NULL,
    NgayNhan    DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    DaSuDung    BIT       NOT NULL DEFAULT 0,
    CONSTRAINT PK_VI_VOUCHER PRIMARY KEY (MaTK, MaVoucher),
    CONSTRAINT FK_VV_TK      FOREIGN KEY (MaTK)      REFERENCES TAI_KHOAN(MaTK),
    CONSTRAINT FK_VV_Voucher FOREIGN KEY (MaVoucher) REFERENCES VOUCHER(MaVoucher)
);

-- ---------------------------------------------------------------------
-- 5. NHÓM ĐƠN HÀNG VÀ THANH TOÁN
-- ---------------------------------------------------------------------
CREATE TABLE TRANG_THAI_DON (
    MaTrangThai   INT IDENTITY(1,1) NOT NULL,
    TenTrangThai  NVARCHAR(30)      NOT NULL,
    CONSTRAINT PK_TRANG_THAI_DON PRIMARY KEY (MaTrangThai),
    CONSTRAINT UQ_TTD_Ten UNIQUE (TenTrangThai)
);

CREATE TABLE PHUONG_THUC_THANH_TOAN (
    MaPT   INT IDENTITY(1,1) NOT NULL,
    TenPT  NVARCHAR(30)      NOT NULL,
    CONSTRAINT PK_PTTT PRIMARY KEY (MaPT),
    CONSTRAINT UQ_PTTT_Ten UNIQUE (TenPT)
);

CREATE TABLE DON_HANG (
    MaDH            INT IDENTITY(1,1) NOT NULL,
    NgayDat         DATETIME2         NOT NULL DEFAULT SYSDATETIME(),
    TenNguoiNhan    NVARCHAR(100)     NOT NULL,
    SDTNguoiNhan    VARCHAR(15)       NOT NULL,
    DiaChiGiao      NVARCHAR(255)     NOT NULL,
    PhiVanChuyen    DECIMAL(12,0)     NOT NULL DEFAULT 0,
    TienGiam        DECIMAL(12,0)     NOT NULL DEFAULT 0,
    GhiChu          NVARCHAR(255)     NULL,
    MaTK            INT               NOT NULL,
    MaTrangThai     INT               NOT NULL,
    MaVoucher       INT               NULL,          -- mỗi đơn áp dụng tối đa 1 mã
    CONSTRAINT PK_DON_HANG PRIMARY KEY (MaDH),
    CONSTRAINT FK_DH_TK        FOREIGN KEY (MaTK)        REFERENCES TAI_KHOAN(MaTK),
    CONSTRAINT FK_DH_TrangThai FOREIGN KEY (MaTrangThai) REFERENCES TRANG_THAI_DON(MaTrangThai),
    CONSTRAINT FK_DH_Voucher   FOREIGN KEY (MaVoucher)   REFERENCES VOUCHER(MaVoucher)
);

CREATE TABLE CHI_TIET_DON_HANG (
    MaDH INT NOT NULL,
    MaBT INT NOT NULL,
    SoLuong INT NOT NULL,
    DonGia DECIMAL(12,0) NOT NULL,

    CONSTRAINT PK_CT_DON_HANG PRIMARY KEY (MaDH, MaBT),
    CONSTRAINT CK_CTDH_SoLuong CHECK (SoLuong > 0),
    CONSTRAINT FK_CTDH_DH FOREIGN KEY (MaDH) REFERENCES DON_HANG(MaDH),
    CONSTRAINT FK_CTDH_BT FOREIGN KEY (MaBT) REFERENCES BIEN_THE(MaBT)
);

CREATE TABLE THANH_TOAN (
    MaTT            INT IDENTITY(1,1) NOT NULL,
    SoTien          DECIMAL(12,0)     NOT NULL,
    TrangThaiTT     NVARCHAR(20)      NOT NULL DEFAULT N'Chưa thanh toán',
    NgayThanhToan   DATETIME2         NULL,
    MaDH            INT               NOT NULL,
    MaPT            INT               NOT NULL,
    CONSTRAINT PK_THANH_TOAN PRIMARY KEY (MaTT),
    CONSTRAINT UQ_TT_DH UNIQUE (MaDH),               -- quan hệ 1-1 với DON_HANG
    CONSTRAINT CK_TT_TrangThai CHECK (TrangThaiTT IN (N'Chưa thanh toán', N'Đã thanh toán')),
    CONSTRAINT FK_TT_DH FOREIGN KEY (MaDH) REFERENCES DON_HANG(MaDH),
    CONSTRAINT FK_TT_PT FOREIGN KEY (MaPT) REFERENCES PHUONG_THUC_THANH_TOAN(MaPT)
);

CREATE TABLE LICH_SU_DON_HANG (
    MaLS         INT IDENTITY(1,1) NOT NULL,
    ThoiGian     DATETIME2         NOT NULL DEFAULT SYSDATETIME(),
    GhiChu       NVARCHAR(255)     NULL,
    MaDH         INT               NOT NULL,
    MaTrangThai  INT               NOT NULL,
    MaNguoiDoi   INT               NULL,            -- NULL nếu hệ thống tự động
    CONSTRAINT PK_LICH_SU_DH PRIMARY KEY (MaLS),
    CONSTRAINT FK_LS_DH        FOREIGN KEY (MaDH)        REFERENCES DON_HANG(MaDH),
    CONSTRAINT FK_LS_TrangThai FOREIGN KEY (MaTrangThai) REFERENCES TRANG_THAI_DON(MaTrangThai),
    CONSTRAINT FK_LS_NguoiDoi  FOREIGN KEY (MaNguoiDoi)  REFERENCES TAI_KHOAN(MaTK)
);

-- ---------------------------------------------------------------------
-- 6. ĐÁNH GIÁ SẢN PHẨM
-- ---------------------------------------------------------------------
IF OBJECT_ID('DANH_GIA', 'U') IS NOT NULL
    DROP TABLE DANH_GIA;
GO

CREATE TABLE DANH_GIA (
    MaDG INT IDENTITY(1,1) NOT NULL,
    SoSao INT NOT NULL,
    NoiDung NVARCHAR(500) NULL,
    NgayDG DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    MaTK INT NOT NULL,
    MaSP INT NOT NULL,

    CONSTRAINT PK_DANH_GIA PRIMARY KEY (MaDG),
    CONSTRAINT UQ_DG_TK_SP UNIQUE (MaTK, MaSP),
    CONSTRAINT CK_DG_SoSao CHECK (SoSao BETWEEN 1 AND 5),
    CONSTRAINT FK_DG_TK FOREIGN KEY (MaTK) REFERENCES TAI_KHOAN(MaTK),
    CONSTRAINT FK_DG_SP FOREIGN KEY (MaSP) REFERENCES SAN_PHAM(MaSP)
);
GO
-- =====================================================================
-- DỮ LIỆU MẪU
-- =====================================================================
INSERT INTO VAI_TRO (TenVaiTro) VALUES (N'Customer'), (N'Staff'), (N'Admin');

INSERT INTO HANG_THANH_VIEN (TenHang, ChiTieuToiThieu, PhanTramUuDai) VALUES
(N'Đồng', 0, 5), (N'Bạc', 3000000, 10), (N'Vàng', 7000000, 15), (N'Kim cương', 15000000, 20);

INSERT INTO TAI_KHOAN (HoTen, Email, MatKhauHash, SoDienThoai, MaVaiTro, MaHang) VALUES
(N'Nguyễn Văn An', 'an.nguyen@gmail.com', 'hash_demo_01', '0901000001', 1, 1),
(N'Trần Thị Bình', 'binh.tran@gmail.com', 'hash_demo_02', '0901000002', 1, 2),
(N'Lê Quốc Cường', 'cuong.le@shop.vn',   'hash_demo_03', '0901000003', 2, NULL),
(N'Phạm Minh Đức', 'duc.pham@shop.vn',   'hash_demo_04', '0901000004', 3, NULL);

INSERT INTO DANH_MUC (TenDM, MoTa) VALUES
(N'Áo thun', N'Áo thun nam thường nhật'),
(N'Áo sơ mi', N'Sơ mi nam công sở và dạo phố'),
(N'Quần jeans', N'Quần jeans nam'),
(N'Quần kaki', N'Quần kaki nam'),
(N'Phụ kiện', N'Nón, thắt lưng, ví');

INSERT INTO SAN_PHAM (TenSP, MoTa, GiaBan, MaDM) VALUES
(N'Áo thun cotton basic', N'Cotton 100%, form regular', 199000, 1),
(N'Áo sơ mi Oxford tay dài', N'Vải Oxford dày dặn', 359000, 2),
(N'Quần jeans slim-fit', N'Denim co giãn nhẹ', 459000, 3),
(N'Quần kaki ống đứng', N'Kaki cotton', 399000, 4);

INSERT INTO MAU_SAC (TenMau, MaHex) VALUES (N'Trắng', '#FFFFFF'), (N'Đen', '#000000'), (N'Xanh navy', '#1F2A44');
INSERT INTO KICH_CO (TenSize, ThuTu) VALUES ('S', 1), ('M', 2), ('L', 3), ('XL', 4);

INSERT INTO BIEN_THE (SKU, SoLuongTon, MaSP, MaMau, MaSize) VALUES
('AT-BASIC-TRG-M',  25, 1, 1, 2),
('AT-BASIC-DEN-L',  18, 1, 2, 3),
('SM-OXF-TRG-M',    12, 2, 1, 2),
('SM-OXF-TRG-L',     0, 2, 1, 3),
('QJ-SLIM-NAV-L',   10, 3, 3, 3),
('QJ-SLIM-DEN-XL',   6, 3, 2, 4),
('QK-ONGDUNG-DEN-M', 15, 4, 2, 2);

INSERT INTO GIO_HANG (MaTK) VALUES (1), (2);
INSERT INTO CHI_TIET_GIO_HANG (MaGH, MaBT, SoLuong) VALUES (1, 1, 2), (1, 5, 1), (2, 3, 1);

INSERT INTO VOUCHER (MaCode, LoaiGiam, GiaTriGiam, DonToiThieu, SoLuotConLai, NgayBatDau, NgayHetHan) VALUES
('FREESHIP30', 'TIEN',      30000, 300000, 100, '2026-01-01', '2026-12-31'),
('GIAM10',     'PHAN_TRAM',    10, 500000,  50, '2026-01-01', '2026-12-31');
INSERT INTO VI_VOUCHER (MaTK, MaVoucher) VALUES (1, 1), (1, 2), (2, 1);

INSERT INTO TRANG_THAI_DON (TenTrangThai) VALUES
(N'Chờ xác nhận'), (N'Đã xác nhận'), (N'Đang đóng gói'), (N'Đang giao'), (N'Hoàn thành'), (N'Đã hủy');
INSERT INTO PHUONG_THUC_THANH_TOAN (TenPT) VALUES (N'COD'), (N'QR');

-- Đơn hàng 1: khách An mua 2 áo thun + 1 quần jeans, dùng mã FREESHIP30
INSERT INTO DON_HANG (TenNguoiNhan, SDTNguoiNhan, DiaChiGiao, PhiVanChuyen, TienGiam, MaTK, MaTrangThai, MaVoucher)
VALUES (N'Nguyễn Văn An', '0901000001', N'12 Trần Hưng Đạo, Hoàn Kiếm, Hà Nội', 30000, 30000, 1, 4, 1);
INSERT INTO CHI_TIET_DON_HANG (MaDH, MaBT, SoLuong, DonGia) VALUES (1, 1, 2, 199000), (1, 5, 1, 459000);
INSERT INTO THANH_TOAN (SoTien, TrangThaiTT, MaDH, MaPT) VALUES (857000, N'Chưa thanh toán', 1, 1);
INSERT INTO LICH_SU_DON_HANG (MaDH, MaTrangThai, MaNguoiDoi, GhiChu) VALUES
(1, 1, NULL, N'Khách đặt hàng'),
(1, 2, 3,    N'Nhân viên xác nhận'),
(1, 4, 3,    N'Bàn giao cho đơn vị vận chuyển');

INSERT INTO DANH_GIA (SoSao, NoiDung, MaTK, MaSP) VALUES (5, N'Vải mát, mặc vừa vặn', 1, 1);
GO

-- =====================================================================
-- VIEW: TỔNG TIỀN ĐƠN HÀNG (thuộc tính suy diễn, không lưu trong bảng)
-- =====================================================================
CREATE or ALTER VIEW vw_TongTienDonHang AS
SELECT  dh.MaDH,
        SUM(ct.SoLuong * ct.DonGia)                              AS TienHang,
        dh.PhiVanChuyen,
        dh.TienGiam,
        SUM(ct.SoLuong * ct.DonGia) + dh.PhiVanChuyen - dh.TienGiam AS TongThanhToan
FROM    DON_HANG dh
JOIN    CHI_TIET_DON_HANG ct ON ct.MaDH = dh.MaDH
GROUP BY dh.MaDH, dh.PhiVanChuyen, dh.TienGiam;
GO

-- =====================================================================
-- TRUY VẤN MẪU
-- =====================================================================
-- (1) Lọc đa tiêu chí: Quần jeans + Size L + Giá dưới 500.000đ + còn hàng
SELECT sp.MaSP, sp.TenSP, sp.GiaBan, ms.TenMau, kc.TenSize, bt.SoLuongTon
FROM   SAN_PHAM sp
JOIN   DANH_MUC dm ON dm.MaDM   = sp.MaDM
JOIN   BIEN_THE bt ON bt.MaSP   = sp.MaSP
JOIN   MAU_SAC  ms ON ms.MaMau  = bt.MaMau
JOIN   KICH_CO  kc ON kc.MaSize = bt.MaSize
WHERE  dm.TenDM = N'Quần jeans'
  AND  kc.TenSize = 'L'
  AND  sp.GiaBan < 500000
  AND  bt.SoLuongTon > 0
  AND  sp.TrangThai = N'Đang bán';

-- (2) Theo dõi đơn hàng của một khách hàng (kèm trạng thái hiện tại)
SELECT dh.MaDH, dh.NgayDat, tt.TenTrangThai, v.TongThanhToan
FROM   DON_HANG dh
JOIN   TRANG_THAI_DON tt ON tt.MaTrangThai = dh.MaTrangThai
JOIN   vw_TongTienDonHang v ON v.MaDH = dh.MaDH
WHERE  dh.MaTK = 1
ORDER BY dh.NgayDat DESC;

-- (3) Tổng chi tiêu của khách hàng (chỉ tính đơn Hoàn thành) để xét hạng
SELECT tk.MaTK, tk.HoTen, SUM(v.TongThanhToan) AS TongChiTieu
FROM   TAI_KHOAN tk
JOIN   DON_HANG dh ON dh.MaTK = tk.MaTK
JOIN   TRANG_THAI_DON tt ON tt.MaTrangThai = dh.MaTrangThai AND tt.TenTrangThai = N'Hoàn thành'
JOIN   vw_TongTienDonHang v ON v.MaDH = dh.MaDH
GROUP BY tk.MaTK, tk.HoTen;
GO

-- =====================================================================
-- GIAO DỊCH ĐẶT HÀNG: tạo đơn + trừ tồn kho trong MỘT transaction (T-SQL)
-- =====================================================================
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @MaDH INT;

    INSERT INTO DON_HANG (TenNguoiNhan, SDTNguoiNhan, DiaChiGiao, PhiVanChuyen, MaTK, MaTrangThai)
    VALUES (N'Trần Thị Bình', '0901000002', N'25 Lê Lợi, Hai Bà Trưng, Hà Nội', 30000, 2, 1);
    SET @MaDH = SCOPE_IDENTITY();

    -- Chỉ trừ kho khi còn đủ hàng; nếu không đủ -> @@ROWCOUNT = 0 -> hủy giao dịch
    UPDATE BIEN_THE SET SoLuongTon = SoLuongTon - 1 WHERE MaBT = 3 AND SoLuongTon >= 1;
    IF @@ROWCOUNT = 0
        THROW 50001, N'Không đủ tồn kho', 1;

    INSERT INTO CHI_TIET_DON_HANG (MaDH, MaBT, SoLuong, DonGia)
    SELECT @MaDH, 3, 1, sp.GiaBan FROM BIEN_THE bt JOIN SAN_PHAM sp ON sp.MaSP = bt.MaSP WHERE bt.MaBT = 3;

    INSERT INTO THANH_TOAN (SoTien, MaDH, MaPT) VALUES (389000, @MaDH, 1);
    INSERT INTO LICH_SU_DON_HANG (MaDH, MaTrangThai, GhiChu) VALUES (@MaDH, 1, N'Khách đặt hàng');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO