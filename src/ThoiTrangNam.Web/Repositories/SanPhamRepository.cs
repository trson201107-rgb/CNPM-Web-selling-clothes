using System.Text;
using Microsoft.Data.SqlClient;
using ThoiTrangNam.Web.Data;
using ThoiTrangNam.Web.Models;
using ThoiTrangNam.Web.Services;

namespace ThoiTrangNam.Web.Repositories;

/// <summary>
/// Truy cập dữ liệu sản phẩm và biến thể. Quy tắc chung:
/// - mọi giá trị người dùng nhập đều truyền qua tham số (chống SQL Injection);
/// - thao tác nhiều bảng chạy trong transaction, lỗi giữa chừng thì rollback.
/// </summary>
public class SanPhamRepository : ISanPhamRepository
{
    private readonly IDbConnectionFactory _factory;
    public SanPhamRepository(IDbConnectionFactory factory) { _factory = factory; }

    // =====================================================================
    // DANH SÁCH ADMIN
    // =====================================================================
    public async Task<List<SanPhamAdminItem>> LayDanhSachAdminAsync(string? tuKhoa, string? trangThai, CancellationToken ct = default)
    {
        var sql = new StringBuilder(@"
SELECT sp.MaSP, sp.TenSP, dm.TenDM, sp.GiaBan, sp.TrangThai, sp.LaNoiBat, sp.NgayTao, sp.NgayCapNhat,
       ISNULL((SELECT COUNT(*) FROM BIEN_THE b WHERE b.MaSP = sp.MaSP), 0)        AS SoBienThe,
       ISNULL((SELECT SUM(b.SoLuongTon) FROM BIEN_THE b WHERE b.MaSP = sp.MaSP), 0) AS TongTon,
       tk.HoTen AS NguoiCapNhatTen
FROM SAN_PHAM sp
JOIN DANH_MUC dm ON dm.MaDM = sp.MaDM
LEFT JOIN TAI_KHOAN tk ON tk.MaTK = sp.NguoiCapNhat
WHERE 1 = 1");
        var ps = new List<(string, object)>();
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            sql.Append(" AND sp.TenSP LIKE @q");
            ps.Add(("@q", "%" + SqlText.EscapeLike(tuKhoa.Trim()) + "%"));
        }
        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            sql.Append(" AND sp.TrangThai = @tt");
            ps.Add(("@tt", trangThai));
        }
        sql.Append(" ORDER BY sp.MaSP DESC");

