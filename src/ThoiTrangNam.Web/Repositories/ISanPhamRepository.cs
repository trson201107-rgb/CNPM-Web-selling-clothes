using ThoiTrangNam.Web.Models;

namespace ThoiTrangNam.Web.Repositories;

public interface ISanPhamRepository
{
    // ----- Quản trị (Admin / Staff) -----
    Task<List<SanPhamAdminItem>> LayDanhSachAdminAsync(string? tuKhoa, string? trangThai, CancellationToken ct = default);
    Task<SanPhamFormViewModel?> LayChoFormAsync(int maSP, CancellationToken ct = default);
    /// <summary>Thêm sản phẩm kèm biến thể trong MỘT transaction. Trả về mã sản phẩm mới.</summary>
    Task<int> ThemAsync(SanPhamFormViewModel vm, int? nguoiThucHien, CancellationToken ct = default);
    Task<bool> CapNhatAsync(SanPhamFormViewModel vm, int? nguoiThucHien, CancellationToken ct = default);
    /// <summary>Ẩn / hiện sản phẩm (đổi TrangThai). Dữ liệu vẫn còn trong CSDL.</summary>
    Task<bool> DoiTrangThaiAsync(int maSP, string trangThai, int? nguoiThucHien, CancellationToken ct = default);
    /// <summary>Xóa hẳn khỏi CSDL, chỉ khi không còn đơn hàng hoặc đánh giá tham chiếu.</summary>
    Task<XoaSanPhamKetQua> XoaHanAsync(int maSP, CancellationToken ct = default);

    // ----- Hiển thị cho khách -----
    Task<SanPhamChiTietViewModel?> LayChiTietAsync(int maSP, bool chiDangBan, CancellationToken ct = default);
    Task<List<ProductListItem>> LaySanPhamLienQuanAsync(int maSP, int soLuong, CancellationToken ct = default);

    // ----- Dữ liệu cho ô chọn -----
    Task<List<LookupOption>> LayDanhMucAsync(CancellationToken ct = default);
    Task<List<LookupOption>> LayMauSacAsync(CancellationToken ct = default);
    Task<List<LookupOption>> LayKichCoAsync(CancellationToken ct = default);
}
