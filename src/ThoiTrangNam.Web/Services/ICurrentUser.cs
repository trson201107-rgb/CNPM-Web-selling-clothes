using System.Security.Claims;

namespace ThoiTrangNam.Web.Services;

/// <summary>Người đang đăng nhập, dùng để ghi "người tạo / người cập nhật".</summary>
public interface ICurrentUser
{
    int? MaTK { get; }
    string? HoTen { get; }
    bool LaAdmin { get; }
    bool LaNhanVien { get; }
}

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;
    public CurrentUser(IHttpContextAccessor accessor) { _accessor = accessor; }

    private ClaimsPrincipal? User => _accessor.HttpContext?.User;

    public int? MaTK
    {
        get
        {
            var v = User?.FindFirst("MaTK")?.Value;
            return int.TryParse(v, out var id) ? id : null;
        }
    }
    public string? HoTen => User?.Identity?.Name;
    public bool LaAdmin => User?.IsInRole("Admin") == true;
    public bool LaNhanVien => User?.IsInRole("Staff") == true;
}
