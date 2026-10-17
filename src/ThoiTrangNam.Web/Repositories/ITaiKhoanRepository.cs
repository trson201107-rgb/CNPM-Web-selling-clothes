using ThoiTrangNam.Web.Models;

namespace ThoiTrangNam.Web.Repositories;

public interface ITaiKhoanRepository
{
    Task<TaiKhoan?> LayTheoEmailAsync(string email, CancellationToken ct = default);
    /// <summary>Tạo tài khoản khách hàng và giỏ hàng trống trong một transaction.</summary>
    Task<int> TaoKhachHangAsync(string hoTen, string email, string matKhauHash, string soDienThoai, CancellationToken ct = default);
    /// <summary>Tăng số lần đăng nhập sai; từ lần thứ 5 khóa 15 phút.</summary>
    Task GhiNhanDangNhapSaiAsync(int maTK, CancellationToken ct = default);
    Task DatLaiDangNhapSaiAsync(int maTK, CancellationToken ct = default);
}
