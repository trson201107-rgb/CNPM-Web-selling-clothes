using ThoiTrangNam.Web.Models;

namespace ThoiTrangNam.Web.Repositories;

/// <summary>Truy vấn danh sách sản phẩm cho khách: tìm kiếm, lọc, sắp xếp, phân trang, Hot, Mới, cảnh báo tồn kho.</summary>
public interface ISanPhamQueryRepository
{
    Task<PagedResult<ProductListItem>> TimKiemAsync(ProductSearchCriteria criteria, CancellationToken ct = default);
    Task<List<ProductListItem>> LayNoiBatAsync(int soLuong, CancellationToken ct = default);
    Task<List<ProductListItem>> LayMoiAsync(int soLuong, CancellationToken ct = default);
    Task<StockAlertViewModel> LayCanhBaoTonKhoAsync(int nguong, CancellationToken ct = default);
    Task<DashboardStats> LayThongKeAsync(int nguong, CancellationToken ct = default);
}
