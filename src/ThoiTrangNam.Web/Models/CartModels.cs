namespace ThoiTrangNam.Web.Models;

public class CartLine
{
    public int MaBT { get; set; }
    public int MaSP { get; set; }
    public string TenSP { get; set; } = "";
    public string? AnhDaiDien { get; set; }
    public string TenMau { get; set; } = "";
    public string TenSize { get; set; } = "";
    public string SKU { get; set; } = "";
    public decimal GiaBan { get; set; }
    public int SoLuong { get; set; }
    public int SoLuongTon { get; set; }
    public bool DangBan { get; set; }
    public decimal ThanhTien => GiaBan * SoLuong;
    public bool VuotTon => SoLuong > SoLuongTon;
}

public class CartViewModel
{
    public List<CartLine> Lines { get; set; } = new();
    public decimal TongTien => Lines.Where(l => l.DangBan).Sum(l => l.ThanhTien);
    public int TongSoLuong => Lines.Sum(l => l.SoLuong);
}

public enum CartResult { ThanhCong, KhongTimThay, HetHang, VuotTon }
