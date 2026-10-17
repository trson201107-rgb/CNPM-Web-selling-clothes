using System.Text;
using Microsoft.Data.SqlClient;
using ThoiTrangNam.Web.Data;
using ThoiTrangNam.Web.Models;

namespace ThoiTrangNam.Web.Repositories;

public class SanPhamQueryRepository : ISanPhamQueryRepository
{
    private readonly IDbConnectionFactory _factory;
    public SanPhamQueryRepository(IDbConnectionFactory factory) { _factory = factory; }

    private const string CotDanhSach = @"
sp.MaSP, sp.TenSP, sp.GiaBan, sp.AnhDaiDien, sp.MaDM, dm.TenDM, sp.LaNoiBat, sp.NgayTao,
ISNULL((SELECT SUM(b.SoLuongTon) FROM BIEN_THE b WHERE b.MaSP = sp.MaSP), 0) AS TongTon";

    public async Task<PagedResult<ProductListItem>> TimKiemAsync(ProductSearchCriteria c, CancellationToken ct = default)
    {
        var (where, ps) = XayDungDieuKien(c);
        var pageSize = Math.Clamp(c.PageSize, 1, 48);

        await using var conn = await _factory.OpenConnectionAsync(ct);

        // 1) Đếm tổng số kết quả
        int tong;
        await using (var cmd = new SqlCommand(
            "SELECT COUNT(*) FROM SAN_PHAM sp JOIN DANH_MUC dm ON dm.MaDM = sp.MaDM WHERE " + where, conn))
        {
            foreach (var (name, value) in ps) cmd.Parameters.AddWithValue(name, value);
            tong = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
        }

        // 2) Chuẩn hóa số trang rồi lấy đúng trang đó (phân trang ngay trong SQL)
        var tongTrang = tong == 0 ? 1 : (int)Math.Ceiling(tong / (double)pageSize);
        var page = Math.Clamp(c.Page, 1, tongTrang);

        var sql = "SELECT " + CotDanhSach +
                  " FROM SAN_PHAM sp JOIN DANH_MUC dm ON dm.MaDM = sp.MaDM WHERE " + where +
                  " ORDER BY " + SapXep(c.Sort) +
                  " OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";

        var kq = new PagedResult<ProductListItem> { Page = page, PageSize = pageSize, TotalItems = tong };
        await using (var cmd = new SqlCommand(sql, conn))
        {
            foreach (var (name, value) in ps) cmd.Parameters.AddWithValue(name, value);
            cmd.Parameters.AddWithValue("@skip", (page - 1) * pageSize);
            cmd.Parameters.AddWithValue("@take", pageSize);
            await using var rd = await cmd.ExecuteReaderAsync(ct);
            while (await rd.ReadAsync(ct)) kq.Items.Add(SqlText.MapProductListItem(rd));
        }
        return kq;
    }

    public async Task<List<ProductListItem>> LayNoiBatAsync(int soLuong, CancellationToken ct = default)
        => (await TimKiemAsync(new ProductSearchCriteria { Loai = "hot", PageSize = soLuong }, ct)).Items;

    public async Task<List<ProductListItem>> LayMoiAsync(int soLuong, CancellationToken ct = default)
        => (await TimKiemAsync(new ProductSearchCriteria { Loai = "new", PageSize = soLuong }, ct)).Items;

    /// <summary>Chỉ sản phẩm đang bán và danh mục đang hiển thị mới xuất hiện cho khách.</summary>
    private static (string Where, List<(string, object)> Params) XayDungDieuKien(ProductSearchCriteria c)
    {
        var w = new StringBuilder("sp.TrangThai = N'Đang bán' AND dm.TrangThai = 1");
        var ps = new List<(string, object)>();

        if (!string.IsNullOrWhiteSpace(c.Q))
        {
            w.Append(" AND sp.TenSP LIKE @q");
            ps.Add(("@q", "%" + SqlText.EscapeLike(c.Q.Trim()) + "%"));
        }
        if (c.MaDM.HasValue && c.MaDM.Value > 0)
        {
            w.Append(" AND sp.MaDM = @dm");
            ps.Add(("@dm", c.MaDM.Value));
        }
        if (c.GiaTu.HasValue)
        {
            w.Append(" AND sp.GiaBan >= @giaTu");
            ps.Add(("@giaTu", c.GiaTu.Value));
        }
        if (c.GiaDen.HasValue)
        {
            w.Append(" AND sp.GiaBan <= @giaDen");
            ps.Add(("@giaDen", c.GiaDen.Value));
        }
        if (c.Loai == "hot") w.Append(" AND sp.LaNoiBat = 1");
        else if (c.Loai == "new") w.Append(" AND sp.NgayTao >= DATEADD(DAY, -30, SYSDATETIME())");

        if (c.ChiConHang)
            w.Append(" AND EXISTS (SELECT 1 FROM BIEN_THE b WHERE b.MaSP = sp.MaSP AND b.SoLuongTon > 0)");

        return (w.ToString(), ps);
    }

