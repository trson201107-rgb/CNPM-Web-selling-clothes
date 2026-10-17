using Microsoft.Data.SqlClient;
using ThoiTrangNam.Web.Data;
using ThoiTrangNam.Web.Models;
using ThoiTrangNam.Web.Services;

namespace ThoiTrangNam.Web.Repositories;

public class DanhMucRepository : IDanhMucRepository
{
    private readonly IDbConnectionFactory _factory;
    public DanhMucRepository(IDbConnectionFactory factory) { _factory = factory; }

    public async Task<List<DanhMucAdminItem>> LayTatCaAdminAsync(CancellationToken ct = default)
    {
        const string sql = @"
SELECT dm.MaDM, dm.TenDM, dm.MoTa, dm.TrangThai, dm.NgayCapNhat,
       (SELECT COUNT(*) FROM SAN_PHAM sp WHERE sp.MaDM = dm.MaDM) AS SoSanPham,
       tk.HoTen AS NguoiCapNhatTen
FROM DANH_MUC dm
LEFT JOIN TAI_KHOAN tk ON tk.MaTK = dm.NguoiCapNhat
ORDER BY dm.TenDM";
        var list = new List<DanhMucAdminItem>();
        await using var conn = await _factory.OpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        await using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new DanhMucAdminItem
            {
                MaDM = rd.Int("MaDM"),
                TenDM = rd.Str("TenDM"),
                MoTa = rd.StrN("MoTa"),
                TrangThai = rd.Bool("TrangThai"),
                SoSanPham = rd.Int("SoSanPham"),
                NgayCapNhat = rd.DtN("NgayCapNhat"),
                NguoiCapNhatTen = rd.StrN("NguoiCapNhatTen"),
            });
        }
        return list;
    }

    public async Task<DanhMuc?> LayTheoIdAsync(int maDM, CancellationToken ct = default)
    {
        await using var conn = await _factory.OpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(@"
SELECT MaDM, TenDM, MoTa, TrangThai, NgayTao, NgayCapNhat, NguoiTao, NguoiCapNhat
FROM DANH_MUC WHERE MaDM = @id", conn);
        cmd.Parameters.AddWithValue("@id", maDM);
        await using var rd = await cmd.ExecuteReaderAsync(ct);
        if (!await rd.ReadAsync(ct)) return null;
        return new DanhMuc
        {
            MaDM = rd.Int("MaDM"),
            TenDM = rd.Str("TenDM"),
            MoTa = rd.StrN("MoTa"),
            TrangThai = rd.Bool("TrangThai"),
            NgayTao = rd.Dt("NgayTao"),
            NgayCapNhat = rd.DtN("NgayCapNhat"),
            NguoiTao = rd.IntN("NguoiTao"),
            NguoiCapNhat = rd.IntN("NguoiCapNhat"),
        };
    }

    public async Task<int> ThemAsync(DanhMuc dm, int? nguoiThucHien, CancellationToken ct = default)
    {
        try
        {
            await using var conn = await _factory.OpenConnectionAsync(ct);
            await using var cmd = new SqlCommand(@"
INSERT INTO DANH_MUC (TenDM, MoTa, NguoiTao) VALUES (@ten, @mota, @nguoi);
SELECT CAST(SCOPE_IDENTITY() AS INT);", conn);
            cmd.Parameters.AddWithValue("@ten", dm.TenDM.Trim());
            cmd.Parameters.AddWithValue("@mota", SqlText.Val(string.IsNullOrWhiteSpace(dm.MoTa) ? null : dm.MoTa.Trim()));
            cmd.Parameters.AddWithValue("@nguoi", SqlText.Val(nguoiThucHien));
            return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new NghiepVuException("Tên danh mục đã tồn tại.");
        }
    }

    public async Task<bool> CapNhatAsync(DanhMuc dm, int? nguoiThucHien, CancellationToken ct = default)
    {
        try
        {
            await using var conn = await _factory.OpenConnectionAsync(ct);
            await using var cmd = new SqlCommand(@"
UPDATE DANH_MUC
SET TenDM = @ten, MoTa = @mota, NgayCapNhat = SYSDATETIME(), NguoiCapNhat = @nguoi
WHERE MaDM = @id", conn);
            cmd.Parameters.AddWithValue("@ten", dm.TenDM.Trim());
            cmd.Parameters.AddWithValue("@mota", SqlText.Val(string.IsNullOrWhiteSpace(dm.MoTa) ? null : dm.MoTa.Trim()));
            cmd.Parameters.AddWithValue("@nguoi", SqlText.Val(nguoiThucHien));
            cmd.Parameters.AddWithValue("@id", dm.MaDM);
            return await cmd.ExecuteNonQueryAsync(ct) > 0;
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new NghiepVuException("Tên danh mục đã tồn tại.");
        }
    }

    public async Task<bool> DoiTrangThaiAsync(int maDM, bool hienThi, int? nguoiThucHien, CancellationToken ct = default)
    {
        await using var conn = await _factory.OpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(@"
UPDATE DANH_MUC SET TrangThai = @tt, NgayCapNhat = SYSDATETIME(), NguoiCapNhat = @nguoi
WHERE MaDM = @id", conn);
        cmd.Parameters.AddWithValue("@tt", hienThi);
        cmd.Parameters.AddWithValue("@nguoi", SqlText.Val(nguoiThucHien));
        cmd.Parameters.AddWithValue("@id", maDM);
        return await cmd.ExecuteNonQueryAsync(ct) > 0;
    }

    public async Task<XoaDanhMucKetQua> XoaAsync(int maDM, CancellationToken ct = default)
    {
        await using var conn = await _factory.OpenConnectionAsync(ct);
        int soSanPham;
        await using (var cmd = new SqlCommand("SELECT COUNT(*) FROM SAN_PHAM WHERE MaDM = @id", conn))
        {
            cmd.Parameters.AddWithValue("@id", maDM);
            soSanPham = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
        }
        if (soSanPham > 0) return XoaDanhMucKetQua.CoSanPham;

        try
        {
            await using var cmd = new SqlCommand("DELETE FROM DANH_MUC WHERE MaDM = @id", conn);
            cmd.Parameters.AddWithValue("@id", maDM);
            var n = await cmd.ExecuteNonQueryAsync(ct);
            return n > 0 ? XoaDanhMucKetQua.DaXoa : XoaDanhMucKetQua.KhongTimThay;
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return XoaDanhMucKetQua.CoSanPham;
        }
    }

    public async Task<List<LookupOption>> LayHienThiAsync(CancellationToken ct = default)
    {
        var list = new List<LookupOption>();
        await using var conn = await _factory.OpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(@"
SELECT MaDM AS Id, TenDM AS Ten, CAST(NULL AS NVARCHAR(10)) AS Phu
FROM DANH_MUC WHERE TrangThai = 1 ORDER BY TenDM", conn);
        await using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
            list.Add(new LookupOption { Id = rd.Int("Id"), Ten = rd.Str("Ten"), Phu = rd.StrN("Phu") });
        return list;
    }
}
