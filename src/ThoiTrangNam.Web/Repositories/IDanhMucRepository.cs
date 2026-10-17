using ThoiTrangNam.Web.Models;

namespace ThoiTrangNam.Web.Repositories;

public interface IDanhMucRepository
{
    Task<List<DanhMucAdminItem>> LayTatCaAdminAsync(CancellationToken ct = default);
    Task<DanhMuc?> LayTheoIdAsync(int maDM, CancellationToken ct = default);
    Task<int> ThemAsync(DanhMuc dm, int? nguoiThucHien, CancellationToken ct = default);
    Task<bool> CapNhatAsync(DanhMuc dm, int? nguoiThucHien, CancellationToken ct = default);
    /// <summary>Ẩn / hiện danh mục. Danh mục ẩn kéo theo sản phẩm của nó không hiện cho khách.</summary>
    Task<bool> DoiTrangThaiAsync(int maDM, bool hienThi, int? nguoiThucHien, CancellationToken ct = default);
    /// <summary>Xóa hẳn, chỉ khi danh mục không còn sản phẩm nào.</summary>
    Task<XoaDanhMucKetQua> XoaAsync(int maDM, CancellationToken ct = default);
    /// <summary>Danh mục đang hiển thị, dùng cho ô lọc ở trang khách.</summary>
    Task<List<LookupOption>> LayHienThiAsync(CancellationToken ct = default);
}
