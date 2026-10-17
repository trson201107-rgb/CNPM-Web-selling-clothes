namespace ThoiTrangNam.Web.Models;

/// <summary>Trạng thái sản phẩm (khớp ràng buộc CHECK của bảng SAN_PHAM).</summary>
public static class TrangThaiSanPham
{
    public const string DangBan = "Đang bán";    // hiển thị cho khách
    public const string NgungBan = "Ngừng bán";  // đã ẩn
}

/// <summary>Entity ứng với bảng SAN_PHAM.</summary>
public class SanPham
{
    public int MaSP { get; set; }
    public string TenSP { get; set; } = "";
    public string? MoTa { get; set; }
    public decimal GiaBan { get; set; }
    public string? AnhDaiDien { get; set; }
    public string TrangThai { get; set; } = TrangThaiSanPham.DangBan;
    public bool LaNoiBat { get; set; }
    public int MaDM { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public int? NguoiTao { get; set; }
    public int? NguoiCapNhat { get; set; }
}

/// <summary>Entity ứng với bảng BIEN_THE (mỗi sản phẩm có nhiều biến thể màu + size).</summary>
public class BienThe
{
    public int MaBT { get; set; }
    public string SKU { get; set; } = "";
    public int SoLuongTon { get; set; }
    public int MaSP { get; set; }
    public int MaMau { get; set; }
    public int MaSize { get; set; }
}
