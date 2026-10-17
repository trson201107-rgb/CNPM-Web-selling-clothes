using Microsoft.Data.SqlClient;
using ThoiTrangNam.Web.Data;
using ThoiTrangNam.Web.Models;

namespace ThoiTrangNam.Web.Repositories;

/// <summary>Giỏ hàng lưu trong CSDL (bảng GIO_HANG, CHI_TIET_GIO_HANG), mỗi tài khoản một giỏ.</summary>
public class CartRepository : ICartRepository
{
    private const int SoLuongToiDaMoiDong = 99;

    private readonly IDbConnectionFactory _factory;
    public CartRepository(IDbConnectionFactory factory) { _factory = factory; }

    public async Task<CartViewModel> LayGioHangAsync(int maTK, CancellationToken ct = default)
    {
        const string sql = @"
SELECT ct.MaBT, bt.MaSP, sp.TenSP, sp.AnhDaiDien, ms.TenMau, kc.TenSize, bt.SKU, sp.GiaBan,
       ct.SoLuong, bt.SoLuongTon, sp.TrangThai
FROM GIO_HANG gh
JOIN CHI_TIET_GIO_HANG ct ON ct.MaGH = gh.MaGH
JOIN BIEN_THE bt ON bt.MaBT = ct.MaBT
JOIN SAN_PHAM sp ON sp.MaSP = bt.MaSP
JOIN MAU_SAC ms ON ms.MaMau = bt.MaMau
JOIN KICH_CO kc ON kc.MaSize = bt.MaSize
WHERE gh.MaTK = @tk
ORDER BY ct.MaBT";
        var vm = new CartViewModel();
        await using var conn = await _factory.OpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@tk", maTK);
        await using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
        {
            vm.Lines.Add(new CartLine
            {
                MaBT = rd.Int("MaBT"),
                MaSP = rd.Int("MaSP"),
                TenSP = rd.Str("TenSP"),
                AnhDaiDien = rd.StrN("AnhDaiDien"),
                TenMau = rd.Str("TenMau"),
                TenSize = rd.Str("TenSize"),
                SKU = rd.Str("SKU"),
                GiaBan = rd.Dec("GiaBan"),
                SoLuong = rd.Int("SoLuong"),
                SoLuongTon = rd.Int("SoLuongTon"),
                DangBan = rd.Str("TrangThai") == TrangThaiSanPham.DangBan,
            });
        }
        return vm;
    }

