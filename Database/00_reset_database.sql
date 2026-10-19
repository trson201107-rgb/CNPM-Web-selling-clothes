-- =====================================================================
-- 00_reset_database.sql
-- XÓA HẲN database ThoiTrangNam để chạy lại từ đầu (01 -> 02 -> 03).
-- CHỈ DÙNG TRÊN MÁY CÁ NHÂN. Mọi dữ liệu trong database này sẽ mất.
-- =====================================================================
USE master;
GO
IF DB_ID(N'ThoiTrangNam') IS NOT NULL
BEGIN
    ALTER DATABASE ThoiTrangNam SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE ThoiTrangNam;
END
GO