        var list = new List<SanPhamAdminItem>();
        await using var conn = await _factory.OpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(sql.ToString(), conn);
        foreach (var (name, value) in ps) cmd.Parameters.AddWithValue(name, value);
        await using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new SanPhamAdminItem
            {
                MaSP = rd.Int("MaSP"),
                TenSP = rd.Str("TenSP"),
                TenDM = rd.Str("TenDM"),
                GiaBan = rd.Dec("GiaBan"),
                TrangThai = rd.Str("TrangThai"),
                LaNoiBat = rd.Bool("LaNoiBat"),
                NgayTao = rd.Dt("NgayTao"),
                NgayCapNhat = rd.DtN("NgayCapNhat"),
                SoBienThe = rd.Int("SoBienThe"),
                TongTon = rd.Int("TongTon"),
                NguoiCapNhatTen = rd.StrN("NguoiCapNhatTen"),
            });
        }
        return list;
    }

    // =====================================================================
    // LẤY DỮ LIỆU CHO FORM SỬA
    // =====================================================================
    public async Task<SanPhamFormViewModel?> LayChoFormAsync(int maSP, CancellationToken ct = default)
    {
        await using var conn = await _factory.OpenConnectionAsync(ct);
        SanPhamFormViewModel? vm = null;

        await using (var cmd = new SqlCommand(
            "SELECT MaSP, TenSP, MoTa, GiaBan, AnhDaiDien, TrangThai, LaNoiBat, MaDM FROM SAN_PHAM WHERE MaSP = @id", conn))
        {
            cmd.Parameters.AddWithValue("@id", maSP);
            await using var rd = await cmd.ExecuteReaderAsync(ct);
            if (await rd.ReadAsync(ct))
            {
                vm = new SanPhamFormViewModel
                {
                    MaSP = rd.Int("MaSP"),
                    TenSP = rd.Str("TenSP"),
                    MoTa = rd.StrN("MoTa"),
                    GiaBan = rd.Dec("GiaBan"),
                    AnhDaiDien = rd.StrN("AnhDaiDien"),
                    TrangThai = rd.Str("TrangThai"),
                    LaNoiBat = rd.Bool("LaNoiBat"),
                    MaDM = rd.Int("MaDM"),
                };
            }
        }
        if (vm == null) return null;

        await using (var cmd = new SqlCommand(
            "SELECT MaBT, SKU, MaMau, MaSize, SoLuongTon FROM BIEN_THE WHERE MaSP = @id ORDER BY MaBT", conn))
        {
            cmd.Parameters.AddWithValue("@id", maSP);
            await using var rd = await cmd.ExecuteReaderAsync(ct);
            while (await rd.ReadAsync(ct))
            {
                vm.BienThes.Add(new BienTheFormRow
                {
                    MaBT = rd.Int("MaBT"),
                    SKU = rd.Str("SKU"),
                    MaMau = rd.Int("MaMau"),
                    MaSize = rd.Int("MaSize"),
                    SoLuongTon = rd.Int("SoLuongTon"),
                });
            }
        }
        return vm;
    }

    // =====================================================================
    // THÊM (transaction: sản phẩm + các biến thể)
    // =====================================================================
    public async Task<int> ThemAsync(SanPhamFormViewModel vm, int? nguoiThucHien, CancellationToken ct = default)
    {
        var rows = vm.BienThes.Where(b => !b.Xoa).ToList();
        KiemTraBienThe(rows);

        await using var conn = await _factory.OpenConnectionAsync(ct);
        using var tx = conn.BeginTransaction();
        try
        {
            int maSP;
            await using (var cmd = new SqlCommand(@"
INSERT INTO SAN_PHAM (TenSP, MoTa, GiaBan, AnhDaiDien, TrangThai, LaNoiBat, MaDM, NguoiTao)
VALUES (@ten, @mota, @gia, @anh, @tt, @hot, @dm, @nguoi);
SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
            {
                cmd.Parameters.AddWithValue("@ten", vm.TenSP.Trim());
                cmd.Parameters.AddWithValue("@mota", SqlText.Val(LamSach(vm.MoTa)));
                cmd.Parameters.AddWithValue("@gia", vm.GiaBan);
                cmd.Parameters.AddWithValue("@anh", SqlText.Val(LamSach(vm.AnhDaiDien)));
                cmd.Parameters.AddWithValue("@tt", TrangThaiSanPham.DangBan);
                cmd.Parameters.AddWithValue("@hot", vm.LaNoiBat);
                cmd.Parameters.AddWithValue("@dm", vm.MaDM);
                cmd.Parameters.AddWithValue("@nguoi", SqlText.Val(nguoiThucHien));
                maSP = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
            }

            foreach (var b in rows)
            {
                await using var cmd = new SqlCommand(@"
INSERT INTO BIEN_THE (SKU, SoLuongTon, MaSP, MaMau, MaSize)
VALUES (@sku, @ton, @sp, @mau, @size);", conn, tx);
                cmd.Parameters.AddWithValue("@sku", b.SKU.Trim());
                cmd.Parameters.AddWithValue("@ton", b.SoLuongTon);
                cmd.Parameters.AddWithValue("@sp", maSP);
                cmd.Parameters.AddWithValue("@mau", b.MaMau);
                cmd.Parameters.AddWithValue("@size", b.MaSize);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            tx.Commit();
            return maSP;
        }
        catch (SqlException ex)
        {
            Huy(tx);
            throw DichLoiSql(ex);
        }
        catch
        {
            Huy(tx);
            throw;
        }
    }

    // =====================================================================
    // SỬA (transaction: sản phẩm + thêm / sửa / xóa biến thể)
    // =====================================================================
    public async Task<bool> CapNhatAsync(SanPhamFormViewModel vm, int? nguoiThucHien, CancellationToken ct = default)
    {
        KiemTraBienThe(vm.BienThes.Where(b => !b.Xoa).ToList());

        await using var conn = await _factory.OpenConnectionAsync(ct);
        using var tx = conn.BeginTransaction();
        try
        {
            int daSua;
            await using (var cmd = new SqlCommand(@"
UPDATE SAN_PHAM
SET TenSP = @ten, MoTa = @mota, GiaBan = @gia, AnhDaiDien = @anh, LaNoiBat = @hot, MaDM = @dm,
    NgayCapNhat = SYSDATETIME(), NguoiCapNhat = @nguoi
WHERE MaSP = @id;", conn, tx))
            {
                cmd.Parameters.AddWithValue("@ten", vm.TenSP.Trim());
                cmd.Parameters.AddWithValue("@mota", SqlText.Val(LamSach(vm.MoTa)));
                cmd.Parameters.AddWithValue("@gia", vm.GiaBan);
                cmd.Parameters.AddWithValue("@anh", SqlText.Val(LamSach(vm.AnhDaiDien)));
                cmd.Parameters.AddWithValue("@hot", vm.LaNoiBat);
                cmd.Parameters.AddWithValue("@dm", vm.MaDM);
                cmd.Parameters.AddWithValue("@nguoi", SqlText.Val(nguoiThucHien));
                cmd.Parameters.AddWithValue("@id", vm.MaSP);
                daSua = await cmd.ExecuteNonQueryAsync(ct);
            }
            if (daSua == 0)
            {
                Huy(tx);
                return false;
            }

            foreach (var b in vm.BienThes)
            {
                if (b.MaBT > 0 && b.Xoa)
                {
                    await XoaBienTheAsync(conn, tx, vm.MaSP, b, ct);
                }
                else if (b.MaBT > 0)
                {
                    await using var cmd = new SqlCommand(@"
UPDATE BIEN_THE SET SKU = @sku, MaMau = @mau, MaSize = @size, SoLuongTon = @ton
WHERE MaBT = @bt AND MaSP = @sp;", conn, tx);
                    cmd.Parameters.AddWithValue("@sku", b.SKU.Trim());
                    cmd.Parameters.AddWithValue("@mau", b.MaMau);
                    cmd.Parameters.AddWithValue("@size", b.MaSize);
                    cmd.Parameters.AddWithValue("@ton", b.SoLuongTon);
                    cmd.Parameters.AddWithValue("@bt", b.MaBT);
                    cmd.Parameters.AddWithValue("@sp", vm.MaSP);
                    await cmd.ExecuteNonQueryAsync(ct);
                }
                else if (!b.Xoa)
                {
                    await using var cmd = new SqlCommand(@"
INSERT INTO BIEN_THE (SKU, SoLuongTon, MaSP, MaMau, MaSize)
VALUES (@sku, @ton, @sp, @mau, @size);", conn, tx);
                    cmd.Parameters.AddWithValue("@sku", b.SKU.Trim());
                    cmd.Parameters.AddWithValue("@ton", b.SoLuongTon);
                    cmd.Parameters.AddWithValue("@sp", vm.MaSP);
                    cmd.Parameters.AddWithValue("@mau", b.MaMau);
                    cmd.Parameters.AddWithValue("@size", b.MaSize);
                    await cmd.ExecuteNonQueryAsync(ct);
                }
            }

            // Sản phẩm phải còn ít nhất một biến thể sau khi sửa
            int conLai;
            await using (var cmd = new SqlCommand("SELECT COUNT(*) FROM BIEN_THE WHERE MaSP = @id", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", vm.MaSP);
                conLai = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
            }
            if (conLai == 0) throw new NghiepVuException("Sản phẩm phải có ít nhất một biến thể.");

            tx.Commit();
            return true;
        }
        catch (SqlException ex)
        {
            Huy(tx);
            throw DichLoiSql(ex);
        }
        catch
        {
            Huy(tx);
            throw;
        }
    }

    /// <summary>Xóa biến thể: không cho xóa nếu đã nằm trong đơn hàng; dòng trong giỏ hàng thì dọn đi.</summary>
    private static async Task XoaBienTheAsync(SqlConnection conn, SqlTransaction tx, int maSP, BienTheFormRow b, CancellationToken ct)
    {
        int trongDon;
        await using (var cmd = new SqlCommand("SELECT COUNT(*) FROM CHI_TIET_DON_HANG WHERE MaBT = @bt", conn, tx))
        {
            cmd.Parameters.AddWithValue("@bt", b.MaBT);
            trongDon = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
        }
        if (trongDon > 0)
            throw new NghiepVuException($"Biến thể {b.SKU} đã có trong đơn hàng nên không thể xóa. Hãy đặt tồn kho về 0 thay vì xóa.");

        await using (var cmd = new SqlCommand("DELETE FROM CHI_TIET_GIO_HANG WHERE MaBT = @bt", conn, tx))
        {
            cmd.Parameters.AddWithValue("@bt", b.MaBT);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        await using (var cmd = new SqlCommand("DELETE FROM BIEN_THE WHERE MaBT = @bt AND MaSP = @sp", conn, tx))
        {
            cmd.Parameters.AddWithValue("@bt", b.MaBT);
            cmd.Parameters.AddWithValue("@sp", maSP);
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }

    // =====================================================================
    // ẨN / HIỆN
    // =====================================================================
    public async Task<bool> DoiTrangThaiAsync(int maSP, string trangThai, int? nguoiThucHien, CancellationToken ct = default)
    {
        if (trangThai != TrangThaiSanPham.DangBan && trangThai != TrangThaiSanPham.NgungBan)
            throw new NghiepVuException("Trạng thái không hợp lệ.");

        await using var conn = await _factory.OpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(@"
UPDATE SAN_PHAM SET TrangThai = @tt, NgayCapNhat = SYSDATETIME(), NguoiCapNhat = @nguoi
WHERE MaSP = @id;", conn);
        cmd.Parameters.AddWithValue("@tt", trangThai);
        cmd.Parameters.AddWithValue("@nguoi", SqlText.Val(nguoiThucHien));
        cmd.Parameters.AddWithValue("@id", maSP);
        return await cmd.ExecuteNonQueryAsync(ct) > 0;
    }

    // =====================================================================
    // XÓA HẲN
    // =====================================================================
    public async Task<XoaSanPhamKetQua> XoaHanAsync(int maSP, CancellationToken ct = default)
    {
        await using var conn = await _factory.OpenConnectionAsync(ct);
        using var tx = conn.BeginTransaction();
        try
        {
            int tonTai;
            await using (var cmd = new SqlCommand("SELECT COUNT(*) FROM SAN_PHAM WHERE MaSP = @id", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", maSP);
                tonTai = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
            }
            if (tonTai == 0)
            {
                Huy(tx);
                return XoaSanPhamKetQua.KhongTimThay;
            }

            int thamChieu;
            await using (var cmd = new SqlCommand(@"
SELECT (SELECT COUNT(*) FROM CHI_TIET_DON_HANG ct JOIN BIEN_THE bt ON bt.MaBT = ct.MaBT WHERE bt.MaSP = @id)
     + (SELECT COUNT(*) FROM DANH_GIA WHERE MaSP = @id);", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", maSP);
                thamChieu = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
            }
            if (thamChieu > 0)
            {
                Huy(tx);
                return XoaSanPhamKetQua.CoThamChieu;
            }

            await using (var cmd = new SqlCommand(@"
DELETE FROM CHI_TIET_GIO_HANG WHERE MaBT IN (SELECT MaBT FROM BIEN_THE WHERE MaSP = @id);
DELETE FROM BIEN_THE WHERE MaSP = @id;
DELETE FROM SAN_PHAM WHERE MaSP = @id;", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", maSP);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            tx.Commit();
            return XoaSanPhamKetQua.DaXoa;
        }
        catch
        {
            Huy(tx);
            throw;
        }
    }

    // =====================================================================
    // CHI TIẾT CHO KHÁCH
    // =====================================================================
    public async Task<SanPhamChiTietViewModel?> LayChiTietAsync(int maSP, bool chiDangBan, CancellationToken ct = default)
    {
        var sql = @"
SELECT sp.MaSP, sp.TenSP, sp.MoTa, sp.GiaBan, sp.AnhDaiDien, sp.TrangThai, sp.LaNoiBat, sp.MaDM,
       sp.NgayTao, sp.NgayCapNhat, dm.TenDM
FROM SAN_PHAM sp
JOIN DANH_MUC dm ON dm.MaDM = sp.MaDM
WHERE sp.MaSP = @id";
        if (chiDangBan) sql += " AND sp.TrangThai = N'Đang bán' AND dm.TrangThai = 1";

        await using var conn = await _factory.OpenConnectionAsync(ct);
        SanPhamChiTietViewModel? vm = null;

        await using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@id", maSP);
            await using var rd = await cmd.ExecuteReaderAsync(ct);
            if (await rd.ReadAsync(ct))
            {
                vm = new SanPhamChiTietViewModel
                {
                    TenDM = rd.Str("TenDM"),
                    SanPham = new SanPham
                    {
                        MaSP = rd.Int("MaSP"),
                        TenSP = rd.Str("TenSP"),
                        MoTa = rd.StrN("MoTa"),
                        GiaBan = rd.Dec("GiaBan"),
                        AnhDaiDien = rd.StrN("AnhDaiDien"),
                        TrangThai = rd.Str("TrangThai"),
                        LaNoiBat = rd.Bool("LaNoiBat"),
                        MaDM = rd.Int("MaDM"),
                        NgayTao = rd.Dt("NgayTao"),
                        NgayCapNhat = rd.DtN("NgayCapNhat"),
                    },
                };
            }
        }
        if (vm == null) return null;

        await using (var cmd = new SqlCommand(@"
SELECT bt.MaBT, bt.SKU, bt.SoLuongTon, bt.MaMau, ms.TenMau, ms.MaHex, bt.MaSize, kc.TenSize
FROM BIEN_THE bt
JOIN MAU_SAC ms ON ms.MaMau = bt.MaMau
JOIN KICH_CO kc ON kc.MaSize = bt.MaSize
WHERE bt.MaSP = @id
ORDER BY ms.TenMau, kc.ThuTu", conn))
        {
            cmd.Parameters.AddWithValue("@id", maSP);
            await using var rd = await cmd.ExecuteReaderAsync(ct);
            while (await rd.ReadAsync(ct))
            {
                vm.BienThes.Add(new BienTheChiTiet
                {
                    MaBT = rd.Int("MaBT"),
                    SKU = rd.Str("SKU"),
                    SoLuongTon = rd.Int("SoLuongTon"),
                    MaMau = rd.Int("MaMau"),
                    TenMau = rd.Str("TenMau"),
                    MaHex = rd.StrN("MaHex"),
                    MaSize = rd.Int("MaSize"),
                    TenSize = rd.Str("TenSize"),
                });
            }
        }
        return vm;
    }

    public async Task<List<ProductListItem>> LaySanPhamLienQuanAsync(int maSP, int soLuong, CancellationToken ct = default)
    {
        const string sql = @"
SELECT TOP (@n) sp.MaSP, sp.TenSP, sp.GiaBan, sp.AnhDaiDien, sp.MaDM, dm.TenDM, sp.LaNoiBat, sp.NgayTao,
       ISNULL((SELECT SUM(b.SoLuongTon) FROM BIEN_THE b WHERE b.MaSP = sp.MaSP), 0) AS TongTon
FROM SAN_PHAM sp
JOIN DANH_MUC dm ON dm.MaDM = sp.MaDM
WHERE sp.MaDM = (SELECT MaDM FROM SAN_PHAM WHERE MaSP = @id)
  AND sp.MaSP <> @id
  AND sp.TrangThai = N'Đang bán' AND dm.TrangThai = 1
ORDER BY sp.NgayTao DESC";
        var list = new List<ProductListItem>();
        await using var conn = await _factory.OpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@n", soLuong);
        cmd.Parameters.AddWithValue("@id", maSP);
        await using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct)) list.Add(SqlText.MapProductListItem(rd));
        return list;
    }

    // =====================================================================
    // Ô CHỌN
    // =====================================================================
    public Task<List<LookupOption>> LayDanhMucAsync(CancellationToken ct = default) => LayLookupAsync(@"
SELECT MaDM AS Id, TenDM + CASE WHEN TrangThai = 1 THEN N'' ELSE N' (ẩn)' END AS Ten, CAST(NULL AS NVARCHAR(10)) AS Phu
FROM DANH_MUC ORDER BY TenDM", ct);

    public Task<List<LookupOption>> LayMauSacAsync(CancellationToken ct = default) => LayLookupAsync(@"
SELECT MaMau AS Id, TenMau AS Ten, MaHex AS Phu FROM MAU_SAC ORDER BY TenMau", ct);

    public Task<List<LookupOption>> LayKichCoAsync(CancellationToken ct = default) => LayLookupAsync(@"
SELECT MaSize AS Id, TenSize AS Ten, CAST(NULL AS NVARCHAR(10)) AS Phu FROM KICH_CO ORDER BY ThuTu", ct);

    private async Task<List<LookupOption>> LayLookupAsync(string sql, CancellationToken ct)
    {
        var list = new List<LookupOption>();
        await using var conn = await _factory.OpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        await using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
            list.Add(new LookupOption { Id = rd.Int("Id"), Ten = rd.Str("Ten"), Phu = rd.StrN("Phu") });
        return list;
    }

    // =====================================================================
    // TIỆN ÍCH
    // =====================================================================
    private static string? LamSach(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static void Huy(SqlTransaction tx)
    {
        try { tx.Rollback(); } catch { /* giao dịch đã đóng */ }
    }

    private static void KiemTraBienThe(List<BienTheFormRow> rows)
    {
        if (rows.Count == 0)
            throw new NghiepVuException("Sản phẩm phải có ít nhất một biến thể (màu + size).");

        if (rows.GroupBy(r => (r.MaMau, r.MaSize)).Any(g => g.Count() > 1))
            throw new NghiepVuException("Có hai biến thể trùng màu và size trong cùng sản phẩm.");

        if (rows.GroupBy(r => r.SKU.Trim().ToUpperInvariant()).Any(g => g.Count() > 1))
            throw new NghiepVuException("Có hai biến thể trùng SKU.");
    }

    /// <summary>Chuyển lỗi ràng buộc của SQL Server thành thông báo dễ hiểu.</summary>
    private static Exception DichLoiSql(SqlException ex) => ex.Number switch
    {
        2627 or 2601 => new NghiepVuException("SKU hoặc tổ hợp màu + size đã tồn tại. Hãy đổi SKU hoặc chọn màu/size khác."),
        547 => new NghiepVuException("Dữ liệu vi phạm ràng buộc (kiểm tra danh mục, giá bán, tồn kho không âm)."),
        _ => ex,
    };
}
