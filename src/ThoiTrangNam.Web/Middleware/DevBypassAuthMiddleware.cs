using System.Security.Claims;

namespace ThoiTrangNam.Web.Middleware;

/// <summary>
/// CHỈ DÙNG KHI PHÁT TRIỂN: nếu cấu hình Auth:DevBypass = true và môi trường là Development,
/// tự coi như đã đăng nhập bằng một tài khoản Admin để thử trang quản trị khi chưa có chức năng đăng nhập.
/// Ở Production middleware này không bao giờ chạy.
/// </summary>
public class DevBypassAuthMiddleware
{
    private readonly RequestDelegate _next;
    public DevBypassAuthMiddleware(RequestDelegate next) { _next = next; }

    public async Task InvokeAsync(HttpContext context, IConfiguration config, IHostEnvironment env)
    {
        if (env.IsDevelopment()
            && config.GetValue<bool>("Auth:DevBypass")
            && context.User.Identity?.IsAuthenticated != true)
        {
            var maTK = config.GetValue<int?>("Auth:DevUserId") ?? 4;
            var identity = new ClaimsIdentity(new[]
            {
                new Claim("MaTK", maTK.ToString()),
                new Claim(ClaimTypes.Name, "Dev Admin"),
                new Claim(ClaimTypes.Role, "Admin"),
            }, "DevBypass");
            context.User = new ClaimsPrincipal(identity);
        }
        await _next(context);
    }
}
