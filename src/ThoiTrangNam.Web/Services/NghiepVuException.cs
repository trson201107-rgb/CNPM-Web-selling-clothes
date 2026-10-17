namespace ThoiTrangNam.Web.Services;

/// <summary>Lỗi nghiệp vụ có thông báo thân thiện, hiển thị thẳng cho người dùng (trùng SKU, xóa dữ liệu đang được dùng...).</summary>
public class NghiepVuException : Exception
{
    public NghiepVuException(string message) : base(message) { }
}