    public async Task<int> DemSoLuongAsync(int maTK, CancellationToken ct = default)
    {
        await using var conn = await _factory.OpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(@"
SELECT ISNULL(SUM(ct.SoLuong), 0)
FROM GIO_HANG gh JOIN CHI_TIET_GIO_HANG ct ON ct.MaGH = gh.MaGH
WHERE gh.MaTK = @tk", conn);
        cmd.Parameters.AddWithValue("@tk", maTK);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
    }

    public async Task<CartResult> ThemAsync(int maTK, int maBT, int soLuong, CancellationToken ct = default)
    {
        soLuong = Math.Clamp(soLuong, 1, SoLuongToiDaMoiDong);

        await using var conn = await _factory.OpenConnectionAsync(ct);
        using var tx = conn.BeginTransaction();
        try
        {
            var maGH = await LayHoacTaoGioAsync(conn, tx, maTK, ct);

            // Biến thể phải tồn tại, sản phẩm đang bán và danh mục đang hiển thị
            int ton;
            await using (var cmd = new SqlCommand(@"
SELECT bt.SoLuongTon
FROM BIEN_THE bt
JOIN SAN_PHAM sp ON sp.MaSP = bt.MaSP
JOIN DANH_MUC dm ON dm.MaDM = sp.MaDM
WHERE bt.MaBT = @bt AND sp.TrangThai = N'Đang bán' AND dm.TrangThai = 1", conn, tx))
            {
                cmd.Parameters.AddWithValue("@bt", maBT);
                var v = await cmd.ExecuteScalarAsync(ct);
                if (v == null || v is DBNull) { Huy(tx); return CartResult.KhongTimThay; }
                ton = Convert.ToInt32(v);
            }
            if (ton <= 0) { Huy(tx); return CartResult.HetHang; }

            int hienCo = 0;
            bool daCoDong = false;
            await using (var cmd = new SqlCommand(
                "SELECT SoLuong FROM CHI_TIET_GIO_HANG WHERE MaGH = @gh AND MaBT = @bt", conn, tx))
            {
                cmd.Parameters.AddWithValue("@gh", maGH);
                cmd.Parameters.AddWithValue("@bt", maBT);
                var v = await cmd.ExecuteScalarAsync(ct);
                if (v != null && v is not DBNull) { hienCo = Convert.ToInt32(v); daCoDong = true; }
            }

            var moi = hienCo + soLuong;
            if (moi > ton) { Huy(tx); return CartResult.VuotTon; }

            var sqlGhi = daCoDong
                ? "UPDATE CHI_TIET_GIO_HANG SET SoLuong = @sl WHERE MaGH = @gh AND MaBT = @bt"
                : "INSERT INTO CHI_TIET_GIO_HANG (MaGH, MaBT, SoLuong) VALUES (@gh, @bt, @sl)";
            await using (var cmd = new SqlCommand(sqlGhi, conn, tx))
            {
                cmd.Parameters.AddWithValue("@gh", maGH);
                cmd.Parameters.AddWithValue("@bt", maBT);
                cmd.Parameters.AddWithValue("@sl", moi);
                await cmd.ExecuteNonQueryAsync(ct);
            }
            await CapNhatNgayAsync(conn, tx, maGH, ct);

            tx.Commit();
            return CartResult.ThanhCong;
        }
        catch
        {
            Huy(tx);
            throw;
        }
    }

    public async Task<CartResult> CapNhatSoLuongAsync(int maTK, int maBT, int soLuong, CancellationToken ct = default)
    {
        if (soLuong <= 0)
        {
            await XoaAsync(maTK, maBT, ct);
            return CartResult.ThanhCong;
        }
        soLuong = Math.Min(soLuong, SoLuongToiDaMoiDong);

        await using var conn = await _factory.OpenConnectionAsync(ct);
        using var tx = conn.BeginTransaction();
        try
        {
            var maGH = await LayHoacTaoGioAsync(conn, tx, maTK, ct);

            int ton;
            await using (var cmd = new SqlCommand("SELECT SoLuongTon FROM BIEN_THE WHERE MaBT = @bt", conn, tx))
            {
                cmd.Parameters.AddWithValue("@bt", maBT);
                var v = await cmd.ExecuteScalarAsync(ct);
                if (v == null || v is DBNull) { Huy(tx); return CartResult.KhongTimThay; }
                ton = Convert.ToInt32(v);
            }
            if (soLuong > ton) { Huy(tx); return CartResult.VuotTon; }

            int daSua;
            await using (var cmd = new SqlCommand(
                "UPDATE CHI_TIET_GIO_HANG SET SoLuong = @sl WHERE MaGH = @gh AND MaBT = @bt", conn, tx))
            {
                cmd.Parameters.AddWithValue("@sl", soLuong);
                cmd.Parameters.AddWithValue("@gh", maGH);
                cmd.Parameters.AddWithValue("@bt", maBT);
                daSua = await cmd.ExecuteNonQueryAsync(ct);
            }
            if (daSua == 0) { Huy(tx); return CartResult.KhongTimThay; }
            await CapNhatNgayAsync(conn, tx, maGH, ct);

            tx.Commit();
            return CartResult.ThanhCong;
        }
        catch
        {
            Huy(tx);
            throw;
        }
    }

    public async Task XoaAsync(int maTK, int maBT, CancellationToken ct = default)
    {
        await using var conn = await _factory.OpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(@"
DELETE ct FROM CHI_TIET_GIO_HANG ct
JOIN GIO_HANG gh ON gh.MaGH = ct.MaGH
WHERE gh.MaTK = @tk AND ct.MaBT = @bt", conn);
        cmd.Parameters.AddWithValue("@tk", maTK);
        cmd.Parameters.AddWithValue("@bt", maBT);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    // ------------------------------------------------------------------
    private static async Task<int> LayHoacTaoGioAsync(SqlConnection conn, SqlTransaction tx, int maTK, CancellationToken ct)
    {
        await using (var cmd = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM GIO_HANG WHERE MaTK = @tk) INSERT INTO GIO_HANG (MaTK) VALUES (@tk);", conn, tx))
        {
            cmd.Parameters.AddWithValue("@tk", maTK);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        await using (var cmd = new SqlCommand("SELECT MaGH FROM GIO_HANG WHERE MaTK = @tk", conn, tx))
        {
            cmd.Parameters.AddWithValue("@tk", maTK);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
        }
    }

    private static async Task CapNhatNgayAsync(SqlConnection conn, SqlTransaction tx, int maGH, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("UPDATE GIO_HANG SET NgayCapNhat = SYSDATETIME() WHERE MaGH = @gh", conn, tx);
        cmd.Parameters.AddWithValue("@gh", maGH);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static void Huy(SqlTransaction tx)
    {
        try { tx.Rollback(); } catch { }
    }
}
