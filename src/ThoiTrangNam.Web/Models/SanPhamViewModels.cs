using System.ComponentModel.DataAnnotations;

namespace ThoiTrangNam.Web.Models;

/// <summary>Một lựa chọn trong ô chọn (danh mục, màu, size). Phu: thông tin phụ, ví dụ mã màu hex.</summary>
public class LookupOption
{
    public int Id { get; set; }
    public string Ten { get; set; } = "";
    public string? Phu { get; set; }
}

public class BienTheFormRow
{
    public int MaBT { get; set; }

    [Required(ErrorMessage = "Nhập SKU cho biến thể")]
    [StringLength(30, ErrorMessage = "SKU tối đa 30 ký tự")]
    public string SKU { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Chọn màu")]
    public int MaMau { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Chọn size")]
    public int MaSize { get; set; }

    [Range(0, 1000000, ErrorMessage = "Tồn kho phải từ 0 trở lên")]
    public int SoLuongTon { get; set; }

    /// <summary>Đánh dấu xóa biến thể (chỉ có tác dụng khi sửa sản phẩm).</summary>
    public bool Xoa { get; set; }
}

public class SanPhamFormViewModel
{
    public int MaSP { get; set; }

    [Required(ErrorMessage = "Nhập tên sản phẩm")]
    [StringLength(150, ErrorMessage = "Tên tối đa 150 ký tự")]
    public string TenSP { get; set; } = "";

    [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự")]
    public string? MoTa { get; set; }

    [Range(0, 1000000000, ErrorMessage = "Giá bán phải từ 0 trở lên")]
    public decimal GiaBan { get; set; }

    [StringLength(255, ErrorMessage = "Đường dẫn ảnh tối đa 255 ký tự")]
    public string? AnhDaiDien { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Chọn danh mục")]
    public int MaDM { get; set; }

    public bool LaNoiBat { get; set; }

    public string TrangThai { get; set; } = TrangThaiSanPham.DangBan;

    public List<BienTheFormRow> BienThes { get; set; } = new();

    // Dữ liệu cho các ô chọn (không nhận từ form, controller nạp lại mỗi lần hiển thị)
    public List<LookupOption> DanhMucs { get; set; } = new();
    public List<LookupOption> Maus { get; set; } = new();
    public List<LookupOption> Sizes { get; set; } = new();
}

public class SanPhamAdminItem
{
    public int MaSP { get; set; }
    public string TenSP { get; set; } = "";
    public string TenDM { get; set; } = "";
    public decimal GiaBan { get; set; }
    public string TrangThai { get; set; } = "";
    public bool LaNoiBat { get; set; }
    public int SoBienThe { get; set; }
    public int TongTon { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public string? NguoiCapNhatTen { get; set; }
    public bool DangBan => TrangThai == TrangThaiSanPham.DangBan;
}

public class BienTheChiTiet
{
    public int MaBT { get; set; }
    public string SKU { get; set; } = "";
    public int SoLuongTon { get; set; }
    public int MaMau { get; set; }
    public string TenMau { get; set; } = "";
    public string? MaHex { get; set; }
    public int MaSize { get; set; }
    public string TenSize { get; set; } = "";
}

public class SanPhamChiTietViewModel
{
    public SanPham SanPham { get; set; } = new();
    public string TenDM { get; set; } = "";
    public List<BienTheChiTiet> BienThes { get; set; } = new();
    public List<ProductListItem> LienQuan { get; set; } = new();
    public int TongTon => BienThes.Sum(b => b.SoLuongTon);
    public bool ConHang => TongTon > 0;
    public bool DaAn => SanPham.TrangThai != TrangThaiSanPham.DangBan;
}

/// <summary>Sản phẩm hiển thị dạng thẻ ở trang chủ, cửa hàng, sản phẩm liên quan.</summary>
public class ProductListItem
{
    public int MaSP { get; set; }
    public string TenSP { get; set; } = "";
    public decimal GiaBan { get; set; }
    public string? AnhDaiDien { get; set; }
    public int MaDM { get; set; }
    public string TenDM { get; set; } = "";
    public bool LaNoiBat { get; set; }
    public DateTime NgayTao { get; set; }
    public int TongTon { get; set; }
    public bool LaMoi => NgayTao >= DateTime.Now.AddDays(-30);
    public bool HetHang => TongTon <= 0;
}

public enum XoaSanPhamKetQua { DaXoa, KhongTimThay, CoThamChieu }
