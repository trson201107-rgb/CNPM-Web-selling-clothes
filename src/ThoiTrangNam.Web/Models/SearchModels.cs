namespace ThoiTrangNam.Web.Models;

/// <summary>Điều kiện tìm kiếm, lọc, sắp xếp, phân trang của trang cửa hàng.</summary>
public class ProductSearchCriteria
{
    public string? Q { get; set; }
    public int? MaDM { get; set; }
    public decimal? GiaTu { get; set; }
    public decimal? GiaDen { get; set; }
    /// <summary>"hot" hoặc "new"; rỗng = tất cả.</summary>
    public string? Loai { get; set; }
    public bool ChiConHang { get; set; }
    public string Sort { get; set; } = "moi";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;

    /// <summary>Các cách sắp xếp được phép (giá trị, nhãn hiển thị).</summary>
    public static readonly (string Value, string Label)[] SortOptions =
    {
        ("moi", "Mới nhất"),
        ("gia-tang", "Giá tăng dần"),
        ("gia-giam", "Giá giảm dần"),
        ("ten-az", "Tên A đến Z"),
        ("ten-za", "Tên Z đến A"),
        ("danh-muc", "Theo danh mục"),
    };
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalItems { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
}

public class ShopIndexViewModel
{
    public ProductSearchCriteria Criteria { get; set; } = new();
    public PagedResult<ProductListItem> Result { get; set; } = new();
    public List<LookupOption> DanhMucs { get; set; } = new();
}

public class HomeViewModel
{
    public List<ProductListItem> Hot { get; set; } = new();
    public List<ProductListItem> New { get; set; } = new();
    public List<LookupOption> DanhMucs { get; set; } = new();
}

public class StockAlertItem
{
    public int MaSP { get; set; }
    public string TenSP { get; set; } = "";
    public bool SanPhamDangBan { get; set; }
    public int MaBT { get; set; }
    public string SKU { get; set; } = "";
    public string TenMau { get; set; } = "";
    public string TenSize { get; set; } = "";
    public int SoLuongTon { get; set; }
}

public class StockAlertViewModel
{
    public int NguongSapHet { get; set; } = 5;
    public List<StockAlertItem> HetHang { get; set; } = new();
    public List<StockAlertItem> SapHet { get; set; } = new();
}

public class DashboardStats
{
    public int SoSanPham { get; set; }
    public int SoSanPhamAn { get; set; }
    public int SoDanhMuc { get; set; }
    public int SoBienTheHetHang { get; set; }
    public int SoBienTheSapHet { get; set; }
    public int NguongSapHet { get; set; } = 5;
}
