using ThoiTrangNam.Web.Models;

namespace ThoiTrangNam.Web.Repositories;

public interface ICartRepository
{
    Task<CartViewModel> LayGioHangAsync(int maTK, CancellationToken ct = default);
    Task<int> DemSoLuongAsync(int maTK, CancellationToken ct = default);
    Task<CartResult> ThemAsync(int maTK, int maBT, int soLuong, CancellationToken ct = default);
    Task<CartResult> CapNhatSoLuongAsync(int maTK, int maBT, int soLuong, CancellationToken ct = default);
    Task XoaAsync(int maTK, int maBT, CancellationToken ct = default);
}