    /// <summary>Danh sách trắng các kiểu sắp xếp: không bao giờ nối chuỗi người dùng vào ORDER BY.</summary>
    private static string SapXep(string? sort) => sort switch
    {
        "gia-tang" => "sp.GiaBan ASC, sp.MaSP DESC",
        "gia-giam" => "sp.GiaBan DESC, sp.MaSP DESC",
        "ten-az" => "sp.TenSP ASC, sp.MaSP DESC",
        "ten-za" => "sp.TenSP DESC, sp.MaSP DESC",
        "danh-muc" => "dm.TenDM ASC, sp.TenSP ASC",
        _ => "sp.NgayTao DESC, sp.MaSP DESC",
    };

    public async Task<StockAlertViewModel> LayCanhBaoTonKhoAsync(int nguong, CancellationToken ct = default)
    {
        nguong = Math.Clamp(nguong, 1, 1000);
        var vm = new StockAlertViewModel { NguongSapHet = nguong };
        await using var conn = await _factory.OpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(@"
SELECT sp.MaSP, sp.TenSP, sp.TrangThai, bt.MaBT, bt.SKU, ms.TenMau, kc.TenSize, bt.SoLuongTon
FROM BIEN_THE bt
JOIN SAN_PHAM sp ON sp.MaSP = bt.MaSP
JOIN MAU_SAC ms ON ms.MaMau = bt.MaMau
JOIN KICH_CO kc ON kc.MaSize = bt.MaSize
WHERE bt.SoLuongTon <= @n
ORDER BY bt.SoLuongTon ASC, sp.TenSP, kc.ThuTu", conn);
        cmd.Parameters.AddWithValue("@n", nguong);
        await using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
        {
            var it = new StockAlertItem
            {
                MaSP = rd.Int("MaSP"),
                TenSP = rd.Str("TenSP"),
                SanPhamDangBan = rd.Str("TrangThai") == TrangThaiSanPham.DangBan,
                MaBT = rd.Int("MaBT"),
                SKU = rd.Str("SKU"),
                TenMau = rd.Str("TenMau"),
                TenSize = rd.Str("TenSize"),
                SoLuongTon = rd.Int("SoLuongTon"),
            };
            if (it.SoLuongTon <= 0) vm.HetHang.Add(it); else vm.SapHet.Add(it);
        }
        return vm;
    }

    public async Task<DashboardStats> LayThongKeAsync(int nguong, CancellationToken ct = default)
    {
        nguong = Math.Clamp(nguong, 1, 1000);
        await using var conn = await _factory.OpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(@"
SELECT (SELECT COUNT(*) FROM SAN_PHAM) AS SoSanPham,
       (SELECT COUNT(*) FROM SAN_PHAM WHERE TrangThai = N'Ngừng bán') AS SoSanPhamAn,
       (SELECT COUNT(*) FROM DANH_MUC) AS SoDanhMuc,
       (SELECT COUNT(*) FROM BIEN_THE WHERE SoLuongTon = 0) AS SoHetHang,
       (SELECT COUNT(*) FROM BIEN_THE WHERE SoLuongTon BETWEEN 1 AND @n) AS SoSapHet", conn);
        cmd.Parameters.AddWithValue("@n", nguong);
        await using var rd = await cmd.ExecuteReaderAsync(ct);
        var s = new DashboardStats { NguongSapHet = nguong };
        if (await rd.ReadAsync(ct))
        {
            s.SoSanPham = rd.Int("SoSanPham");
            s.SoSanPhamAn = rd.Int("SoSanPhamAn");
            s.SoDanhMuc = rd.Int("SoDanhMuc");
            s.SoBienTheHetHang = rd.Int("SoHetHang");
            s.SoBienTheSapHet = rd.Int("SoSapHet");
        }
        return s;
    }
}
