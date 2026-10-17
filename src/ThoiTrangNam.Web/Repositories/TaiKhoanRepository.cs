using Microsoft.Data.SqlClient;
using ThoiTrangNam.Web.Data;
using ThoiTrangNam.Web.Models;
using ThoiTrangNam.Web.Services;

namespace ThoiTrangNam.Web.Repositories;

public class TaiKhoanRepository : ITaiKhoanRepository
{
    private readonly IDbConnectionFactory _factory;
    public TaiKhoanRepository(IDbConnectionFactory factory) { _factory = factory; }

    public async Task<TaiKhoan?> LayTheoEmailAsync(string email, CancellationToken ct = default)
    {
        // Việc so sánh thời gian khóa làm ngay trong SQL để không phụ thuộc múi giờ của máy chạy ứng dụng.
        const string sql = @"
SELECT tk.MaTK, tk.HoTen, tk.Email, tk.MatKhauHash, tk.SoDienThoai, tk.TrangThai, tk.SoLanDangNhapSai,
       tk.MaVaiTro, vt.TenVaiTro,
       CASE WHEN tk.KhoaDen IS NOT NULL AND tk.KhoaDen >  SYSDATETIME() THEN 1 ELSE 0 END AS DangKhoa,
       CASE WHEN tk.KhoaDen IS NOT NULL AND tk.KhoaDen <= SYSDATETIME() THEN 1 ELSE 0 END AS KhoaDaHetHan,
       CASE WHEN tk.KhoaDen IS NOT NULL AND tk.KhoaDen >  SYSDATETIME()
            THEN DATEDIFF(MINUTE, SYSDATETIME(), tk.KhoaDen) + 1 ELSE 0 END AS SoPhutKhoaConLai
FROM TAI_KHOAN tk
JOIN VAI_TRO vt ON vt.MaVaiTro = tk.MaVaiTro
WHERE tk.Email = @email";
        await using var conn = await _factory.OpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@email", email);
        await using var rd = await cmd.ExecuteReaderAsync(ct);
        if (!await rd.ReadAsync(ct)) return null;
        return new TaiKhoan
        {
            MaTK = rd.Int("MaTK"),
            HoTen = rd.Str("HoTen"),
            Email = rd.Str("Email"),
            MatKhauHash = rd.Str("MatKhauHash"),
            SoDienThoai = rd.Str("SoDienThoai"),
            TrangThai = rd.Str("TrangThai"),
            SoLanDangNhapSai = rd.Int("SoLanDangNhapSai"),
            MaVaiTro = rd.Int("MaVaiTro"),
            TenVaiTro = rd.Str("TenVaiTro"),
            DangKhoa = rd.Int("DangKhoa") == 1,
            KhoaDaHetHan = rd.Int("KhoaDaHetHan") == 1,
            SoPhutKhoaConLai = rd.Int("SoPhutKhoaConLai"),
        };
    }

    public async Task<int> TaoKhachHangAsync(string hoTen, string email, string matKhauHash, string soDienThoai, CancellationToken ct = default)
    {
        await using var conn = await _factory.OpenConnectionAsync(ct);
        using var tx = conn.BeginTransaction();
        try
        {
            int maTK;
            await using (var cmd = new SqlCommand(@"
INSERT INTO TAI_KHOAN (HoTen, Email, MatKhauHash, SoDienThoai, MaVaiTro, MaHang)
VALUES (@ten, @email, @hash, @sdt,
        (SELECT MaVaiTro FROM VAI_TRO WHERE TenVaiTro = N'Customer'),
        (SELECT TOP 1 MaHang FROM HANG_THANH_VIEN ORDER BY ChiTieuToiThieu));
SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
            {
                cmd.Parameters.AddWithValue("@ten", hoTen.Trim());
                cmd.Parameters.AddWithValue("@email", email.Trim());
                cmd.Parameters.AddWithValue("@hash", matKhauHash);
                cmd.Parameters.AddWithValue("@sdt", soDienThoai.Trim());
                maTK = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
            }
            await using (var cmd = new SqlCommand("INSERT INTO GIO_HANG (MaTK) VALUES (@tk)", conn, tx))
            {
                cmd.Parameters.AddWithValue("@tk", maTK);
                await cmd.ExecuteNonQueryAsync(ct);
            }
            tx.Commit();
            return maTK;
        }
        catch (SqlException ex)
        {
            try { tx.Rollback(); } catch { }
            if (ex.Number == 2627 || ex.Number == 2601)
            {
                if (ex.Message.Contains("UQ_TK_SDT"))
                    throw new NghiepVuException("Số điện thoại này đã được đăng ký.");
                throw new NghiepVuException("Email này đã được đăng ký.");
            }
            throw;
        }
        catch
        {
            try { tx.Rollback(); } catch { }
            throw;
        }
    }

    public async Task GhiNhanDangNhapSaiAsync(int maTK, CancellationToken ct = default)
    {
        await using var conn = await _factory.OpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(@"
UPDATE TAI_KHOAN
SET SoLanDangNhapSai = SoLanDangNhapSai + 1,
    KhoaDen = CASE WHEN SoLanDangNhapSai + 1 >= 5 THEN DATEADD(MINUTE, 15, SYSDATETIME()) ELSE KhoaDen END
WHERE MaTK = @id", conn);
        cmd.Parameters.AddWithValue("@id", maTK);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task DatLaiDangNhapSaiAsync(int maTK, CancellationToken ct = default)
    {
        await using var conn = await _factory.OpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(
            "UPDATE TAI_KHOAN SET SoLanDangNhapSai = 0, KhoaDen = NULL WHERE MaTK = @id", conn);
        cmd.Parameters.AddWithValue("@id", maTK);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
